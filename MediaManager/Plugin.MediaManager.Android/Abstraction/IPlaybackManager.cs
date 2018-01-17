using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media.Session;
using Android.Views;
using Android.Widget;
using Plugin.MediaManager.Abstractions;

namespace Plugin.MediaManager.Abstraction
{
    public interface IPlayback
    {
        /// <summary>
        /// Start/setup the playback.
        /// Resources/listeners would be allocated by implementations.
        /// </summary>
        void Start();
        
        /// <summary>
        /// Stops the playback
        /// </summary>
        /// <param name="notifyListeners">  
        /// notifyListeners if true and a callback has been set by setCallback, callback.onPlaybackStatusChanged will be called after changing the state.
        /// if set to <c>true</c> [notify listeners].
        /// </param>
        void Stop(bool notifyListeners);

        /// <summary>
        /// Gets the current stream position.
        /// </summary>
        /// <returns></returns>
        long GetCurrentStreamPosition();
        
        /// <summary>
        /// Updates the last known stream position.  Queries the underlying stream and update the internal last known stream position.
        /// </summary>
        void UpdateLastKnownStreamPosition();

        /// <summary>
        /// Plays the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        void Play(MediaSessionCompat.QueueItem item);

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        void Pause();

        /// <summary>
        /// Seeks to.
        /// </summary>
        /// <param name="position">The position.</param>
        void SeekTo(long position);

        /// <summary>
        /// Gets or sets the current media identifier.
        /// </summary>
        /// <value>
        /// The current media identifier.
        /// </value>
        string CurrentMediaId { get; set; }

        /// <summary>
        /// Sets the callback.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void SetCallback(IPlabackCallback callback);

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        int State { get; }

        /// <summary>
        /// Determines whether this instance is connected.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is connected and is ready to be used.; otherwise, <c>false</c>.
        /// </returns>
        bool IsConnected { get; }

        /// <summary>
        /// Determines whether this instance is playing.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is playing or is supposed to be playing when we gain audio focus.; otherwise, <c>false</c>.
        /// </returns>
        bool IsPlaying { get; };
    }

    public interface IPlabackCallback
    {
        /// <summary>
        /// Called when [completion].
        /// </summary>
        void OnCompletion();

        /// <summary>
        /// Called when [playback status changed].
        /// on Playback status changed
        /// Implementations can use this callback to update
        /// playback state on the media sessions.
        /// </summary>
        /// <param name="state">The state.</param>
        void OnPlaybackStatusChanged(int state);


        /// <summary>
        /// Called when [error].
        /// </summary>
        /// <param name="error">The error.</param>
        void OnError(string error);

        /// <summary>
        /// Sets the current media identifier.
        /// </summary>
        /// <param name="mediaId">The media identifier.</param>
        void SetCurrentMediaId(string mediaId);
    }


}