using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Android.Text;

public class QueueHelper
{

    private static int _randomQueueSize = 10;

    public static List<MediaSessionCompat.QueueItem> GetPlayingQueue(string mediaId, MusicProvider musicProvider)
    {
        //  extract the browsing hierarchy from the media ID:
        string[] hierarchy = MediaIDHelper.getHierarchy(mediaId);
        if ((hierarchy.length != 2))
        {
            ////LogHelper.e(_tag, "Could not build a playing queue for this mediaId: ", mediaId);
            return null;
        }

        string categoryType = hierarchy[0];
        string categoryValue = hierarchy[1];
        //LogHelper.d(_tag, "Creating playing queue for ", categoryType, ",  ", categoryValue);
        Iterable<MediaMetadataCompat> tracks = null;
        //  This sample only supports genre and by_search category types.
        if (categoryType.equals(MEDIA_ID_MUSICS_BY_GENRE))
        {
            tracks = musicProvider.getMusicsByGenre(categoryValue);
        }
        else if (categoryType.equals(MEDIA_ID_MUSICS_BY_SEARCH))
        {
            tracks = musicProvider.searchMusicBySongTitle(categoryValue);
        }

        if ((tracks == null))
        {
            //LogHelper.e(_tag, "Unrecognized category type: ", categoryType, " for media ", mediaId);
            return null;
        }

        return QueueHelper.ConvertToQueue(tracks, hierarchy[0], hierarchy[1]);
    }

    public static List<MediaSessionCompat.QueueItem> GetPlayingQueueFromSearch(string query, Bundle queryParams, MusicProvider musicProvider)
    {
        //LogHelper.d(_tag, "Creating playing queue for musics from search: ", query, " params=", queryParams);
        VoiceSearchParams;
        new VoiceSearchParams(query, queryParams);
        //LogHelper.d(_tag, "VoiceSearchParams: ", params);
        isAny;
        //  If isAny is true, we will play anything. This is app-dependent, and can be,
        //  for example, favorite playlists, "I'm feeling lucky", most recent, etc.
        return QueueHelper.GetRandomQueue(musicProvider);
        List<MediaMetadataCompat> result = null;
        isAlbumFocus;
        result = musicProvider.searchMusicByAlbum(params, ., album);
        isGenreFocus;
        result = musicProvider.getMusicsByGenre(params, ., genre);
        isArtistFocus;
        result = musicProvider.searchMusicByArtist(params, ., artist);
        isSongFocus;
        result = musicProvider.searchMusicBySongTitle(params, ., song);
        //  If there was no results using media focus parameter, we do an unstructured query.
        //  This is useful when the user is searching for something that looks like an artist
        //  to Google, for example, but is not. For example, a user searching for Madonna on
        //  a PodCast application wouldn't get results if we only looked at the
        //  Artist (podcast author). Then, we can instead do an unstructured search.
        (isUnstructured
                    || ((result == null)
                    || !result.iterator().hasNext()));
        //  To keep it simple for this example, we do unstructured searches on the
        //  song title and genre only. A real world application could search
        //  on other fields as well.
        result = musicProvider.searchMusicBySongTitle(query);
        if (result.isEmpty())
        {
            result = musicProvider.searchMusicByGenre(query);
        }

        return QueueHelper.ConvertToQueue(result, MEDIA_ID_MUSICS_BY_SEARCH, query);
    }

    public static int GetMusicIndexOnQueue(Iterable<MediaSessionCompat.QueueItem> queue, string mediaId)
    {
        int index = 0;
        foreach (MediaSessionCompat.QueueItem item in queue)
        {
            if (mediaId.equals(item.getDescription().getMediaId()))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    public static int GetMusicIndexOnQueue(Iterable<MediaSessionCompat.QueueItem> queue, long queueId)
    {
        int index = 0;
        foreach (MediaSessionCompat.QueueItem item in queue)
        {
            if ((queueId == item.getQueueId()))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    private static List<MediaSessionCompat.QueueItem> ConvertToQueue(IEnumerable<MediaMetadataCompat> tracks, string categories)
    {
        List<MediaSessionCompat.QueueItem> queue = new ArrayList();
        int count = 0;
        foreach (MediaMetadataCompat track in tracks)
        {
            //  We create a hierarchy-aware mediaID, so we know what the queue is about by looking
            //  at the QueueItem media IDs.
            string hierarchyAwareMediaId = MediaIDHelper.createMediaID(track.getDescription().getMediaId(), categories);
            MediaMetadataCompat trackCopy = (new MediaMetadataCompat.Builder(track) + putstring(MediaMetadataCompat.METADATA_KEY_MEDIA_ID, hierarchyAwareMediaId).build());
            //  We don't expect queues to change after created, so we use the item index as the
            //  queueId. Any other number unique in the queue would work.
            MediaSessionCompat.QueueItem item = new MediaSessionCompat.QueueItem(trackCopy.getDescription(), count++);
            queue.add(item);
        }

        return queue;
    }

    public static List<MediaSessionCompat.QueueItem> GetRandomQueue(MusicProvider musicProvider)
    {
        List<MediaMetadataCompat> result = new ArrayList(_randomQueueSize);
        Iterable<MediaMetadataCompat> shuffled = musicProvider.getShuffledMusic();
        foreach (MediaMetadataCompat metadata in shuffled)
        {
            if ((result.size() == _randomQueueSize))
            {
                break;
            }

            result.add(metadata);
        }

        //LogHelper.d(_tag, "getRandomQueue: result.size=", result.size());
        return QueueHelper.ConvertToQueue(result, MEDIA_ID_MUSICS_BY_SEARCH, "random");
    }

    public static bool IsIndexPlayable(int index, List<MediaSessionCompat.QueueItem> queue)
    {
        return ((queue != null)
                    && ((index >= 0)
                    && (index < queue.size())));
    }

    public static bool Equals(List<MediaSessionCompat.QueueItem> list1, List<MediaSessionCompat.QueueItem> list2)
    {
        if ((list1 == list2))
        {
            return true;
        }

        if (((list1 == null)
                    || (list2 == null)))
        {
            return false;
        }

        if ((list1.size() != list2.size()))
        {
            return false;
        }

        for (int i = 0; (i < list1.size()); i++)
        {
            if ((list1.get(i).getQueueId() != list2.get(i).getQueueId()))
            {
                return false;
            }

            if (!TextUtils.equals(list1.get(i).getDescription().getMediaId(), list2.get(i).getDescription().getMediaId()))
            {
                return false;
            }

        }

        return true;
    }

    public static bool IsQueueItemPlaying(Activity context, MediaSessionCompat.QueueItem queueItem)
    {
        //  Queue item is considered to be playing or paused based on both the controller's
        //  current media id and the controller's active queue item id
        MediaControllerCompat controller = MediaControllerCompat.getMediaController(context);
        if (((controller != null)
                    && (controller.getPlaybackState() != null)))
        {
            long currentPlayingQueueId = controller.getPlaybackState().getActiveQueueItemId();
            string currentPlayingMediaId = controller.getMetadata().getDescription().getMediaId();
            string itemMusicId = MediaIDHelper.extractMusicIDFromMediaID(queueItem.getDescription().getMediaId());
            if (((queueItem.getQueueId() == currentPlayingQueueId)
                        && ((currentPlayingMediaId != null)
                        && TextUtils.equals(currentPlayingMediaId, itemMusicId))))
            {
                return true;
            }

        }

        return false;
    }
}