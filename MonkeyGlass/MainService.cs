using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Glass.App;
using Android.Glass.Media;
using Android.Glass.Timeline;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RadiusNetworks.IBeaconAndroid;

namespace MonkeyGlass
{
	[Service (Icon = "@drawable/icon")]
	[IntentFilter (new String[]{ "com.google.android.glass.action.VOICE_TRIGGER" })]
	[MetaData ("com.google.android.glass.VoiceTrigger", Resource = "@xml/voicetriggerstart")]
	public class MainService : Service, IBeaconConsumer
	{
		private const string UUID = "e4C8A4FCF68B470D959F29382AF72CE7";
		private const string monkeyId = "Monkey";

		IBeaconManager _iBeaconManager;
		RangeNotifier _rangeNotifier;
		Region _rangingRegion;
		LiveCard livecard;
		RemoteViews remoteViews;

		public override IBinder OnBind (Intent intent)
		{
			return null;
		}

		public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
		{
			if (livecard == null)
			{
				livecard = new LiveCard (Application, "beacon");
				remoteViews = new RemoteViews (PackageName, Resource.Layout.LiveCardBeacon);
				remoteViews.SetTextViewText (Resource.Id.LivecardContent, "Finding the monkey...");
				livecard.SetViews (remoteViews);

				// Set up the live card's action with a pending intent
				// to show a menu when tapped
				var menuIntent = new Intent(this, typeof(MenuActivity));
				menuIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
				livecard.SetAction(PendingIntent.GetActivity(this, 0, menuIntent, 0));
				livecard.Publish (LiveCard.PublishMode.Reveal);

			}
			return StartCommandResult.Sticky;
		}
			
		public override void OnCreate ()
		{
			base.OnCreate ();

			_iBeaconManager = IBeaconManager.GetInstanceForApplication (this);

			_iBeaconManager.SetForegroundScanPeriod (2000);
			_iBeaconManager.SetForegroundBetweenScanPeriod (2500);

			_rangeNotifier = new RangeNotifier ();

			_rangingRegion = new Region (monkeyId, UUID, null, null);
			_iBeaconManager.Bind (this);

			_rangeNotifier.DidRangeBeaconsInRegionComplete += RangingBeaconsInRegion;

		}

		void RangingBeaconsInRegion(object sender, RangeEventArgs e)
		{
			if (e.Beacons.Count > 0)
			{

				var beacon = e.Beacons.FirstOrDefault();

				switch((ProximityType)beacon.Proximity)
				{
				case ProximityType.Immediate:
					UpdateDisplay("You found the monkey!", Android.Graphics.Color.Green);
					break;
				case ProximityType.Near:
					UpdateDisplay("You're getting warmer", Android.Graphics.Color.Yellow);
					break;
				case ProximityType.Far:
					UpdateDisplay("You're freezing cold", Android.Graphics.Color.Blue);
					break;
				case ProximityType.Unknown:
					UpdateDisplay("I'm not sure how close you are to the monkey", Android.Graphics.Color.Red);
					break;
				}
			}
		}

		public void OnIBeaconServiceConnect()
		{
			_iBeaconManager.SetRangeNotifier(_rangeNotifier);

			_iBeaconManager.StartRangingBeaconsInRegion(_rangingRegion);
		}

		private void UpdateDisplay(string message, Android.Graphics.Color color)
		{
			//Uncomment if you want to hear a success sound on every screenupdate 
			//			AudioManager audio = (AudioManager) GetSystemService(Context.AudioService);
			//			audio.PlaySoundEffect((SoundEffect)Sounds.Success);

			if (color == Android.Graphics.Color.Yellow)
				remoteViews.SetTextColor (Resource.Id.LivecardContent, Android.Graphics.Color.Black);
			else
				remoteViews.SetTextColor (Resource.Id.LivecardContent, Android.Graphics.Color.White);
			remoteViews.SetInt(Resource.Id.Framelayout1, "setBackgroundColor", color);

			remoteViews.SetTextViewText (Resource.Id.LivecardContent, message);

			livecard.SetViews (remoteViews);
		}

		public override void OnDestroy() 
		{
			if (livecard != null && livecard.IsPublished) {
				_rangeNotifier.DidRangeBeaconsInRegionComplete -= RangingBeaconsInRegion;

				_iBeaconManager.StopRangingBeaconsInRegion (_rangingRegion);
				_iBeaconManager.UnBind (this);

				livecard.Unpublish();
				livecard = null;
			}
			base.OnDestroy();
		}
	}
}