using Android.Support.V4.Media.Session;

namespace Plugin.MediaManager.Abstraction
{
    public interface IPlaybackServiceCallback
    {

        void OnPlaybackStart();

        void OnNotificationRequired();

        void OnPlaybackStop();

        void OnPlaybackStateUpdated(PlaybackStateCompat newState);
    }
}