YouTubeSitefinitySync
=====================

Nightly call from sitefinity to download videos from youtube account

_Please keep in mind that the code for this was compiled for SF 5.2, I will be updating for 6.3 in near future_

## Install
To install the module begin by going to the Module Builder section of the Administration Drop Down. From there import the install.sf file. This contains the base structure of the module.

Once the new module is in place, there are two template files "ListOfVideos.ascx" and "SingleVideo.ascx" these will replace the template files that get created by sitefinity for the module.

For the source code, please compile it for your specific version of sitefinity, once compiled find the "WebVideoSync.dll" in the bin folder, and place it into the bin folder of the SitefinityWebApp project.

Finally inside of the global.asax, add a using to:
    using WebVideoSync.Register;
    
and in the Applicatation_Start method add:
    protected void Application_Start(object sender, EventArgs e)
    {
        Telerik.Sitefinity.Abstractions.Bootstrapper.Initialized += new EventHandler<Telerik.Sitefinity.Data.ExecutedEventArgs>(new GlobalMethods().Bootstrapper_Initialized);
    }
