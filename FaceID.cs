using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using System.Threading.Tasks;
using ModernHttpClient;
using System.Net.Http.Headers;


namespace CameraAppDemo
{
    public class FaceID
    {
        public static async Task<string> DescreverFace(byte[] data)
        {
            var client = new HttpClient(new NativeMessageHandler());

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "7f1c1398073b4acb9fc1f0829289a090");

            var uri = "https://api.projectoxford.ai/face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender";
            HttpResponseMessage response;

            string resposta = string.Empty;
            using (var content = new ByteArrayContent(data))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
                System.Diagnostics.Debug.WriteLine("Codigo de resposta: " + response.StatusCode);
                //Falta tratar quando ele não reconhece a imagem. Isso é via código de resposta HTTP.

                resposta = await response.Content.ReadAsStringAsync();
                //resposta = resposta.Substring(1, resposta.Length - 2); //retirar os '[' e ']'
            }
            System.Diagnostics.Debug.WriteLine("A resposta foi: " + resposta);
            return resposta;
        }

    }

    public class RespostaFaceID
    {
        public string faceId { get; set; }
        public FaceRectangle faceRectangle { get; set; }
        public FaceAttributes faceAttributes { get; set; }
    }

    public class FaceRectangle
    {
        public int top { get; set; }
        public int left { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class FaceAttributes
    {
        public string gender { get; set; }
        public double age { get; set; }
    }
}