using Android.Drm;
using Android.OS;
using Android.Support.V4.Media.Session;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.MediaBrowserServiceImplementation;
using Plugin.MediaManager.Playback;

public class MediaSessionCallback : MediaSessionCompat.Callback
{
    private ILoggingService _log;
    private PlaybackManager manager;

    public MediaSessionCallback(PlaybackManager manager)
    {
        this.manager = manager;
    }

    public override void OnPlay()
    {
        _log.Debug("play");
        if (manager.QueueManager.GetCurrentMusic() == null)
        {
            manager.QueueManager.SetRandomQueue();
        }
        manager.HandlePlayRequest();
    }


    public override void OnSkipToQueueItem(long queueId)
    {
        _log.Debug("OnSkipToQueueItem:" + queueId);
        manager.QueueManager.SetCurrentQueueItem(queueId);
        manager.QueueManager.UpdateMetadata();
    }


    public override void OnSeekTo(long position)
    {
        _log.Debug("onSeekTo: {position}");
        manager.Playback.SeekTo((int)position);
    }


    public override void OnPlayFromMediaId(string mediaId, Bundle extras)
    {
        _log.Debug($"playFromMediaId mediaId: {mediaId}  extras={extras}");
        manager.QueueManager.SetQueueFromMusic(mediaId);
        manager.HandlePlayRequest();
    }


    public override void OnPause()
    {
        _log.Debug("pause. current state=" + manager.Playback.State);
        manager.HandlePauseRequest();
    }


    public override void OnStop()
    {
        _log.Debug("stop. current state=" + manager.Playback.State);
        manager.HandleStopRequest(null);
    }


    public override void OnSkipToNext()
    {
        _log.Debug("skipToNext");
        if (manager.QueueManager.SkipQueuePosition(1))
        {
            manager.HandlePlayRequest();
        }
        else
        {
            manager.HandleStopRequest("Cannot skip");
        }
        manager.QueueManager.UpdateMetadata();
    }


    public override void OnSkipToPrevious()
    {
        if (manager.QueueManager.SkipQueuePosition(-1))
        {
            manager.HandlePlayRequest();
        }
        else
        {
            manager.HandleStopRequest("Cannot skip");
        }
        manager.QueueManager.UpdateMetadata();
    }

}