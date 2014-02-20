using System;
using System.Configuration;
using System.Linq;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;

namespace WebVideoSync.Configuration
{
	[ObjectInfo(Title = "WebVideoSync Title", Description = "WebVideoSync Description")]
	public class WebVideoSyncConfig : ConfigSection
	{
		[ObjectInfo(Title = "YouTube Feed Url")]
		[ConfigurationProperty("FeedUri")]
		public string FeedUri
		{
			get
			{
				return (string)this["FeedUri"];
			}
			set
			{
				this["FeedUri"] = value;
			}
		}
		[ObjectInfo(Title = "Sync Frequency", Description = "In number of days")]
		[ConfigurationProperty("SyncFreq")]
		public int SyncFreq
		{
			get
			{
				return (int)this["SyncFreq"];
			}
			set
			{
				this["SyncFreq"] = value;
			}
		}
		[ObjectInfo(Title = "Thumbnail Library Name", Description = "The library that the video thumbnails should be stored at")]
		[ConfigurationProperty("ThumbLibName")]
		public string ThumbLibName
		{
			get
			{
				return (string)this["ThumbLibName"];
			}
			set
			{
				this["ThumbLibName"] = value;
			}
		}
		[ObjectInfo(Title = "Last Runtime", Description = "Last time videos were synced NOTE: Do not edit this field. For reference only")]
		[ConfigurationProperty("LastRuntime")]
		public string LastRuntime
		{
			get
			{
				return (string)this["LastRuntime"];
			}
			set
			{
				this["LastRuntime"] = value;
			}
		}
	}
}