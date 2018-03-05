using System;
using System.Runtime.InteropServices;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.Media.Session;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.Abstractions.Enums;
using Plugin.MediaManager.Abstractions.Implementations;
using NotificationCompat = Android.Support.V7.App.NotificationCompat;

namespace Plugin.MediaManager
{
    internal class MediaNotificationManagerImplementation : IMediaNotificationManager
    {
        // private MediaSessionManagerImplementation _sessionHandler;
        private Intent _intent;
        private PendingIntent _pendingCancelIntent;
        private PendingIntent _pendingIntent;
        private NotificationCompat.MediaStyle _notificationStyle = new NotificationCompat.MediaStyle();
        private MediaSessionCompat.Token _sessionToken;
        private Context _appliactionContext;
        private NotificationCompat.Builder _builder;

        public MediaNotificationManagerImplementation(Context appliactionContext, MediaSessionCompat.Token sessionToken, Type serviceType)
        {
            _sessionToken = sessionToken;
            _appliactionContext = appliactionContext;
            _intent = new Intent(_appliactionContext, serviceType);
            var mainActivity =
                _appliactionContext.PackageManager.GetLaunchIntentForPackage(_appliactionContext.PackageName);
            _pendingIntent = PendingIntent.GetActivity(_appliactionContext, 0, mainActivity,
                PendingIntentFlags.UpdateCurrent);
        }

        /// <summary>
        /// Starts the notification.
        /// </summary>
        /// <param name="mediaFile">The media file.</param>
        public void StartNotification(IMediaFile mediaFile)
        {
            StartNotification(mediaFile, true, false);
        }

        /// <summary>
        /// When we start on the foreground we will present a notification to the user
        /// When they press the notification it will take them to the main page so they can control the music
        /// </summary>
        public void StartNotification(IMediaFile mediaFile, bool mediaIsPlaying, bool canBeRemoved)
        {
            var icon = (_appliactionContext.Resources?.GetIdentifier("xam_mediamanager_notify_ic", "drawable", _appliactionContext?.PackageName)).GetValueOrDefault(0);

            _notificationStyle.SetMediaSession(_sessionToken);
            _notificationStyle.SetCancelButtonIntent(_pendingCancelIntent);

            _builder = new NotificationCompat.Builder(_appliactionContext);
            _builder.SetSmallIcon(icon != 0 ? icon : _appliactionContext.ApplicationInfo.Icon);
            _builder.SetContentIntent(_pendingIntent);
            _builder.SetOngoing(mediaIsPlaying);
            _builder.SetVisibility(1);

            if (Build.Manufacturer.ToUpper() == "HUAWEI" &&
                (Build.VERSION.SdkInt == BuildVersionCodes.Lollipop ||
                 Build.VERSION.SdkInt == BuildVersionCodes.LollipopMr1))
            {
                // Custom media styles are not supported on some Huawei devices. It causes a RemoteServiceException: https://appcenter.ms/orgs/BCC-IT-Services/apps/BMM-Android/crashes/groups/e3d2f69acd6ea211f2fa9a0f6563e6cd9ddd9bec/crashes/195d8b23-4ca4-4d3b-8563-5218f37b8f15/raw
                // Also see https://stackoverflow.com/questions/46771521/android-app-remoteserviceexception-bad-notification-posted-on-huawei-y6
                // Therefore we don't set a custom style for those devices. It might be that we exclude too many devices but I think that's acceptable
            }
            else
            {
                _builder.SetStyle(_notificationStyle);
            }

            SetMetadata(mediaFile);
            AddActionButtons(mediaIsPlaying);
            if (_builder.MActions.Count >= 3)
            {
                (_builder.MStyle as NotificationCompat.MediaStyle)?.SetShowActionsInCompactView(0, 1, 2);
            }

            NotificationManagerCompat.From(_appliactionContext)
                .Notify(MediaServiceBase.NotificationId, _builder.Build());
        }


        public void StopNotifications()
        {
            NotificationManagerCompat nm = NotificationManagerCompat.From(_appliactionContext);
            nm.CancelAll();
        }

        public void UpdateNotifications(IMediaFile mediaFile, MediaPlayerStatus status)
        {
            try
            {
                var isPlaying = status == MediaPlayerStatus.Playing || status == MediaPlayerStatus.Buffering;
                var nm = NotificationManagerCompat.From(_appliactionContext);
                if (nm != null && _builder != null)
                {
                    SetMetadata(mediaFile);
                    AddActionButtons(isPlaying);
                    _builder.SetOngoing(isPlaying);
                    nm.Notify(MediaServiceBase.NotificationId, _builder.Build());
                }
                else
                {
                    StartNotification(mediaFile, isPlaying, false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                StopNotifications();
            }

        }

        private void SetMetadata(IMediaFile mediaFile)
        {
            _builder.SetContentTitle(mediaFile?.Metadata?.Title ?? string.Empty);
            _builder.SetContentText(mediaFile?.Metadata?.Artist ?? string.Empty);
            _builder.SetContentInfo(mediaFile?.Metadata?.Album ?? string.Empty);
            _builder.SetLargeIcon(mediaFile?.Metadata?.AlbumArt as Bitmap);
        }

        private Android.Support.V4.App.NotificationCompat.Action GenerateActionCompat(int icon, string title, string intentAction)
        {
            _intent.SetAction(intentAction);

            PendingIntentFlags flags = PendingIntentFlags.UpdateCurrent;
            if (intentAction.Equals(MediaServiceBase.ActionStop))
                flags = PendingIntentFlags.CancelCurrent;

            PendingIntent pendingIntent = PendingIntent.GetService(_appliactionContext, 1, _intent, flags);

            return new Android.Support.V4.App.NotificationCompat.Action.Builder(icon, title, pendingIntent).Build();
        }

        private void AddActionButtons(bool mediaIsPlaying)
        {
            _builder.MActions.Clear();
            _builder.AddAction(GenerateActionCompat(Resource.Drawable.IcMediaPrevious, "Previous", MediaServiceBase.ActionPrevious));
            _builder.AddAction(mediaIsPlaying
                ? GenerateActionCompat(Resource.Drawable.IcMediaPause, "Pause", MediaServiceBase.ActionPause)
                : GenerateActionCompat(Resource.Drawable.IcMediaPlay, "Play", MediaServiceBase.ActionPlay));
            _builder.AddAction(GenerateActionCompat(Resource.Drawable.IcMediaNext, "Next", MediaServiceBase.ActionNext));
        }
    }
}