using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader.Android;

namespace XabeAndroidTest
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            await DoTest();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void RequestPermission()
        {

            if (ShouldShowRequestPermissionRationale(Android.Manifest.Permission.WriteExternalStorage))
            {

            }
            else
            {
                RequestPermissions(new string[] { Android.Manifest.Permission.WriteExternalStorage }, 1);
            }
        }
        private async Task<bool> DoTest()
        {
            RequestPermission();
            string ffmpegDirectory = Path.Combine(Application.Context.FilesDir.AbsolutePath, "FFmpeg");

            Directory.CreateDirectory(ffmpegDirectory);

            string mediaDirectory = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "FFmpegTest");

            Directory.CreateDirectory(mediaDirectory);

            string videoFileName = "test.mp4";
            string videoFileFullName = Path.Combine(mediaDirectory, videoFileName);
            string outputFileFullName = Path.Combine(mediaDirectory, "output.mp4");

            FFmpeg.SetExecutablesPath(ffmpegDirectory);
            EABIProvider eABIProvider = new EABIProvider();
            AndroidFFmpegDownloader downloader = new AndroidFFmpegDownloader(eABIProvider);

            await downloader.GetLatestVersion(FFmpeg.ExecutablesPath);

            CheckAndSetExecutable(FFmpeg.ExecutablesPath, "ffmpeg");
            CheckAndSetExecutable(FFmpeg.ExecutablesPath, "ffprobe");

            if (File.Exists(videoFileFullName))
            {
                IMediaInfo inputMediaInfo = await FFmpeg.GetMediaInfo(videoFileFullName);

                Log.Info("DoTest", $"Input File has {inputMediaInfo.VideoStreams.Count()} Video Streams");
                
                Toast.MakeText(Application.Context, $"Input File has {inputMediaInfo.VideoStreams.Count()} Video Streams", ToastLength.Long).Show();

                IConversionResult result = await (await FFmpeg.Conversions.FromSnippet.Convert(videoFileFullName, outputFileFullName)).Start();

                IMediaInfo outputMediaInfo = await FFmpeg.GetMediaInfo(outputFileFullName);

                Log.Info("DoTest", $"Output File has {outputMediaInfo.VideoStreams.Count()} Video Streams");

                Toast.MakeText(Application.Context, $"Output File has {outputMediaInfo.VideoStreams.Count()} Video Streams", ToastLength.Long).Show();
            }
            else
            {
                Log.Info("DoTest", "File doesn't exists.");
            }


            return true;

        }
        private void CheckAndSetExecutable(string directory, string fileName)
        {
            Java.IO.File myFile = new Java.IO.File(Path.Combine(directory, fileName));

            if (!myFile.CanExecute())
            {
                if (myFile.SetExecutable(true, false))
                {
                    Log.Info("CheckAndSetExecutable", "File is executable");
                }
                else
                {
                    Log.Info("CheckAndSetExecutable", "Failed to make the file executable");
                }
            }
            else
            {
                Log.Info("CheckAndSetExecutable", "File is executable");
            }
        }
    }
}
