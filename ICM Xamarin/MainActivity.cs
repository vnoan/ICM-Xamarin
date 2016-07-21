namespace CameraAppDemo
{
    using System;
    using System.Collections.Generic;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Graphics;
    using Android.OS;
    using Android.Provider;
    using Android.Widget;
    using Java.IO;
    using Environment = Android.OS.Environment;
    using Uri = Android.Net.Uri;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using ModernHttpClient;
    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
    }

    [Activity(Label = "Camera App Demo", MainLauncher = true)]
    public class MainActivity : Activity
    {

        private ImageView _imageView;
        TextoParaVoz textoParaVoz;
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            //FileInputStream fileInputStream = null;

            //File file = App._file;

            //byte[] bFile = new byte[(int)file.Length()];
            //byte[] bImg = file.ToArray<byte>();
            //await ComputerVision.DescreverImagem(bImg);
            
            //try
            //{
            //    //convert file into array of bytes
            //    fileInputStream = new FileInputStream(file);
            //    fileInputStream.Read(bFile);
            //    fileInputStream.Close();
                
            //}
            //catch (Exception)
            //{

            //}

            Context c = this;
            // Make it available in the gallery
            textoParaVoz.Speak("Identificando imagem");
            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            
            Uri contentUri = Uri.FromFile(App._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            // Display in ImageView. We will resize the bitmap to fit the display
            // Loading the full sized image will consume to much memory 
            // and cause the application to crash.

            int height = Resources.DisplayMetrics.HeightPixels;
            int width = _imageView.Height;
            height /= 2;
            width /= 2;
            App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
            if (App.bitmap != null)
            {
                _imageView.SetImageBitmap(App.bitmap);

                byte[] bitmapData;
                using (var stream = new System.IO.MemoryStream())
                {
                    App.bitmap.Compress(Bitmap.CompressFormat.Jpeg, 99, stream);
                    bitmapData = stream.ToArray();
                }

                string JsonResposta = await ComputerVision.DescreverImagem(bitmapData);
                Rootobject resposta = JsonConvert.DeserializeObject<Rootobject>(JsonResposta);
                string descricao = resposta.description.captions[0].text;
                double confianca = resposta.description.captions[0].confidence;
                confianca *= 100;
                int conf = (int)confianca;
                if (confianca < 15)
                {
                    textoParaVoz.Speak("Confiança muito baixa");
                }
                else
                {
                    textoParaVoz.Speak($"Com confiança de {conf} porcento, eu vejo {descricao}");
                }

                App.bitmap = null;
            }

            // Dispose of the Java side bitmap.
            GC.Collect();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            textoParaVoz = new TextoParaVoz(this);
            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();

                Button button = FindViewById<Button>(Resource.Id.myButton);
                _imageView = FindViewById<ImageView>(Resource.Id.imageView1);
                button.Click += TakeAPicture;
            }

        }

        private void CreateDirectoryForPictures()
        {
            App._dir = new File(
                Environment.GetExternalStoragePublicDirectory(
                    Environment.DirectoryPictures), "CameraAppDemo");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            //Intent intent = new Intent(MediaStore.ActionImageCapture);
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            
            //FileInputStream fileInputStream = null;

            //File file = App._file;

            //byte[] bFile = new byte[(int)file.Length()];

            //try
            //{
            //    //convert file into array of bytes
            //    fileInputStream = new FileInputStream(file);
            //    fileInputStream.Read(bFile);
            //    fileInputStream.Close();
            //}
            //catch (Exception)
            //{

            //}


            intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(App._file));
            //StartActivity(intent);
            StartActivityForResult(intent, 0);
        }
    }

    class ComputerVision
    {
        public static async Task<string> DescreverImagem(byte[] data)
        {
            var client = new HttpClient(new NativeMessageHandler());

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "e27d0a628eee4ab8984a771eb470cc63");

            var uri = "https://api.projectoxford.ai/vision/v1.0/analyze?visualFeatures=Description";
            HttpResponseMessage response;
            // Request body
            string resposta = string.Empty;
            using (var content = new ByteArrayContent(data))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
                resposta = await response.Content.ReadAsStringAsync();
            }

            return resposta;
        }

        public static async void DownoadHttp()
        {
            var client = new HttpClient();
            byte[] resposta = await client.GetByteArrayAsync("https://clotildetavares.files.wordpress.com/2009/11/futebol.jpg");
            DescreverImagem(resposta);
        }
    }


    public class Rootobject
    {
        public Description description { get; set; }
        public string requestId { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Description
    {
        public string[] tags { get; set; }
        public Caption[] captions { get; set; }
    }

    public class Caption
    {
        public string text { get; set; }
        public float confidence { get; set; }
    }

    public class Metadata
    {
        public int width { get; set; }
        public int height { get; set; }
        public string format { get; set; }
    }

}