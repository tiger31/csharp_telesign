using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Telesign
{
    using Strategy;

    /// <summary>
    /// The TeleSign RestClient is a generic HTTP REST client that can be extended to make requests against any of
    /// TeleSign's REST API endpoints.
    /// 
    /// See https://developer.telesign.com for detailed API documentation.
    /// </summary>
    public class RestClient : IDisposable
    {
        /// <summary>
        /// Default Telesign User-Agent header 
        /// </summary>
        public static readonly string UserAgent =
            $"TeleSignSdk/csharp-2.2.2 .Net/{Environment.Version} HttpClient";

        /// <summary>
        /// Telesign customerId
        /// </summary>
        protected readonly string CustomerId;

        /// <summary>
        /// Telesign apiKey (secretKey)
        /// </summary>
        protected readonly string ApiKey;

        /// <summary>
        /// URI to Telesign API location
        /// </summary>
        protected readonly string RestEndpoint;


        /// <summary>
        /// Default http client
        /// </summary>
        protected readonly HttpClient HttpClient;


        /// <summary>
        /// Strategy that provides method to generates TeleSign REST API headers used to authenticate requests
        /// </summary>
        protected IHeadersStrategy Strategy;

        private bool _disposed;

        /// <summary>
        /// TeleSign RestClient useful for making generic RESTful requests against our API.
        /// </summary>
        /// <param name="customerId">Your customer_id string associated with your account.</param>
        /// <param name="apiKey">Your api_key string associated with your account.</param>
        /// <param name="restEndpoint">Override the default restEndpoint to target another endpoint.</param>
        /// <param name="timeout">The timeout in seconds passed into HttpClient.</param>
        /// <param name="proxy">The proxy passed into HttpClient.</param>
        /// <param name="proxyUsername">The username passed into HttpClient.</param>
        /// <param name="proxyPassword">The password passed into HttpClient.</param>
        /// <param name="strategy">Method used to generates TeleSign REST API headers used to authenticate requests</param>
        /// <param name="client">Instance of HttpClient. Allows it to be injected from IHttpClientFactory</param>
        public RestClient(string customerId,
                          string apiKey,
                          string restEndpoint = "https://rest-api.telesign.com",
                          int timeout = 10,
                          IWebProxy proxy = null,
                          string proxyUsername = null,
                          string proxyPassword = null,
                          IHeadersStrategy strategy = null,
                          HttpClient client = null)
        {
            this.CustomerId = customerId;
            this.ApiKey = apiKey;
            this.RestEndpoint = restEndpoint;

            this.Strategy = strategy ?? new TelesignHeaderStrategy();

            if (client != null)
            {
                this.HttpClient = client;
            }

            if (proxy == null)
            {
                this.HttpClient = new HttpClient();
            }
            else
            {
                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = proxy
                };

                if (proxyUsername != null && proxyPassword != null)
                {
                    httpClientHandler.Credentials = new NetworkCredential(proxyUsername, proxyPassword);
                }

                this.HttpClient = new HttpClient(httpClientHandler);
            }

            this.HttpClient.Timeout = TimeSpan.FromSeconds(timeout);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets header generation strategy
        /// </summary>
        /// <param name="strategy">Header strategy</param>
        public void SetHeaderStrategy(IHeadersStrategy strategy)
        {
            this.Strategy = strategy;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            this.HttpClient.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// A simple HTTP Response object to abstract the underlying HttpClient library response.
        /// </summary>
        public class TelesignResponse
        {
            private Task<string> BodyTask;
            
            /// <summary>
            /// Telesign response object
            /// </summary>
            /// <param name="response"></param>
            /// <param name="isAsync"></param>
            public TelesignResponse(HttpResponseMessage response, bool isAsync = false)
            {
                this.StatusCode = (int)response.StatusCode;
                this.Headers = response.Headers;
                this.OK = response.IsSuccessStatusCode;

                if (isAsync)
                {
                    this.BodyTask = response.Content.ReadAsStringAsync();
                }
                else
                {
                    this.Body = response.Content.ReadAsStringAsync().Result;

                    try
                    {
                        this.Json = JObject.Parse(this.Body);
                    }
                    catch (JsonReaderException)
                    {
                        this.Json = new JObject();
                    }    
                }
                
            }

            /// <summary>
            /// Asynchronously parses response body into Json property
            /// </summary>
            public async Task Initialize()
            {
                this.Body = await BodyTask.ConfigureAwait(false) ;
                try
                {
                    this.Json = JObject.Parse(this.Body);
                }
                catch (JsonReaderException)
                {
                    this.Json = new JObject();
                }
            }

            public int StatusCode { get; set; }
            public HttpResponseHeaders Headers { get; set; }
            public string Body { get; set; }
            public JObject Json { get; set; }
            public bool OK { get; set; }
        }

        /// <summary>
        /// Generic TeleSign REST API POST handler.
        /// </summary>
        /// <param name="resource">The partial resource URI to perform the request against.</param>
        /// <param name="parameters">Body params to perform the POST request with.</param>
        /// <returns>The TelesignResponse for the request.</returns>
        public TelesignResponse Post(string resource, Dictionary<string, string> parameters)
        {
            return Execute(resource, HttpMethod.Post, parameters);
        }
        
        
        /// <summary>
        /// Generic TeleSign REST API POST handler.
        /// </summary>
        /// <param name="resource">The partial resource URI to perform the request against.</param>
        /// <param name="parameters">Body params to perform the POST request with.</param>
        /// <returns>The TelesignResponse for the request.</returns>
        public Task<TelesignResponse> PostAsync(string resource, Dictionary<string, string> parameters)
        {
            return ExecuteAsync(resource, HttpMethod.Post, parameters);
        }

        /// <summary>
        /// Generic TeleSign REST API GET handler.
        /// </summary>
        /// <param name="resource">The partial resource URI to perform the request against.</param>
        /// <param name="parameters">Body params to perform the GET request with.</param>
        /// <returns>The TelesignResponse for the request.</returns>
        public TelesignResponse Get(string resource, Dictionary<string, string> parameters)
        {
            return Execute(resource, HttpMethod.Get, parameters);
        }
        
        /// <summary>
        /// Generic TeleSign REST API GET handler.
        /// </summary>
        /// <param name="resource">The partial resource URI to perform the request against.</param>
        /// <param name="parameters">Body params to perform the GET request with.</param>
        /// <returns>The TelesignResponse for the request.</returns>
        public Task<TelesignResponse> GetAsync(string resource, Dictionary<string, string> parameters)
        {
            return ExecuteAsync(resource, HttpMethod.Get, parameters);
        }

        /// <summary>
        /// Generic TeleSign REST API PUT handler.
        /// </summary>
        /// <param name="resource">The partial resource URI to perform the request against.</param>
        /// <param name="parameters">Body params to perform the PUT request with.</param>
        /// <returns>The TelesignResponse for the request.</returns>
        public TelesignResponse Put(string resource, Dictionary<string, string> parameters)
        {
            return Execute(resource, HttpMethod.Put, parameters);
        }

        /// <summary>
        /// Generic TeleSign REST API PUT handler.
        /// </summary>
        /// <param name="resource">The partial resource URI to perform the request against.</param>
        /// <param name="parameters">Body params to perform the PUT request with.</param>
        /// <returns>The TelesignResponse for the request.</returns>
        public Task<TelesignResponse> PutAsync(string resource, Dictionary<string, string> parameters)
        {
            return ExecuteAsync(resource, HttpMethod.Put, parameters);
        }
        
        /// <summary>
        /// Generic TeleSign REST API DELETE handler.
        /// </summary>
        /// <param name="resource">The partial resource URI to perform the request against.</param>
        /// <param name="parameters">Body params to perform the DELETE request with.</param>
        /// <returns>The TelesignResponse for the request.</returns>
        public TelesignResponse Delete(string resource, Dictionary<string, string> parameters)
        {
            return Execute(resource, HttpMethod.Delete, parameters);
        }

        /// <summary>
        /// Generic TeleSign REST API DELETE handler.
        /// </summary>
        /// <param name="resource">The partial resource URI to perform the request against.</param>
        /// <param name="parameters">Body params to perform the DELETE request with.</param>
        /// <returns>The TelesignResponse for the request.</returns>
        public Task<TelesignResponse> DeleteAsync(string resource, Dictionary<string, string> parameters)
        {
            return ExecuteAsync(resource, HttpMethod.Delete, parameters);
        }
        
        /// <summary>
        /// Generic TeleSign REST API request handler.
        /// </summary>
        /// <param name="resource">The partial resource URI to perform the request against.</param>
        /// <param name="method">The HTTP method name, as an upper case string.</param>
        /// <param name="parameters">Params to perform the request with.</param>
        /// <returns></returns>
        private TelesignResponse Execute(string resource, HttpMethod method, Dictionary<string, string> parameters)
        {
            return ExecuteAsync(resource, method, parameters).Result;
        }

        private async Task<TelesignResponse> ExecuteAsync(string resource, HttpMethod method, Dictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }

            var resourceUri = $"{this.RestEndpoint}{resource}";

            var formBody = new FormUrlEncodedContent(parameters);
            var urlEncodedFields = await formBody.ReadAsStringAsync().ConfigureAwait(false);

            HttpRequestMessage request;
            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                request = new HttpRequestMessage(method, resourceUri)
                {
                    Content = formBody
                };
            }
            else
            {
                var resourceUriWithQuery = new UriBuilder(resourceUri)
                {
                    Query = urlEncodedFields
                };
                request = new HttpRequestMessage(method, resourceUriWithQuery.ToString());
            }

            var headers = this.Strategy.GenerateHeaders(this.CustomerId,
                                                                         this.ApiKey,
                                                                         method.ToString().ToUpper(),
                                                                         resource,
                                                                         urlEncodedFields,
                                                                         null,
                                                                         null,
                                                                         RestClient.UserAgent);

            foreach (var header in headers)
            {
                if (header.Key == "Content-Type")
                    // skip Content-Type, otherwise HttpClient will complain
                    continue;

                request.Headers.Add(header.Key, header.Value);
            }

            var response = await this.HttpClient.SendAsync(request).ConfigureAwait(false);

            var tsResponse = new TelesignResponse(response, true);
            await tsResponse.Initialize();
            return tsResponse;
        }
    }
}
