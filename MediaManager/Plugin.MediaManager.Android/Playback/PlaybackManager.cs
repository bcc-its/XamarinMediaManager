using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Media.Session;
using Plugin.MediaManager.Abstraction;
using Plugin.MediaManager.Abstractions;

namespace Plugin.MediaManager.Playback
{
    public class PlaybackManager : IPlabackCallback
    {
        private ILoggingService _log;
        private IPlaybackServiceCallback _serviceCallback;
        private Resources _resources;
        public IPlayback Playback { get; set; }
        public MediaSessionCallback MediaSessionCallback { get; set; }
        public QueueManager QueueManager { get; set; }

        public PlaybackManager(IPlaybackServiceCallback serviceCallback, Resources resources, IPlayback playback)
        {
            this._serviceCallback = serviceCallback;
            this._resources = resources;
            Playback = playback;
        }

        public void HandlePlayRequest()
        {
            var currentItem = QueueManager.GetCurrentMusic();
            if (currentItem != null)
            {
                _serviceCallback.OnPlaybackStart();
                Playback.Play(currentItem); // TODO Get media file
            }
        }


        public void HandlePauseRequest()
        {
            if (Playback.IsPlaying())
            {
                Playback.Pause();
                _serviceCallback.OnPlaybackStop(); // TODO Get media file
            }
        }

        public void HandleStopRequest(string withError)
        {
            Playback.Stop(true);
            _serviceCallback.OnPlaybackStop();
            UpdatePlaybackState(withError);
        }

        /**
         * Update the current media player state, optionally showing an error message.
         *
         * @param error if not null, error message to present to the user.
         */
        public void UpdatePlaybackState(string error)
        {
            _log.Debug("updatePlaybackState, playback state=" + Playback.State);
            long position = PlaybackStateCompat.PlaybackPositionUnknown;
            if (Playback != null && Playback.IsConnected())
            {
                position = Playback.GetCurrentStreamPosition();
            }

            //noinspection ResourceType
            PlaybackStateCompat.Builder stateBuilder = new PlaybackStateCompat.Builder().SetActions(GetAvailableActions());

           // setCustomAction(stateBuilder);
            int state = Playback.State;

            // If there is an error message, send it to the playback state:
            if (error != null)
            {
                // Error states are really only supposed to be used for errors that cause playback to
                // stop unexpectedly and persist until the user takes action to fix it.
                stateBuilder.SetErrorMessage(error);
                state = PlaybackStateCompat.StateError;
            }
            //noinspection ResourceType
            stateBuilder.SetState(state, position, 1.0f, SystemClock.ElapsedRealtime());

            // Set the activeQueueItemId if the current index is valid.
            MediaSessionCompat.QueueItem currentMusic = QueueManager.GetCurrentMusic();
            if (currentMusic != null)
            {
                stateBuilder.SetActiveQueueItemId(currentMusic.QueueId);
            }

            _serviceCallback.OnPlaybackStateUpdated(stateBuilder.Build());

            if (state == PlaybackStateCompat.StatePlaying || state == PlaybackStateCompat.StatePaused)
            {
                _serviceCallback.OnNotificationRequired();
            }
        }


        public void OnCompletion()
        {
            if (QueueManager.SkipQueuePosition(1))
            {
                HandlePlayRequest();
                QueueManager.UpdateMetadata();
            }
            else
            {
                HandleStopRequest(null);
            }

        }

        public void OnPlaybackStatusChanged(int state)
        {
            UpdatePlaybackState(null);
        }

        public void OnError(string error)
        {
            UpdatePlaybackState(error);
        }

        public void SetCurrentMediaId(string mediaId)
        {
            QueueManager.SetQueueFromMusic(mediaId);
        }

        private long GetAvailableActions()
        {
            var actions =
                PlaybackStateCompat.ActionPause |
                PlaybackStateCompat.ActionPlayFromMediaId |
                PlaybackStateCompat.ActionPlayFromSearch|
                PlaybackStateCompat.ActionSkipToPrevious|
                PlaybackStateCompat.ActionSkipToNext;
            if (Playback.IsPlaying())
            {
                actions |= PlaybackStateCompat.ActionPause;
            }
            else
            {
                actions |= PlaybackStateCompat.ActionPlay;
            }
            return actions;
        }

        private void SetCustomAction(PlaybackStateCompat.Builder stateBuilder)
        {
            //MediaSessionCompat.QueueItem currentMusic = QueueManager.GetCurrentMusic();
            //// Set appropriate "Favorite" icon on Custom action:
            //string mediaId = currentMusic?.Description.MediaId;
            //if (mediaId == null)
            //{
            //    return;
            //}
            //string musicId = MediaIDHelper.extractMusicIDFromMediaID(mediaId);
            //int favoriteIcon = mMusicProvider.isFavorite(musicId) ?
            //    R.drawable.ic_star_on : R.drawable.ic_star_off;
            //LogHelper.d(TAG, "updatePlaybackState, setting Favorite custom action of music ",
            //    musicId, " current favorite=", mMusicProvider.isFavorite(musicId));
            //Bundle customActionExtras = new Bundle();
            //WearHelper.setShowCustomActionOnWear(customActionExtras, true);
            //stateBuilder.addCustomAction(new PlaybackStateCompat.CustomAction.Builder(
            //    CUSTOM_ACTION_THUMBS_UP, mResources.getstring(R.string.favorite), favoriteIcon)
            //    .setExtras(customActionExtras)
            //    .build());
        }

    }
}