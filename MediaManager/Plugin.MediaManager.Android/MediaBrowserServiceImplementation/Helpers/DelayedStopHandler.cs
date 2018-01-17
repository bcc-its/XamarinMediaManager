using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.Abstractions.Enums;

namespace Plugin.MediaManager.MediaBrowserServiceImplementation.Helpers
{
    public class DelayedStopHandler : Handler
    {
        private WeakReference<AudioService> weakReference;
        private ILoggingService log;

        internal DelayedStopHandler(AudioService service, ILoggingService log)
        {
            this.Log = log;
            weakReference = new WeakReference<AudioService>(service);
        }
        
        public override void HandleMessage(Message msg)
        {
            weakReference.TryGetTarget(out var service);
            if (service?.AudioPlayer == null)
                return;

            if (service.AudioPlayer.Status == MediaPlayerStatus.Playing)
            {
                log.Debug("Ignoring delayed stop since the media player is in use.");
                return;
            }
            log.Debug("Stopping service with delay handler.");
            service.StopSelf();
        }
    }
}