using System;
using System.IO;
using System.Net;
using System.Text;

namespace MTFClientServerCommon.RestApi
{
    public class RestApiClient
    {
        public static string MakeRequest(string endpoint, RestApiMethod method)
        {
            return MakeRequest(endpoint, method, null);
        }

        public static string MakeRequest(string endpoint, RestApiMethod method, string requestInput)
        {
            var endPoint = endpoint;
            var responseValue = string.Empty;

            var request = (HttpWebRequest)WebRequest.Create(endPoint);
            request.Method = method.ToString().ToLower();
            request.MediaType = "application/json";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.UseDefaultCredentials = true;
            request.PreAuthenticate = true;
            request.Credentials = CredentialCache.DefaultCredentials;

            if (method != RestApiMethod.Get && !string.IsNullOrEmpty(requestInput))
            {
                var requestStream = request.GetRequestStream();
                var bytes = Encoding.UTF8.GetBytes(requestInput);
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var status = response.StatusCode;

                    if (status != HttpStatusCode.OK)
                    {
                        throw new ApplicationException(string.Format("Error code: {0}", status));
                    }

                    using (var stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                responseValue = reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    var stream = ex.Response.GetResponseStream();
                    if (stream != null)
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            var result = sr.ReadToEnd();
                            throw new Exception(result);
                        }
                    }
                }
                throw;
            }

            return responseValue;
        }
    }
}