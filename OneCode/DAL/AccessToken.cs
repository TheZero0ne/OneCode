using System;
using System.Net;
using System.Text;

namespace OneCode {
    /// <summary>
    /// The AccessToken is in the DataAccessLayer to provide the AccessToken for the Azure-API for translation.
    /// </summary>
    class AccessToken {
        private static AccessToken instance;
        private string bearer;
        private DateTime bearerCreationTime;

        /// <summary>
        /// The Constructor is private because AccessToken follows the pattern of a Singleton
        /// </summary>
        private AccessToken() {
            SetBearer();
        }

        /// <summary>
        /// This Method is the only point to get an instance of AccessToken. Because the Construcor is private there will only be one Instance of this class at runtime at once.
        /// </summary>
        /// <returns>An instance of AccessToken</returns>
        public static AccessToken GetInstance() {
            if (instance == null)
                instance = new AccessToken();

            return instance;
        }

        /// <summary>
        /// Checks if the actual Bearer is valid, asks for a new one if not and provides the Bearer.
        /// </summary>
        /// <returns>The Bearer in the format "Bearer <<Bearer>>"</returns>
        public string GetBearer() {
            if ((DateTime.Now - bearerCreationTime).Minutes > 8)
                SetBearer();

            return "Bearer " + bearer;
        }

        /// <summary>
        /// Sets a new Bearer by calling the Azure-API
        /// </summary>
        private void SetBearer() {
            bearerCreationTime = DateTime.Now;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.cognitive.microsoft.com/sts/v1.0/issueToken");
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/jwt";
            httpWebRequest.Headers.Add("Ocp-Apim-Subscription-Key", "09b4b6d90d124e45b16427900078fab3");
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
