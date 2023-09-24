using Android.App;
using Android.Content;
using Android.Media.Projection;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Services;

[Service(Enabled = true, Exported = true, Name = "com.companyname.ForegroundService", ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaProjection)]
public class ForegroundService : Service
{
    public const string FOREGROUND_CHANNEL_ID = "9001";
    public const string EXIT_ACTION = "EXIT";
    public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
    MainActivity _context;
    MediaProjectionManager _mediaProjectionManager;
    Lazy<AndroidWindowManagerService> _windowManagerService;
    Lazy<MediaProjectionService> _mediaProjectionService;

    public ForegroundService()
    {
        Console.WriteLine("[*****YeetMacro*****] ForegroundService Constructor");
    }

    public override void OnCreate()
    {
        Console.WriteLine("[*****YeetMacro*****] ForegroundService OnCreate");
        _context = (MainActivity)Platform.CurrentActivity;
        Console.WriteLine("[*****YeetMacro*****] ForegroundService set _windowManagerService");
        _windowManagerService = ServiceHelper.GetService<Lazy<AndroidWindowManagerService>>();
        Console.WriteLine("[*****YeetMacro*****] ForegroundService set _mediaProjectionService");
        _mediaProjectionService = ServiceHelper.GetService<Lazy<MediaProjectionService>>();
        Console.WriteLine("[*****YeetMacro*****] ForegroundService set _mediaProjectionManager");
        _mediaProjectionManager = (MediaProjectionManager)_context.GetSystemService(Context.MediaProjectionService);
        Console.WriteLine("[*****YeetMacro*****] ForegroundService start screen capture");
        _context.StartActivityForResult(_mediaProjectionManager.CreateScreenCaptureIntent(), YeetMacro2.Platforms.Android.Services.MediaProjectionService.REQUEST_MEDIA_PROJECTION);
        base.OnCreate();
    }

    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        switch (intent.Action)
        {
            case EXIT_ACTION:
                StopForeground(StopForegroundFlags.Remove);
                _windowManagerService.Value.Close(AndroidWindowView.ActionView);
                _windowManagerService.Value.Close(AndroidWindowView.StatusPanelView);
                _windowManagerService.Value.CloseOverlayWindow();
                _mediaProjectionService.Value.Stop();
                _mediaProjectionService.Value.StopRecording();
                Intent exitEvent = new Intent("com.companyname.ForegroundService.EXIT");
                _context.SendBroadcast(exitEvent);
                break;
            default:
                StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, GenerateNotification());
                _mediaProjectionService.Value.Start();
                _windowManagerService.Value.ShowOverlayWindow();
                break;
        }

        return StartCommandResult.Sticky;
    }

    //https://stackoverflow.com/questions/61079610/how-to-create-a-xamarin-foreground-service
    public Notification GenerateNotification()
    {
        var intent = new Intent(_context, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.SingleTop);
        intent.PutExtra("Title", "Message");

        var pendingIntent = PendingIntent.GetActivity(_context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var notifBuilder = new NotificationCompat.Builder(_context, FOREGROUND_CHANNEL_ID)
            .SetContentTitle("YeetMacro")
            .SetPriority((int)NotificationCompat.PriorityHigh)
            //.SetContentText("Main Text Body")
            .SetSmallIcon(Resource.Drawable.appicon)
            .AddAction(BuildExitAction())
            .SetOngoing(true)
            .SetContentIntent(pendingIntent);

        // Building channel if API verion is 26 or above
        if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            NotificationChannel notificationChannel = new NotificationChannel(FOREGROUND_CHANNEL_ID, "Title", NotificationImportance.High);
            notificationChannel.Importance = NotificationImportance.Low;
            notificationChannel.EnableLights(true);
            notificationChannel.EnableVibration(true);
            notificationChannel.SetShowBadge(true);
            notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300, 400, 500, 400, 300, 200, 400 });

            var notifManager = _context.GetSystemService(Context.NotificationService) as NotificationManager;
            if (notifManager != null)
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

        var builder = new NotificationCompat.Action.Builder(Resource.Drawable.appicon,
                                          "Exit",
                                          exitPendingIntent);

        return builder.Build();
    }

    public override void OnRebind(Intent intent)
    {
        Console.WriteLine("[*****YeetMacro*****] ForegroundService OnRebind");
        base.OnRebind(intent);
    }

    public override void OnDestroy()
    {
        Console.WriteLine("[*****YeetMacro*****] ForegroundService OnDestroy");
        base.OnDestroy();
    }

    public override IBinder OnBind(Intent intent)
    {
        Console.WriteLine("[*****YeetMacro*****] ForegroundService OnBind");
        return null;
    }
}