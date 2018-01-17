using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace Plugin.MediaManager.Playback
{
    public class QueueManager
    {

        //private static string TAG = //LogHelper.makeLogTag(QueueManager.class);

        //private MusicProvider mMusicProvider;

        //private MetadataUpdateListener mListener;

        private Resources _mResources;

        //  "Now playing" queue:
        private List<MediaSessionCompat.QueueItem> _mPlayingQueue;

        private int _mCurrentIndex;

        public QueueManager()
        {
        }

        public bool IsSameBrowsingCategory()
        {
            return true;
        }

        //string[] newBrowseHierarchy = MediaIDHelper.getHierarchy(mediaId);

        //MediaSessionCompat.QueueItem current = getCurrentMusic();

        //string[] currentBrowseHierarchy = MediaIDHelper.getHierarchy(current.getDescription().getMediaId());

        private void SetCurrentQueueIndex(int index)
        {
            if (((index >= 0)
                 && (index < _mPlayingQueue.size())))
            {
                _mCurrentIndex = index;
                mListener.onCurrentQueueIndexUpdated(_mCurrentIndex);
            }

        }

        public bool SetCurrentQueueItem(long queueId)
        {
            //  set the current index on queue from the queue Id:
            int index = QueueHelper.GetMusicIndexOnQueue(_mPlayingQueue, queueId);
            SetCurrentQueueIndex(index);
            return (index >= 0);
        }

        public bool SetCurrentQueueItem(string mediaId)
        {
            //  set the current index on queue from the music Id:
            int index = QueueHelper.GetMusicIndexOnQueue(_mPlayingQueue, mediaId);
            SetCurrentQueueIndex(index);
            return (index >= 0);
        }

        public bool SkipQueuePosition(int amount)
        {
            int index = (_mCurrentIndex + amount);
            if ((index < 0))
            {
                //  skip backwards before the first song will keep you on the first song
                index = 0;
            }
            else
            {
                //  skip forwards when in last song will cycle back to start of the queue
                _mPlayingQueue.size();
            }

            if (!QueueHelper.IsIndexPlayable(index, _mPlayingQueue))
            {
                //LogHelper.e(TAG, "Cannot increment queue index by ", amount, ". Current=", mCurrentIndex, " queue length=", mPlayingQueue.size());
                return false;
            }

            _mCurrentIndex = index;
            return true;
        }

        public bool SetQueueFromSearch(string query, Bundle extras)
        {
            List<MediaSessionCompat.QueueItem> queue =
                QueueHelper.GetPlayingQueueFromSearch(query, extras, mMusicProvider);
            setCurrentQueue(_mResources.getstring(R.string.search_queue_title), queue);
            UpdateMetadata();
            return ((queue != null)
                    && !queue.isEmpty());
        }

        public void SetRandomQueue()
        {
            setCurrentQueue(_mResources.getstring(R.string.random_queue_title), QueueHelper.GetRandomQueue(
                mMusicProvider));
            UpdateMetadata();
        }

        public void SetQueueFromMusic(string mediaId)
        {
            //LogHelper.d(TAG, "setQueueFromMusic", mediaId);
            //  The mediaId used here is not the unique musicId. This one comes from the
            //  MediaBrowser, and is actually a "hierarchy-aware mediaID": a concatenation of
            //  the hierarchy in MediaBrowser and the actual unique musicID. This is necessary
            //  so we can build the correct playing queue, based on where the track was
            //  selected from.
            bool canReuseQueue = false;
            if (isSameBrowsingCategory(mediaId))
            {
                canReuseQueue = SetCurrentQueueItem(mediaId);
            }

            if (!canReuseQueue)
            {
                string queueTitle = _mResources.getstring(R.string.browse_musics_by_genre_subtitle, MediaIDHelper
                    .extractBrowseCategoryValueFromMediaID(mediaId));
                SetCurrentQueue(queueTitle, QueueHelper.GetPlayingQueue(mediaId, mMusicProvider), mediaId);
            }

            UpdateMetadata();
        }

        public MediaSessionCompat.QueueItem GetCurrentMusic()
        {
            if (!QueueHelper.IsIndexPlayable(_mCurrentIndex, _mPlayingQueue))
            {
                return null;
            }

            return _mPlayingQueue.get(_mCurrentIndex);
        }

        public int GetCurrentQueueSize()
        {
            if ((_mPlayingQueue == null))
            {
                return 0;
            }

            return _mPlayingQueue.size();
        }

        protected void SetCurrentQueue(string title, List<MediaSessionCompat.QueueItem> newQueue)
        {
            SetCurrentQueue(title, newQueue, null);
        }

        protected void SetCurrentQueue(string title, List<MediaSessionCompat.QueueItem> newQueue, string initialMediaId)
        {
            _mPlayingQueue = newQueue;
            int index = 0;
            if ((initialMediaId != null))
            {
                index = QueueHelper.GetMusicIndexOnQueue(_mPlayingQueue, initialMediaId);
            }

            _mCurrentIndex = Math.max(index, 0);
            mListener.onQueueUpdated(title, newQueue);
        }

        public void UpdateMetadata()
        {
            MediaSessionCompat.QueueItem currentMusic = GetCurrentMusic();
            if ((currentMusic == null))
            {
                mListener.onMetadataRetrieveError();
                return;
            }

            string musicId = MediaIDHelper.extractMusicIDFromMediaID(currentMusic.getDescription().getMediaId());
            MediaMetadataCompat metadata = mMusicProvider.getMusic(musicId);
            if ((metadata == null))
            {
                throw new IllegalArgumentException(("Invalid musicId " + musicId));
            }

            mListener.onMetadataChanged(metadata);
            //  Set the proper album artwork on the media session, so it can be shown in the
            //  locked screen and in other places.
            if (((metadata.getDescription().getIconBitmap() == null)
                 && (metadata.getDescription().getIconUri() != null)))
            {
                string albumUri = metadata.getDescription().getIconUri().tostring();
                AlbumArtCache.getInstance().fetch(albumUri, new AlbumArtCache.FetchListener());
            }

        }
    }
}