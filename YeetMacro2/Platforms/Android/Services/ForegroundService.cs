using Android.App;
using Android.Content;
using Android.Media.Projection;
using Android.OS;
using Android.Runtime;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Services;
[Service(Enabled = true, Exported = true, Name = "com.companyname.ForegroundService", ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaProjection)]
public class ForegroundService : Service
{
    MainActivity _context;
    MediaProjectionManager _mediaProjectionManager;
    public ForegroundService()
    {
        Console.WriteLine("[*****YeetMacro*****] ForegroundService Constructor Start");
        Console.WriteLine("[*****YeetMacro*****] ForegroundService Constructor End");
    }

    private void Init()
    {
        try
        {
            Console.WriteLine("[*****YeetMacro*****] ForegroundService Init");
            _context = (MainActivity)Platform.CurrentActivity;
            //_windowManagerService = App.GetService<IWindowManagerService>();
            //_mediaProjectionService = App.GetService<IMediaProjectionService>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] ForegroundService Exception");
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
        }
    }

    public override void OnCreate()
    {
        Console.WriteLine("[*****YeetMacro*****] ForegroundService OnCreate");
        Init();
        _mediaProjectionManager = (MediaProjectionManager)_context.GetSystemService(Context.MediaProjectionService);
        _context.StartActivityForResult(_mediaProjectionManager.CreateScreenCaptureIntent(), YeetMacro2.Platforms.Android.Services.MediaProjectionService.REQUEST_MEDIA_PROJECTION);
        base.OnCreate();
    }

    public const string FOREGROUND_CHANNEL_ID = "9001";
    public const string EXIT_ACTION = "EXIT";
    public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        var _windowManagerService = ServiceHelper.GetService<AndroidWindowManagerService>();
        var _mediaProjectionService = ServiceHelper.GetService<MediaProjectionService>();
        switch (intent.Action)
        {
            case EXIT_ACTION:
                StopForeground(true);
                _windowManagerService.Close(WindowView.ActionView);
                _windowManagerService.Close(WindowView.LogView);
                _windowManagerService.CloseOverlayWindow();
                _mediaProjectionService.Stop();
                Intent exitEvent = new Intent("com.companyname.ForegroundService.EXIT");
                _context.SendBroadcast(exitEvent);
                break;
            default:
                StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, GenerateNotification());
                _windowManagerService.ShowOverlayWindow();
                _windowManagerService.Show(WindowView.ActionView);
                break;
        }

        return StartCommandResult.Sticky;
    }

    //https://stackoverflow.com/questions/61079610/how-to-create-a-xamarin-foreground-service
    public Notification GenerateNotification()
    {
        if (global::Android.OS.Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return null;
        }

        // Building channel if API verion is 26 or above
        var intent = new Intent(_context, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.SingleTop);
        intent.PutExtra("Title", "Message");

        var pendingIntent = PendingIntent.GetActivity(_context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var notifBuilder = new Notification.Builder(_context, FOREGROUND_CHANNEL_ID)
            .SetContentTitle("YeetMacro")
            .SetPriority((int)Notification.PriorityHigh)
            //.SetContentText("Main Text Body")
            .SetSmallIcon(Resource.Drawable.dotnet_bot)
            .AddAction(BuildExitAction())
            .SetOngoing(true)
            .SetContentIntent(pendingIntent);

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
        

        return notifBuilder.Build();
    }

    //https://stackoverflow.com/questions/46862583/android-notification-button-addaction-to-make-a-toast-message-when-pressed
    //https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/services/foreground-services
    //https://docs.microsoft.com/en-us/samples/xamarin/monodroid-samples/applicationfundamentals-servicesamples-foregroundservicedemo/
    public Notification.Action BuildExitAction()
    {
        var exitIntent = new Intent(this, typeof(ForegroundService));
        exitIntent.SetAction(EXIT_ACTION);
        var exitPendingIntent = PendingIntent.GetService(this, 0, exitIntent, PendingIntentFlags.Immutable);

        var builder = new Notification.Action.Builder(Resource.Drawable.typicons_media_stop_outline,
                                          "Exit",
                                          exitPendingIntent);

        return builder.Build();
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