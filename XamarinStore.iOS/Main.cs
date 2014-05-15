using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace XamarinStore.iOS
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			PixateFreestyleLib.PixateFreestyle.InitializePixateFreestyle ();
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}
