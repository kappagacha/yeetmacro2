﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace YeetMacro2.Platforms.Android.Services;

[Service(Enabled = true, Exported = true, Name = "com.yeetoverflow.ForegroundService", ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaProjection)]
public class ForegroundService : Service
{
    public const string FOREGROUND_CHANNEL_ID = "9001";
    public const string EXIT_ACTION = "EXIT";
    public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
    public bool IsRunning = false;
    public ForegroundService()
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();
    }

    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        switch (intent.Action)
        {
            case EXIT_ACTION:
                this.IsRunning = false;
                StopForeground(StopForegroundFlags.Remove);
                break;
            default:
                if (OperatingSystem.IsAndroidVersionAtLeast(29))
                {
                    StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, GenerateNotification(), global::Android.Content.PM.ForegroundService.TypeMediaProjection);
                }
                else
                {
                    StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, GenerateNotification());
                }
                this.IsRunning = true;
                break;
        }

        WeakReferenceMessenger.Default.Send(this);

        return StartCommandResult.NotSticky;
    }

    //https://stackoverflow.com/questions/61079610/how-to-create-a-xamarin-foreground-service
    public Notification GenerateNotification()
    {
        var intent = new Intent(Platform.CurrentActivity, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.SingleTop);
        intent.PutExtra("Title", "Message");

        var pendingIntent = PendingIntent.GetActivity(Platform.CurrentActivity, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var notifBuilder = new NotificationCompat.Builder(Platform.CurrentActivity, FOREGROUND_CHANNEL_ID)
            .SetContentTitle("YeetMacro")
            .SetPriority((int)NotificationCompat.PriorityHigh)
            //.SetContentText("Main Text Body")
            .SetSmallIcon(Resource.Drawable.appicon2)
            .AddAction(BuildExitAction())
            .SetOngoing(true)
            .SetContentIntent(pendingIntent);

        // Building channel if API verion is 26 or above
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            NotificationChannel notificationChannel = new(FOREGROUND_CHANNEL_ID, "Title", NotificationImportance.High)
            {
                Importance = NotificationImportance.Low
            };
            notificationChannel.EnableLights(true);
            notificationChannel.EnableVibration(true);
            notificationChannel.SetShowBadge(true);
            notificationChannel.SetVibrationPattern([100, 200, 300, 400, 500, 400, 300, 200, 400]);

            if (Platform.CurrentActivity.GetSystemService(Context.NotificationService) is NotificationManager notifManager)
            {
                notifBuilder.SetChannelId(FOREGROUND_CHANNEL_ID);
                notifManager.CreateNotificationChannel(notificationChannel);
            }
        }
        return notifBuilder.Build();
    }

    //https://stackoverflow.com/questions/46862583/android-notification-button-addaction-to-make-a-toast-message-when-pressed
    //https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/services/foreground-services
    //https://docs.microsoft.com/en-us/samples/xamarin/monodroid-samples/applicationfundamentals-servicesamples-foregroundservicedemo/
    public NotificationCompat.Action BuildExitAction()
    {
        var exitIntent = new Intent(this, typeof(ForegroundService));
        exitIntent.SetAction(EXIT_ACTION);
        var exitPendingIntent = PendingIntent.GetService(this, 0, exitIntent, PendingIntentFlags.Immutable);

        var builder = new NotificationCompat.Action.Builder(Resource.Drawable.appicon2,
                                          "Exit",
                                          exitPendingIntent);
        
        return builder.Build();
    }

    public override void OnRebind(Intent intent)
    {
        base.OnRebind(intent);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }
}