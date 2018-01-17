using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Media;
using Android.Net;
using Android.Net.Wifi;
using Android.Support.V4.Media.Session;
using Plugin.MediaManager.Abstraction;
using Plugin.MediaManager.Abstraction.Enums;
using Plugin.MediaManager.Playback.Helpers;


namespace Plugin.MediaManager
{
    public abstract class MediaPlaybackBase : IPlayback, AudioManager.IOnAudioFocusChangeListener
    {
        private WifiManager.WifiLock _wifiLock;
        private BroadcastReceiver _audioBecomeNoisyReceiver;
        private static object _lock;

        private bool _audioNoisyReceiverRegistered;
        private bool _playOnFocusGain;
        private bool _delayedPlayback = false;
        private bool _playbackAuthorized;
        
        internal Context ApplicationContext { get; set; }
        internal AudioManager AudioManager { get; set; }
        internal AudioFocusState CurrentAudioFocusState { get; set; }
        internal IPlaybackServiceCallback Callback { get; set; }

        public bool IsConnected => true;
        public string CurrentMediaId { get; set; }
        public IntPtr Handle { get; }


        public abstract int State { get; }
        public abstract bool IsPlaying { get; }
        public abstract void Start();
        public abstract void Play(MediaSessionCompat.QueueItem item);
        public abstract long GetCurrentStreamPosition();
        public abstract void UpdateLastKnownStreamPosition();
        public abstract void OnAudioFocusChange(AudioFocus focusChange);
        public abstract void Dispose();


        protected MediaPlaybackBase(Context context)
        {
            ApplicationContext = context;
            AudioManager = ApplicationContext.GetSystemService(Context.AudioService) as AudioManager;
            _wifiLock =
                ((WifiManager)ApplicationContext.GetSystemService(Context.WifiService)).CreateWifiLock(WifiMode.Full,
                    "XamMediaMgr_lock");
            _audioBecomeNoisyReceiver = new AudioBecomeNoisyReceiver(this);
            
        }

        public void Stop(bool notifyListeners)
        {
            GiveUpAudioFocus();
            UnregisterAudioNoisyReceiver();
            ReleaseResources(true);
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void SeekTo(long position)
        {
            throw new NotImplementedException();
        }

        public void SetCallback(IPlabackCallback callback)
        {
            throw new NotImplementedException();
        }

        private void GiveUpAudioFocus()
        {
            
        }

        private void TryGetAudioFocus()
        {
            var attributes = new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Media)
                .SetContentType(AudioContentType.Music)
                .Build();
            var request = new AudioFocusRequestClass.Builder(AudioFocus.Gain)
                .SetAudioAttributes(attributes)
                .SetAcceptsDelayedFocusGain(true)
                .SetOnAudioFocusChangeListener(this)
                .Build();

            var res = AudioManager.RequestAudioFocus(request);
            lock (_lock)
            {
                switch (res)
                {
                    case AudioFocusRequest.Delayed:
                        _delayedPlayback = true;
                        _playbackAuthorized = false;
                        break;
                    case AudioFocusRequest.Failed:
                        _playbackAuthorized = false;
                        break;
                    case AudioFocusRequest.Granted:
                        _playbackAuthorized = true;
                        //Can start playback
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ConfigurePlayState()
        {
            
        }

        private void SetAudioFocusChangedListener()
        {
            
        }

        private void ReleaseResources(bool releasePlayer)
        {
            
        }

        private void RegisterAudioNoisyReceiver()
        {
            
        }

        private void UnregisterAudioNoisyReceiver()
        {
            
        }
    }
}

