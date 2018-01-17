using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Plugin.MediaManager.Abstraction;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.MediaBrowserServiceImplementation.Helpers;
using Plugin.MediaManager.Playback;
using Exception = System.Exception;

namespace Plugin.MediaManager.MediaBrowserServiceImplementation
{
    public class AudioService : MediaBrowserServiceCompat, IPlaybackServiceCallback
    {
        private ILoggingService log;

        // Extra on MediaSession that contains the Cast device name currently connected to
        public static string EXTRA_CONNECTED_CAST = "com.example.android.uamp.CAST_NAME";
        // The action of the incoming Intent indicating that it contains a command
        // to be executed (see {@link #onStartCommand})
        public static string ACTION_CMD = "com.example.android.uamp.ACTION_CMD";
        // The key in the extras of the incoming Intent indicating the command that
        // should be executed (see {@link #onStartCommand})
        public static string CMD_NAME = "CMD_NAME";
        // A value of a CMD_NAME key in the extras of the incoming Intent that
        // indicates that the music playback should be paused (see {@link #onStartCommand})
        public static string CMD_PAUSE = "CMD_PAUSE";
        // A value of a CMD_NAME key that indicates that the music playback should switch
        // to local playback from cast playback.
        public static string CMD_STOP_CASTING = "CMD_STOP_CASTING";
        // Delay stopSelf by using a handler.
        private static int STOP_DELAY = 30000;

        internal IPlayback AudioPlayer { get; set; }
        internal IPlayback CastPlayer { get; set; }

        public MediaSessionCompat MediaSession { get; set; }
        private IMediaNotificationManager notificationManager;
        private Bundle sessionExtras;
        private PlaybackManager manager;

        private DelayedStopHandler delayedStopHandler;
        private MediaRouter mediaRouter;
        private PackageValidator packageValidator;
       // private SessionManager mCastSessionManager;
       // private SessionManagerListener<CastSession> mCastSessionManagerListener;

        public AudioService()
        {
            //TODO: Logging service
            delayedStopHandler = new DelayedStopHandler(this, null);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            AudioPlayer = new MediaPlayerPlayback(this);
            MediaSession = new MediaSessionCompat(this, nameof(AudioService));
            SessionToken = MediaSession.SessionToken;
            var queueManager = new QueueManager();
            //TODO: Set listener
            manager = new PlaybackManager(this, Resources, AudioPlayer);
            var intent = new Intent(ApplicationContext, typeof(string)); //Pass correct type
            var pendingIntent = PendingIntent.GetActivity(ApplicationContext, 99, intent, PendingIntentFlags.UpdateCurrent);

            MediaSession.SetCallback(manager.MediaSessionCallback);
            MediaSession.SetFlags(MediaSessionCompat.FlagHandlesMediaButtons | MediaSessionCompat.FlagHandlesTransportControls);           
            MediaSession.SetSessionActivity(pendingIntent);

            //sessionExtras = new Bundle();
            // SET Car / WEAR Helpers

            manager.UpdatePlaybackState(null);

            try
            {
                notificationManager = new MediaNotificationManagerImplementation(this);
            }
            catch (Exception e)
            {
                throw new IllegalStateException("Could not create a MediaNotificationManager");
            }

            //GoogleApiAvilability

            // MediaRouter.
        }


        public override StartCommandResult OnStartCommand(Intent startIntent, StartCommandFlags flags, int startId)
        {
            if (startIntent != null)
            {
                string action = startIntent.Action;
                string command = startIntent.GetStringExtra(CMD_NAME);
                if (ACTION_CMD.Equals(action))
                {
                    if (CMD_PAUSE.Equals(command))
                    {
                        manager.HandlePauseRequest();
                    }
                    //else if (CMD_STOP_CASTING.equals(command))
                    //{
                    //    CastContext.getSharedInstance(this).getSessionManager().endCurrentSession(true);
                    //}
                }
                else
                {
                    // Try to handle the intent as a media button event wrapped by MediaButtonReceiver
                    MediaButtonReceiver.HandleIntent(MediaSession, startIntent);
                }
            }
            // Reset the delay handler to enqueue a message to stop the service if
            // nothing is playing.
            delayedStopHandler.RemoveCallbacksAndMessages(null);
            delayedStopHandler.SendEmptyMessageDelayed(0, STOP_DELAY);
            return StartCommandResult.Sticky;
        }

        public override BrowserRoot OnGetRoot(string p0, int p1, Bundle p2)
        {
            throw new NotImplementedException();
        }

        public override void OnLoadChildren(string p0, Result p1)
        {
            throw new NotImplementedException();
        }

        public void OnPlaybackStart()
        {
            throw new NotImplementedException();
        }

        public void OnNotificationRequired()
        {
            throw new NotImplementedException();
        }

        public void OnPlaybackStop()
        {
            throw new NotImplementedException();
        }

        public void OnPlaybackStateUpdated(PlaybackStateCompat newState)
        {
            throw new NotImplementedException();
        }
    }
}