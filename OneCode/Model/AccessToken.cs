using System;
using System.Net;
using System.Text;

namespace OneCode {
    class AccessToken {
        private static AccessToken instance;
        private string bearer;
        private DateTime bearerCreationTime;

        private AccessToken() {
            SetBearer();
        }

        public static AccessToken GetInstance() {
            if (instance == null)
                instance = new AccessToken();

            return instance;
        }

        public string GetBearer() {
            if ((DateTime.Now - bearerCreationTime).Minutes > 8)
                SetBearer();

            return "Bearer " + bearer;
        }

        private void SetBearer() {
            bearerCreationTime = DateTime.Now;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.cognitive.microsoft.com/sts/v1.0/issueToken");
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/jwt";
            httpWebRequest.Headers.Add("Ocp-Apim-Subscription-Key", "");
            httpWebRequest.ContentLength = 0;

            using (WebResponse response = httpWebRequest.GetResponse()) {
                WebHeaderCollection header = response.Headers;

                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding)) {
                    bearer = reader.ReadToEnd();
                }
            }
        }
    }
}
