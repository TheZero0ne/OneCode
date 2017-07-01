using System;
using System.Web;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.IO;

namespace OneCode {
    class Translator {
        public static void TranslateString(string textToTranslate) {

            string from = "en";
            string to = "de";
            string uri = "https://api.microsofttranslator.com/V2/Http.svc/Translate?text=" + HttpUtility.UrlEncode(textToTranslate) + "&from=" + from + "&to=" + to;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", AccessToken.GetInstance().GetBearer());

            using (WebResponse response = httpWebRequest.GetResponse())

            using (Stream stream = response.GetResponseStream()) {
                DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
                string translation = (string)dcs.ReadObject(stream);
                //TODO: Übersetzung machen
            }
        }

        public static async void TranslateStringArray(string[] textArrayToTranslate) {
            List<string> translated = new List<string>();

            var from = "en";
            var to = "es";
            var uri = "https://api.microsofttranslator.com/v2/Http.svc/TranslateArray";
            var body = "<TranslateArrayRequest>" +
                           "<AppId />" +
                           "<From>{0}</From>" +
                           "<Options>" +
                           " <Category xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<ContentType xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\">{1}</ContentType>" +
                               "<ReservedFlags xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<State xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<Uri xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<User xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                           "</Options>" +
                           "<Texts>";

            foreach (string s in textArrayToTranslate)
                body += "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">" + s + "</string>";

            body +=
                           "</Texts>" +
                           "<To>{2}</To>" +
                       "</TranslateArrayRequest>";
            string requestBody = string.Format(body, from, "text/plain", to);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage()) {

                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "text/xml");
                request.Headers.Add("Authorization", AccessToken.GetInstance().GetBearer());

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                switch (response.StatusCode) {
                    case HttpStatusCode.OK:
                        var doc = XDocument.Parse(responseBody);
                        var ns = XNamespace.Get("http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2");
                        var sourceTextCounter = 0;

                        foreach (XElement xe in doc.Descendants(ns + "TranslateArrayResponse")) {
                            foreach (var node in xe.Elements(ns + "TranslatedText")) {
                                translated.Add(node.Value);
                            }
                            sourceTextCounter++;
                        }

                        break;
                    default:
                        //TODO: Exception hübsch machen
                        throw new Exception("Geht nicht");
                }

            }
        }
    }
}
