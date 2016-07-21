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
    public class ComputerVision
    {
        public static async Task<string> DescreverImagem(byte[] data)
        {
            var client = new HttpClient(new NativeMessageHandler());

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "e27d0a628eee4ab8984a771eb470cc63");

            var uri = "https://api.projectoxford.ai/vision/v1.0/analyze?visualFeatures=Description";
            HttpResponseMessage response;

            string resposta = string.Empty;
            using (var content = new ByteArrayContent(data))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
                resposta = await response.Content.ReadAsStringAsync();
            }

            return resposta;
        }

    }

    public class RespostaComputerVision
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