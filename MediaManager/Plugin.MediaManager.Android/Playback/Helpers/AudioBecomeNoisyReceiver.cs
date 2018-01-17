using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.MediaManager.Abstraction;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.MediaBrowserServiceImplementation;

namespace Plugin.MediaManager.Playback.Helpers
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class AudioBecomeNoisyReceiver : BroadcastReceiver
    {
        private readonly MediaPlaybackBase _playback;
        public AudioBecomeNoisyReceiver(MediaPlaybackBase playback)
        {
            _playback = playback;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if(_playback?.ApplicationContext == null)
                return;

            if (!AudioManager.ActionAudioBecomingNoisy.Equals(intent.Action))
                return;
            if (!_playback.IsPlaying())
                return;

            var i = new Intent(context, typeof(AudioService));
            i.SetAction(AudioService.ACTION_CMD);
            i.PutExtra(AudioService.CMD_NAME, AudioService.CMD_PAUSE);
            _playback.ApplicationContext.StartService(i);
        }
    }
}