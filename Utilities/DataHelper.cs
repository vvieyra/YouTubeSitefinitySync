using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Model.ContentLinks;

namespace WebVideoSync.Utilities
{
	public class DataHelper
	{
		public static string GetImageUrl(object data, object title)
		{
			string imageUrl = "";
			try
			{
				ContentLink[] thumbnails = data as ContentLink[];

				Image img = App.WorkWith().Images().Where(i => i.Id == thumbnails[0].ChildItemId).Get().FirstOrDefault();
				if (img != null)
					imageUrl = img.MediaUrl;
			}
			catch
			{
				imageUrl = "http://placehold.it/233x151&text=" + title.ToString().Replace(" ", "+");
			}
			return imageUrl;
		}
	}
}
