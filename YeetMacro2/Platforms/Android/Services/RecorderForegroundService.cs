using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using CommunityToolkit.Mvvm.Messaging;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Services;

[Service(Enabled = true, Exported = true, Name = "com.yeetoverflow.RecorderForegroundService", ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaProjection)]
public class RecorderForegroundService : Service
{
    public const string FOREGROUND_CHANNEL_ID = "9002";
    public const string EXIT_ACTION = "EXIT_RECORDER";
    public const int SERVICE_RUNNING_NOTIFICATION_ID = 10001;
    public bool IsRunning = false;

    public RecorderForegroundService()
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();
    }

    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        try
        {
            var recorderService = ServiceHelper.GetService<IRecorderService>();
            bool hasMediaProjection = recorderService?.IsInitialized == true;

            switch (intent?.Action)
            {
                case EXIT_ACTION:
                    recorderService?.StopRecording();
                    Stop();
                    break;
                default:
                    // On Android 35+, we MUST have MediaProjection permission before starting foreground service
                    // TypeNone is prohibited on API 35+
                    if (!hasMediaProjection)
                    {
                        StopSelf();
                        return StartCommandResult.NotSticky;
                    }

                    // Start with media projection type
                    Start();
                    break;
            }
        }
        catch (Exception ex)
        {
            ServiceHelper.LogService?.LogException(ex);
        }

        return StartCommandResult.Sticky;
    }

    public Notification GenerateNotification()
    {
        var context = Platform.CurrentActivity ?? global::Android.App.Application.Context;

        var intent = new Intent(context, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.SingleTop);
        intent.PutExtra("Title", "Recording");

        var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var notifBuilder = new NotificationCompat.Builder(context, FOREGROUND_CHANNEL_ID)
            .SetContentTitle("YeetMacro Recording")
            .SetContentText("Screen recording in progress")
            .SetPriority((int)NotificationCompat.PriorityHigh)
            .SetSmallIcon(Resource.Drawable.appicon2)
            .AddAction(BuildStopAction())
            .SetOngoing(true)
            .SetContentIntent(pendingIntent);

        // Building channel if API version is 26 or above
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            NotificationChannel notificationChannel = new(FOREGROUND_CHANNEL_ID, "Recording", NotificationImportance.High)
            {
                Importance = NotificationImportance.Low
            };
            notificationChannel.EnableLights(false);
            notificationChannel.EnableVibration(false);
            notificationChannel.SetShowBadge(true);

            if (context.GetSystemService(Context.NotificationService) is NotificationManager notifManager)
            {
                notifBuilder.SetChannelId(FOREGROUND_CHANNEL_ID);
                notifManager.CreateNotificationChannel(notificationChannel);
            }
        }
        return notifBuilder.Build();
    }

    public NotificationCompat.Action BuildStopAction()
    {
        var exitIntent = new Intent(this, typeof(RecorderForegroundService));
        exitIntent.SetAction(EXIT_ACTION);
        var exitPendingIntent = PendingIntent.GetService(this, 0, exitIntent, PendingIntentFlags.Immutable);

        var builder = new NotificationCompat.Action.Builder(Resource.Drawable.appicon2,
                                          "Stop Recording",
                                          exitPendingIntent);

        return builder.Build();
    }

    void Start()
    {
        if (IsRunning) return;

        this.IsRunning = true;

        // On Android 29+, we must specify the service type
        // On Android 35+, TypeNone is prohibited - we must use TypeMediaProjection
        if (OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, GenerateNotification(), global::Android.Content.PM.ForegroundService.TypeMediaProjection);
        }
        else
        {
            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, GenerateNotification());
        }

        WeakReferenceMessenger.Default.Send(this);
    }

    void Stop()
    {
        this.IsRunning = false;
        StopForeground(StopForegroundFlags.Remove);
        StopSelf();
        WeakReferenceMessenger.Default.Send(this);
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
