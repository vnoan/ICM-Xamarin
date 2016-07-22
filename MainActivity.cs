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
    using CameraAppDemo.Tradutor;

    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
    }

    [Activity(Label = "App - RIDV", MainLauncher = true)]
    public class MainActivity : Activity
    {

        private ImageView _imageView;
        TextoParaVoz textoParaVoz;
        TranslateService tradutor;

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            
            Context c = this;
            textoParaVoz.Speak("Identificando imagem");
            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            
            Uri contentUri = Uri.FromFile(App._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            int altura = Resources.DisplayMetrics.HeightPixels;
            int largura = _imageView.Height;
            altura /= 2;
            largura /= 2;
            App.bitmap = App._file.Path.LoadAndResizeBitmap(largura, altura);
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
                RespostaComputerVision resposta = JsonConvert.DeserializeObject<RespostaComputerVision>(JsonResposta);
                string descricao = await tradutor.TranslateString(resposta.description.captions[0].text, "pt");
                double confianca = resposta.description.captions[0].confidence;
                confianca *= 100;
                int conf = (int)confianca;
                if (confianca < 15)
                {
                    textoParaVoz.Speak("Confiança muito baixa para descrever a imagem. Tire outra foto.");
                }
                else
                {
                    textoParaVoz.Speak($"Com confiança de {conf} porcento, eu vejo {descricao}");
                }

                //reconhecendo rosto.

                textoParaVoz.Speak("Reconhecendo rostos");

                string JsonRespostaFace = await FaceID.DescreverFace(bitmapData);
                RespostaFaceID[] respostaFace = JsonConvert.DeserializeObject<RespostaFaceID[]>(JsonRespostaFace);

                if (respostaFace.Length == 0)
                    textoParaVoz.Speak("Nenhum rosto identificado.");
                else {
                    var idade = respostaFace[0].faceAttributes.age;
                    string sexo = (respostaFace[0].faceAttributes.gender == "male") ? "homem" : "mulher";
                    textoParaVoz.Speak($"Eu vejo um {sexo} de aproximadamente {idade} anos");
                }


                App.bitmap = null;
            }

            // Dispose of the Java side bitmap.
            GC.Collect();
        }
        

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            tradutor = new TranslateService();

            SetContentView(Resource.Layout.Main);
            textoParaVoz = new TextoParaVoz(this);
            if (TemAppNativoCamera())
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

        private bool TemAppNativoCamera()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));

            intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(App._file));
            //StartActivity(intent);
            StartActivityForResult(intent, 0);
        }
    }

}