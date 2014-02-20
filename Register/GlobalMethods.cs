using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Configuration.Model;
using Telerik.Sitefinity.Scheduling;
using WebVideoSync.Configuration;
using WebVideoSync.Module;
namespace WebVideoSync.Register
{
	public class GlobalMethods
	{
		public void Bootstrapper_Initialized(object sender, Telerik.Sitefinity.Data.ExecutedEventArgs e)
		{
			//Register config section
			Config.RegisterSection<WebVideoSync.Configuration.WebVideoSyncConfig>();

			WebVideoSyncConfig webVidConfig = Config.Get<WebVideoSyncConfig>();

			//Create Scheduled task if not exist
			if (!webVidConfig.FeedUri.IsNullOrWhitespace())
				createTask();
		} 
		protected void createTask() 
		{
			SchedulingManager manager = SchedulingManager.GetManager();
			string myKey = "D92F7C6F-A121-4f15-87F7-04ABBE0D7BE0";

			var count = manager.GetTaskData().Where(i => i.Key == myKey).ToList().Count;

			//if (count != 0)
			//{
			//	manager.DeleteTaskData(manager.GetTaskData().Where(i => i.Key == myKey).ToList().FirstOrDefault());
			//}
			//count = manager.GetTaskData().Where(i => i.Key == myKey).ToList().Count;

			if (count == 0)
			{
				ScheduledVideoSync newTask = new ScheduledVideoSync()
				{
					Key = myKey
				};
				manager.AddTask(newTask);
				manager.SaveChanges();
			}
		}
	}
}
