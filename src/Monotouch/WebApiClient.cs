using System.Dynamic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

namespace SimpleWebApi
{
    /// <summary>
    /// Provides a simple async way to make requests to WebApi methods that return Json and ultimately strongly typed results.
    /// </summary>
    public class WebApiClient
    {
        /// <summary>
        /// Calls WebApi with HTTP POST.
        /// </summary>
        /// <typeparam name="TReturnType">The strongly typed object that the WebApi returns.</typeparam>
        /// <param name="apiUrl">The full URL inlcuding any querystring parameters.</param>
        /// <param name="authToken">An 'AuthToken' header is added to the request with this parameter's value.</param>
        /// <param name="data">The data to post.</param>
        /// <returns>A strongly typed object that was returned from te WebApi call.</returns>

        public static Task<TReturnType> Post<TReturnType>(string apiUrl, string authToken, string data)
        {
            var webClient = new WebClient();
            var uri = new Uri(apiUrl);
            webClient.Headers["AuthToken"] = authToken;

            return Post<TReturnType>(webClient, uri, data);
        }

        /// <summary>
        /// Calls WebApi with HTTP POST.
        /// </summary>
        /// <typeparam name="TReturnType">The strongly typed object that the WebApi returns.</typeparam>
        /// <param name="apiUrl">The full URL inlcuding any querystring parameters.</param>
        /// <param name="authToken">An 'AuthToken' header is added to the request with this parameter's value.</param>
        /// <param name="request">The data to post as a request object.</param>
        /// <returns>A strongly typed object that was returned from te WebApi call.</returns>
        public static Task<TReturnType> Post<TReturnType, TRequestType>(string apiUrl, string authToken, TRequestType request)
        {
            var data = JsonConvert.SerializeObject(request);
            return Post<TReturnType>(apiUrl, authToken, data);
        }

        /// <summary>
        /// Calls WebApi with HTTP POST.
        /// </summary>
        /// <typeparam name="TReturnType">The strongly typed object that the WebApi returns.</typeparam>
        /// <param name="apiUrl">The full URL inlcuding any querystring parameters.</param>
        /// <param name="headers">A dictionary of Http headers to add to the request.  Key, Value</param>
        /// <param name="data">The data to post.</param>
        /// <returns>A strongly typed object that was returned from te WebApi call.</returns>
        public static Task<TReturnType> Post<TReturnType>(string apiUrl, Dictionary<string,string> headers, string data)
        {
            var webClient = new WebClient();
            var uri       = new Uri(apiUrl);

            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    webClient.Headers[key] = headers[key];
                }
            }

            return Post<TReturnType>(webClient, uri, data);
        }

        /// <summary>
        /// This method actually makes the request, deserializes the JSON, and returns the result.
        /// </summary>
        /// <typeparam name="TReturnType">Strongly typed object returned from WebApi call.</typeparam>
        /// <param name="webClient">The instance of the WebClient.</param>
        /// <param name="uri">The Uri to post to.</param>
        /// <param name="data">The data being posted in the body of the request.</param>
        /// <returns>A strongly typed object returned from the WebApi call.</returns>
        private static Task<TReturnType> Post<TReturnType>(WebClient webClient, Uri uri, string data)
        {
            var returnObject = default(TReturnType);

            var taskCompletionSource = new TaskCompletionSource<TReturnType>();

            webClient.Headers["Content-Type"] = "application/json";

            webClient.UploadStringCompleted += (s, e) =>
            {
                var result = e.Result;

                try
                {
                    returnObject = JsonConvert.DeserializeObject<TReturnType>(result);

                    taskCompletionSource.SetResult(returnObject);
                }
                catch (Exception ex)
                {
                    var newEx = new Exception(string.Format("Failed to deserialize server response: {0}", result), ex);
                    taskCompletionSource.SetException(newEx);
                }
            };

            webClient.UploadStringAsync(uri, data);

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Calls WebApi with HTTP GET.
        /// </summary>
        /// <typeparam name="TReturnType">The type of the return type.</typeparam>
        /// <param name="apiUrl">The API URL.</param>
        /// <param name="authToken">The auth token.</param>
        /// <returns>A strongly typed object returned from the WebApi call.</returns>
        public static Task<TReturnType> Get<TReturnType>(string apiUrl, string authToken, int i)
        {
            var webClient = new WebClient();

            var uri = new Uri(apiUrl);
            webClient.Headers["AuthToken"] = authToken;

            return Get<TReturnType>(webClient, uri);
        }

        /// <summary>
        /// Gets the specified web client.
        /// </summary>
        /// <typeparam name="TReturnType">The type of the return type.</typeparam>
        /// <param name="apiUrl">The URL to the webapi call.</param>
        /// <param name="queryStringParameters">Parameters for the get which are placed on the Uri as querystring parameters. E.g. http://someuri/somepath?param1=value1&param2=value2
        ///     Pass a dynamic object in here and the properties are mapped to the querystring.  If you pass in a querystring "?param1=value1&param2=value2" then 
        ///     that string will be appended to the uri.  Please note that urlencoding is your own responsibility if you pass in the querystring as a string.  When
        ///     a dynamic object is passed in the name and values are UrlEncoded automatically.
        /// </param>
        /// <returns>A strongly typed object returned from the WebApi call.</returns>
        public static Task<TReturnType> Get<TReturnType>(string apiUrl, Dictionary<string, string> headers = null, dynamic queryStringParameters = null)
        {
            var parameters = queryStringParameters as object;
            string queryString;

            if (parameters != null && !(parameters is string))
            {
                var queryStringBuilder = new StringBuilder("?");

                foreach (var property in parameters.GetType().GetProperties())
                {
                    queryStringBuilder.AppendFormat("{0}={1}&", property.Name, property.GetValue(parameters, null));
                }

                queryString = queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1).ToString();
            }
            else
            {
                queryString = queryStringParameters as string;
            }

            var webClient = new WebClient();

            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    webClient.Headers[key] = headers[key];
                }
            }

            var uri = new Uri(apiUrl + queryString);

            return Get<TReturnType>(webClient, uri);
        }

        /// <summary>
        /// Gets the specified web client.
        /// </summary>
        /// <typeparam name="TReturnType">The type of the return type.</typeparam>
        /// <param name="webClient">The web client.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>A strongly typed object returned from the WebApi call.</returns>
        private static Task<TReturnType> Get<TReturnType>(WebClient webClient, Uri uri)
        {
            var returnObject = default(TReturnType);

            var taskCompletionSource = new TaskCompletionSource<TReturnType>();

            webClient.Headers["Accept"] = "application/json";

            webClient.DownloadStringCompleted += (s, e) =>
            {
                var result = e.Result;

                try
                {
                    returnObject = JsonConvert.DeserializeObject<TReturnType>(result);

                    taskCompletionSource.SetResult(returnObject);
                }
                catch (Exception ex)
                {
                    var newEx = new Exception(string.Format("Failed to deserialize server response: {0}", result), ex);
                    taskCompletionSource.SetException(newEx);
                }
            };

            webClient.DownloadStringAsync(uri);

            return taskCompletionSource.Task;
        }
    }
}
