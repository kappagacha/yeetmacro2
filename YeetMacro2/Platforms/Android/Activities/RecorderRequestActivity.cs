using Android.App;
using Android.Content;
using Android.Media.Projection;
using Android.Runtime;
using Android.Widget;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Activities;

[Activity(
    Name = "com.yeetoverflow.RecorderRequestActivity",
    Theme = "@android:style/Theme.Translucent.NoTitleBar",
    ExcludeFromRecents = true,
    NoHistory = true,
    Exported = false
)]
public class RecorderRequestActivity : Activity
{
    private const int REQUEST_MEDIA_PROJECTION = 3;

    protected override void OnCreate(global::Android.OS.Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Show context message if this is a re-request due to orientation change
        var isOrientationChange = Intent.GetBooleanExtra("orientation_change", false);
        if (isOrientationChange)
        {
            var activity = Platform.CurrentActivity;
            if (activity != null)
            {
                Toast.MakeText(
                    activity,
                    "Screen orientation changed - please grant screen capture permission again",
                    ToastLength.Long
                )?.Show();
            }
        }

        // Immediately request permission
        RequestMediaProjectionPermission();
    }

    private void RequestMediaProjectionPermission()
    {
        var mgr = (MediaProjectionManager)GetSystemService(Context.MediaProjectionService);
        if (mgr == null)
        {
            Finish();
            return;
        }

        var intent = mgr.CreateScreenCaptureIntent();
        StartActivityForResult(intent, REQUEST_MEDIA_PROJECTION);
    }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] global::Android.App.Result resultCode, global::Android.Content.Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode == REQUEST_MEDIA_PROJECTION)
        {
            if (resultCode == global::Android.App.Result.Ok && data != null)
            {
                // Initialize RecorderService with result
                var recorderService = ServiceHelper.GetService<RecorderService>();
                recorderService?.Init(resultCode, data);

                // Start foreground service will be triggered by RecorderService.Init()
            }

            // Always finish - whether granted or denied
            Finish();
        }
    }
}
