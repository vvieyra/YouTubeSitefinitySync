using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Scheduling;
using Telerik.Sitefinity.Utilities.TypeConverters;
using WebVideoSync.Configuration;
using google = Google.YouTube;

namespace WebVideoSync.Module
{
	class ScheduledVideoSync : ScheduledTask
	{
		DynamicModuleManager dynamicModuleManager;
		WebVideoSyncConfig webVidConfig = Config.Get<WebVideoSyncConfig>();
		public override string TaskName
		{
			get
			{
				return "WebVideoSync.Module.ScheduledVideoSync, WebVideoSync";
			}
		}
		public ScheduledVideoSync()
		{
			// Guid associated only to this module
			this.Key = "D92F7C6F-A121-4f15-87F7-04ABBE0D7BE0";

			#region Next execute time for task
			{
				int numberOfDays = webVidConfig.SyncFreq;
				DateTime configLastRun;
				if (DateTime.TryParse(webVidConfig.LastRuntime, out configLastRun) && numberOfDays > 0)
				{
					ExecuteTime = configLastRun.AddDays(numberOfDays);
					//ExecuteTime = configLastRun.AddMinutes(numberOfDays);
				}
				else
				{
					this.ExecuteTime = DateTime.Now.ToUniversalTime();
				}
			} 
			#endregion
		}
		public override void ExecuteTask()
		{
			//If no url is provided, die
			if (webVidConfig.FeedUri.IsNullOrWhitespace())
				return;
			#region Execute Task
			{
				Type syncedVideoType;
				var providerName = "OpenAccessProvider";
				dynamicModuleManager = DynamicModuleManager.GetManager(providerName);
				syncedVideoType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.SyncedVideos.SyncedVideo");
				List<google.Video> videos;
				DateTime lastRan;
				CallVideoService callService = new CallVideoService(webVidConfig.FeedUri);

				DateTime.TryParse(webVidConfig.LastRuntime, out lastRan);

				videos = callService.GetVideos().ToList();

				foreach (google.Video vid in videos)
				{
					var myVideo = dynamicModuleManager.GetDataItems(syncedVideoType).Where(i => i.GetValue<string>("YouTubeVidId") == vid.VideoId && i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).FirstOrDefault();
					// if lastRan is set and (vid.YouTubeEntry.Published.Ticks > lastRan.Ticks || vid.YouTubeEntry.Updated.Ticks > lastRan.Ticks)
					if (webVidConfig.LastRuntime.IsNullOrWhitespace() || vid.YouTubeEntry.Published.Ticks > lastRan.Ticks || vid.YouTubeEntry.Updated.Ticks > lastRan.Ticks) 
						callService.createUpdateVideo(myVideo, vid);
				}
			}
			#endregion
			#region Rescheduling task
			{
				//Update Configuration Time
				ConfigManager manager = ConfigManager.GetManager();
				manager.Provider.SuppressSecurityChecks = true;
				WebVideoSyncConfig config = manager.GetSection<WebVideoSyncConfig>();
				config.LastRuntime = DateTime.UtcNow.ToString();
				manager.SaveSection(config);
				manager.Provider.SuppressSecurityChecks = false;

				//Reschedule Task
				SchedulingManager schedulingManager = SchedulingManager.GetManager();
				ScheduledVideoSync newTask = new ScheduledVideoSync();
				schedulingManager.AddTask(newTask);
				schedulingManager.SaveChanges();
			}
			#endregion
		}
	}
}
