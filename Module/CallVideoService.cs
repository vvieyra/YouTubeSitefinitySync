using Google.GData.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Telerik.Microsoft.Practices.EnterpriseLibrary.Logging;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data.ContentLinks;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Model.ContentLinks;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Utilities.TypeConverters;
using WebVideoSync.Configuration;
using draw = System.Drawing;
using goog = Google.YouTube;

namespace WebVideoSync.Module
{
	public class CallVideoService
	{
		public Uri ServiceUri { get; set; }
		WebVideoSyncConfig config = Config.Get<WebVideoSyncConfig>();
		DynamicModuleManager dynamicModuleManager;
		Type syncedVideoType;

		public IEnumerable<goog.Video> GetVideos()
		{
			goog.YouTubeRequest request = new goog.YouTubeRequest(new goog.YouTubeRequestSettings("Sitefinity", ""));
			Feed<goog.Video> videoFeed = request.Get<goog.Video>(ServiceUri);
			var videoEntries = videoFeed.Entries.OrderBy(k=>k.Updated);
			return videoEntries;
		}
		public CallVideoService(string Url) 
		{
			this.ServiceUri = new Uri(Url);
			var providerName = "OpenAccessProvider";
			dynamicModuleManager = DynamicModuleManager.GetManager(providerName);
			dynamicModuleManager.Provider.SuppressSecurityChecks = true;
			syncedVideoType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.SyncedVideos.SyncedVideo");

		}
		protected Guid SaveImageToSitefinity(string Url, DynamicContent syncedVideoItem, string title)
		{
			
			try
			{
				string extension;
				var imageToSave = ReturnImageStreamFromUrl(Url, out extension);
				LibrariesManager librariesManager = LibrariesManager.GetManager();

				librariesManager.Provider.SuppressSecurityChecks = true;

				//The album post is created as master. The masterImageId is assigned to the master version.
				Image image = librariesManager.CreateImage();

				//Set the parent album.
				Album album = librariesManager.GetAlbums().Where(i => i.Title == config.ThumbLibName).SingleOrDefault();
				if (album == null)
				{
					CreateNewAlbum(out album, config.ThumbLibName, librariesManager);
				}
				image.Parent = album;

				//Set the properties of the image.
				image.Title = title;
				image.DateCreated = DateTime.UtcNow;
				image.PublicationDate = DateTime.UtcNow;
				image.LastModified = DateTime.UtcNow;

				int intAppend = 0;
				Image imgToTest;
				string testName;

				do // Test url-name to verify doesnt exist
				{
					intAppend++;
					testName = Regex.Replace(title.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-") + "-" + intAppend;
					imgToTest = librariesManager.GetImages().Where(i => i.UrlName == testName).FirstOrDefault();
				}
				while (imgToTest != null);
				image.UrlName = testName;

				//Upload the image file.
				librariesManager.Upload(image, imageToSave, extension);
				image.SetWorkflowStatus(librariesManager.Provider.ApplicationName, "Published");
				librariesManager.Lifecycle.Publish(image);
				librariesManager.SaveChanges();
				librariesManager.Provider.SuppressSecurityChecks = false;
				return image.Id;
			}
			catch (Exception e)
			{
				Logger.Writer.Write("The WebVideoSync process failed on video id: " + syncedVideoItem.GetValue("YouTubeVidId") + ". The title of the video that failed was: \"" + title + "\". Stack trace below:\n");
				Logger.Writer.Write(e.StackTrace);
				return Guid.Empty;		
			}
		}
		protected void CreateNewAlbum(out Album album, string AlbumTitle, LibrariesManager librariesManager)
		{
			album = librariesManager.CreateAlbum(Guid.NewGuid());

			//Set the properties of the album.
			album.Title = AlbumTitle;
			album.DateCreated = DateTime.UtcNow;
			album.LastModified = DateTime.UtcNow;
			album.UrlName = Regex.Replace(AlbumTitle.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");

			//Save the changes.
			librariesManager.SaveChanges();
		}

		protected Stream ReturnImageStreamFromUrl(string Url, out string extension)
		{
			// Create request/response for image
			HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(Url);
			WebResponse imageResponse = imageRequest.GetResponse();
			Stream responseStream = imageResponse.GetResponseStream();
			
			// Save image to memory stream
			var image = draw.Image.FromStream(responseStream);
			MemoryStream s = new MemoryStream();
			image.Save(s, draw.Imaging.ImageFormat.Jpeg);
			s.Position = 0;
			
			// Return necessary values
			extension = imageResponse.ContentType.Replace("image/", ".").Replace("jpeg", "jpg");
			return s;
		}
		public void createUpdateVideo(DynamicContent sitefinityVideo, goog.Video vid)
		{
			DynamicContent syncedVideoItem;
			// Create new video
			if (sitefinityVideo == null)
			{
				syncedVideoItem = dynamicModuleManager.CreateDataItem(syncedVideoType);
				syncedVideoItem.SetString("UrlName", Regex.Replace(vid.VideoId.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-"));
				//syncedVideoItem.SetValue("Owner", user.Id);
				
			}
			else // Check-out existing video
			{
				syncedVideoItem = dynamicModuleManager.Lifecycle.CheckOut(sitefinityVideo) as DynamicContent;
			}

			// Add/Update fields
			syncedVideoItem.SetValue("PublicationDate", vid.YouTubeEntry.Published);
			syncedVideoItem.SetValue("YouTubeVidId", vid.VideoId);
			syncedVideoItem.SetValue("Title", vid.Title);
			syncedVideoItem.SetValue("Duration", vid.Media.Duration.Seconds);

			// Delete existing thumbnail if it already exists.
			if (syncedVideoItem.GetValue<ContentLink[]>("Thumbnail") != null)
			{
				Guid ThumbID = syncedVideoItem.GetValue<ContentLink[]>("Thumbnail")[0].ChildItemId;
				LibrariesManager librariesManager = LibrariesManager.GetManager();
				Image thumbToDelete = librariesManager.GetImages().Where(i => i.Id == ThumbID).FirstOrDefault();
				if (thumbToDelete != null)
				{
					librariesManager.DeleteImage(thumbToDelete);
					librariesManager.SaveChanges();
				}
			}

			// Create and save Thumnail
			if (vid.Media.Thumbnails != null && vid.Media.Thumbnails.Count > 0)
			{
				var thumb = vid.Media.Thumbnails.OrderBy(k => k.Width).Reverse().FirstOrDefault();
				Guid imgId = SaveImageToSitefinity(thumb.Url, syncedVideoItem, vid.Title);
				ContentLinksManager manager = ContentLinksManager.GetManager();

				// Make sure to test when there are no content links in the Thumbnail field.
				if (syncedVideoItem.GetValue<ContentLink[]>("Thumbnail") != null)
				{
					syncedVideoItem.ClearImages("Thumbnail");
					syncedVideoItem.SetValue("Thumbnail", null);
				}
				if (imgId != Guid.Empty)
					syncedVideoItem.AddImage("Thumbnail", imgId);
			}
			// Save new video
			if (sitefinityVideo == null)
			{
				syncedVideoItem.SetWorkflowStatus(dynamicModuleManager.Provider.ApplicationName, "Draft");
				dynamicModuleManager.SaveChanges();
				ILifecycleDataItem publishedSyncedVideoItem = dynamicModuleManager.Lifecycle.Publish(syncedVideoItem);
				syncedVideoItem.SetWorkflowStatus(dynamicModuleManager.Provider.ApplicationName, "Published");
				dynamicModuleManager.SaveChanges();
			}
			else // Check-in pre-existing video
			{
				ILifecycleDataItem checkInSyncedVideoItem = dynamicModuleManager.Lifecycle.CheckIn(syncedVideoItem);
				dynamicModuleManager.Lifecycle.Publish(checkInSyncedVideoItem);
				dynamicModuleManager.SaveChanges();
			}
		}
	}
}
