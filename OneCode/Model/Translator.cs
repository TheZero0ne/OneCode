using System;
using System.Web;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.IO;
using OneCode.Properties;
using System.Threading.Tasks;
using System.Windows;

namespace OneCode {
    /// <summary>
    /// The Translator provides methods to translate strings or arrays of strings.
    /// </summary>
    static class Translator {

        /// <summary>
        /// Translates a string from and into a language which was configured in the settings.
        /// </summary>
        /// <param name="textToTranslate">A string which should be translated</param>
        /// <returns>The translated string</returns>
        public static async Task<string> TranslateString(string textToTranslate) {

            // If the text needs to be separated for translation automaticly use the dictonary method
            if (textToTranslate != null && VariableFormatter.SplitString(textToTranslate).Contains(" ")) {
                return await TranslateAsDictonary(textToTranslate);
            }

            // building the request for the Azure-Api
            string translatedString = "";
            string from = "" + Settings.Default.BaseLanguage;
            string to = "" + Settings.Default.CodeLanguage;
            string uri = "https://api.microsofttranslator.com/V2/Http.svc/Translate?text=" + HttpUtility.UrlEncode(textToTranslate) + "&from=" + from + "&to=" + to;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", AccessToken.GetInstance().GetBearer());

            // calling the Azure-Api
            using (WebResponse response = httpWebRequest.GetResponse())

            using (Stream stream = response.GetResponseStream()) {
                DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
                translatedString = (string)dcs.ReadObject(stream);
            }

            return translatedString;
        }

        /// <summary>
        /// Translates the given string into a Dictionary
        /// </summary>
        /// <param name="textToTranslate">A string which should be translated</param>
        /// <returns>The translated string</returns>
        private static async Task<string> TranslateAsDictonary(string textToTranslate) {
            VariableCollection varCollection = new VariableCollection();

            // add textToTranslate
            varCollection.Add(new Variable("", textToTranslate, "", 0, ""));

            var dictionary = varCollection.GetNamesDictionaryForTranslation();
            Task<Dictionary<int, VariableNameInfo>> translationTask = TranslateDictionary(dictionary);
            Dictionary<int, VariableNameInfo> translationDic = await translationTask;

            varCollection.ApplyTranslationDictionary(translationDic);

            return varCollection[0].Translation.GetContentWithPrefix();
        }

        /// <summary>
        /// Translates a Dictionary of strings into a language which was configured in the settings.
        /// </summary>
        /// <param name="dictionaryToTranslate">The Dictionary which should be translated</param>
        /// <returns>A Task of the translated Dictionary</returns>
        public static async Task<Dictionary<int, VariableNameInfo>> TranslateDictionary(Dictionary<int, VariableNameInfo> dictionaryToTranslate) {
            Dictionary<int, VariableNameInfo> translated = new Dictionary<int, VariableNameInfo>();

            // building the request-body for the call of the Azure-Api
            var from = "" + Settings.Default.BaseLanguage;
            var to = "" + Settings.Default.CodeLanguage;
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

            foreach (VariableNameInfo s in dictionaryToTranslate.Values)
                body += "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">" + s.Content + "</string>";

            body +=
                           "</Texts>" +
                           "<To>{2}</To>" +
                       "</TranslateArrayRequest>";
            string requestBody = string.Format(body, from, "text/plain", to);

            // calling the Azure-Api
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

                        var keys = dictionaryToTranslate.Keys.GetEnumerator();
                        foreach (XElement xe in doc.Descendants(ns + "TranslateArrayResponse")) {
                            foreach (var node in xe.Elements(ns + "TranslatedText")) {
                                keys.MoveNext();
                                translated.Add(keys.Current, new VariableNameInfo(node.Value, dictionaryToTranslate[keys.Current].Prefix));
                            }
                            sourceTextCounter++;
                        }

                        return translated;
                    default:
                        MessageBox.Show("Bei der Übersetzung ist ein Fehler in der Übersetzungs-API aufgetreten.");
                        break;
                }

            }
        }
    }
}
