using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Support.V4.Media.Session;
using Java.Lang;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.Abstractions.Enums;
using Plugin.MediaManager.Abstractions.EventArguments;

namespace Plugin.MediaManager
{

    using Android.OS;

    using Java.Util.Concurrent;

    public delegate IMediaFile GetNextSong();
    public class AudioPlayerBase : IAudioPlayer
    {
        public event BufferingChangedEventHandler BufferingChanged;
        public event MediaFailedEventHandler MediaFailed;
        public event MediaFinishedEventHandler MediaFinished;
        public event PlayingChangedEventHandler PlayingChanged;
        public event StatusChangedEventHandler StatusChanged;
        public event MediaFileChangedEventHandler MediaFileChanged;
        public event MediaFileFailedEventHandler MediaFileFailed;

        public Context applicationContext;
        private Intent mediaPlayerServiceIntent;
        

        private IScheduledExecutorService _executorService = Executors.NewSingleThreadScheduledExecutor();
        private IScheduledFuture _scheduledFuture;


        public TimeSpan Position => TimeSpan.Zero;//GetMediaPlayerService().Position;

        public TimeSpan Duration => TimeSpan.Zero;//GetMediaPlayerService().Duration;

        public TimeSpan Buffered => TimeSpan.Zero;//GetMediaPlayerService().Buffered;

        public MediaSessionCompat.Callback AlternateRemoteCallback { get; set; }

        public Dictionary<string, string> RequestHeaders { get; set; }

        private MediaPlayerStatus status;
        public virtual MediaPlayerStatus Status
        {
            get
            {
                //if(!isBound) return MediaPlayerStatus.Stopped;
                //var state = GetMediaPlayerService().MediaPlayerState;
                return GetStatusByCompatValue(1);
            }
            private set
            {
                status = value;
                StatusChanged?.Invoke(this, new StatusChangedEventArgs(status));
            }
        }

        public AudioPlayerBase()
        {
          
        }



        public async Task Play(IEnumerable<IMediaFile> mediaFiles)
        {
            //await BinderReady();
            //await GetMediaPlayerService().Play(mediaFiles);
        }

        public virtual async Task Pause()
        {
            //await BinderReady();
            //await GetMediaPlayerService().Pause();
        }

        public virtual async Task Play(IMediaFile mediaFile)
        {
            //await BinderReady();
            //await GetMediaPlayerService().Play(mediaFile);
        }

        public virtual async Task Seek(TimeSpan position)
        {
            //await BinderReady();
            //await GetMediaPlayerService().Seek(position);
        }


        public virtual async Task Stop()
        {
            //await BinderReady();
            //await GetMediaPlayerService().Stop();
        }

        private void OnPlayingHandler(StatusChangedEventArgs args)
        {
            if (args.Status == MediaPlayerStatus.Playing)
            {
                CancelPlayingHandler();
                StartPlayingHandler();
            }
            if (args.Status == MediaPlayerStatus.Stopped || args.Status == MediaPlayerStatus.Failed || args.Status == MediaPlayerStatus.Paused)
                CancelPlayingHandler();
        }

        private void CancelPlayingHandler()
        {
            _scheduledFuture?.Cancel(false);
        }

        private void StartPlayingHandler()
        {
            var handler = new Handler();
            var runnable = new Runnable(() => { handler.Post(OnPlaying); });
			if (!_executorService.IsShutdown)
			{
				_scheduledFuture = _executorService.ScheduleAtFixedRate(runnable, 100, 1000, TimeUnit.Milliseconds);
			}
        }

        private void OnPlaying()
        {
            var progress = (Position.TotalSeconds/Duration.TotalSeconds) * 100;
            var position = Position;
            var duration = Duration;

            PlayingChanged?.Invoke(this, new PlayingChangedEventArgs(
                progress >= 0 ? progress : 0, 
                position.TotalSeconds >= 0 ? position : TimeSpan.Zero, 
                duration.TotalSeconds >= 0 ? duration : TimeSpan.Zero));
        }


        public MediaPlayerStatus GetStatusByCompatValue(int state)
        {
            switch (state)
            {
                case PlaybackStateCompat.StateFastForwarding:
                case PlaybackStateCompat.StateRewinding:
                case PlaybackStateCompat.StateSkippingToNext:
                case PlaybackStateCompat.StateSkippingToPrevious:
                case PlaybackStateCompat.StateSkippingToQueueItem:
                case PlaybackStateCompat.StatePlaying:
                    return MediaPlayerStatus.Playing;

                case PlaybackStateCompat.StatePaused:
                    return MediaPlayerStatus.Paused;

                case PlaybackStateCompat.StateConnecting:
                case PlaybackStateCompat.StateBuffering:
                    return MediaPlayerStatus.Buffering;

                case PlaybackStateCompat.StateError:
                case PlaybackStateCompat.StateStopped:
                    return MediaPlayerStatus.Stopped;

                default:
                    return MediaPlayerStatus.Stopped;
            }
        }

    }
}
