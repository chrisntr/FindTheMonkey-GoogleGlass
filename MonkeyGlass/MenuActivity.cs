using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Glass.App;
using RadiusNetworks.IBeaconAndroid;
using System.Collections.Generic;
using System.Linq;
using Android.Glass.Timeline;
using Android.Media;
using Android.Glass.Media;

namespace MonkeyGlass
{
	[Activity (Theme = "@style/MenuTheme")]
	public class MenuActivity : Activity
	{

		public override void OnAttachedToWindow ()
		{
			base.OnAttachedToWindow ();
			OpenOptionsMenu ();
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.MonkeySearch, menu);
			return true;
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			// Handle item selection.
			switch (item.ItemId) {
			case Resource.Id.Stop:
				StopService (new Intent (this, typeof(MainService)));
				return true;
			default:
				return base.OnOptionsItemSelected (item);
			}
		}

		public override void OnOptionsMenuClosed (IMenu menu)
		{
			Finish ();
		}
	}
}


