using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace CameraAppDemo.Tradutor
{
    public class TranslateService
    {
        private readonly string _clientId = "TranslatorAPItoICM";
        private readonly string _clientSecret = "W9ruhrSLPaMhpjRp1fKNHcVmJ9qI17/5DTzNaJwboq0=";

        private readonly Uri _dataMarketUri = new Uri("https://datamarket.accesscontrol.windows.net/v2/OAuth2-13");

        private readonly HttpClient _client = new HttpClient();

        
        public async Task<string> TranslateString(string strSource, string language)
        {
            string auth = await GetAzureDataMarketToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth);
            var requestUri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" +
                                System.Net.WebUtility.UrlEncode(strSource) +
                                "&to=" + language;
            string strTransText = string.Empty;
            try
            {
                var strTranslated = await _client.GetStringAsync(requestUri);
                var xTranslation = XDocument.Parse(strTranslated);
                strTransText = xTranslation.Root?.FirstNode.ToString();
                if (strTransText == strSource)
                    return "";
                else
                    return strTransText;
            }
            catch (Exception ex)
            {
                //
            }
            return strTransText;
        }

        private async Task<string> GetAzureDataMarketToken()
        {
            var properties = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id",  _clientId},
                { "client_secret", _clientSecret },
                { "scope", "http://api.microsofttranslator.com" }
            };

            var authentication = new FormUrlEncodedContent(properties);
            var dataMarketResponse = await _client.PostAsync(_dataMarketUri, authentication);
            string response;
            if (!dataMarketResponse.IsSuccessStatusCode)
            {
                response = await dataMarketResponse.Content.ReadAsStringAsync();
                var error = Newtonsoft.Json.JsonConvert.DeserializeObject<JToken>(response);
                var err = error.Value<string>("error");
                var msg = error.Value<string>("error_description");
                throw new HttpRequestException($"Azure market place request failed: {err} {msg}");
            }
            response = await dataMarketResponse.Content.ReadAsStringAsync();
            var accessToken = Newtonsoft.Json.JsonConvert.DeserializeObject<DataMarketAccessToken>(response);
            return accessToken.access_token;
        }
    }
    
}