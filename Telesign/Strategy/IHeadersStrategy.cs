namespace Telesign.Strategy
{
    using System.Collections.Generic;

    public interface IHeadersStrategy
    {
        /// <summary>
        /// Generates the TeleSign REST API headers used to authenticate requests.
        ///
        /// Creates the canonicalized stringToSign and generates the HMAC signature.This is used to authenticate requests
        /// against the TeleSign REST API.
        ///
        /// See https://developer.telesign.com/docs/authentication for detailed API documentation.
        /// </summary>
        /// <param name="customerId">Your account customer_id.</param>
        /// <param name="apiKey">Your account api_key.</param>
        /// <param name="methodName">The HTTP method name of the request as a upper case string, should be one of 'POST', 'GET', 'PUT' or 'DELETE'.</param>
        /// <param name="resource">The partial resource URI to perform the request against.</param>
        /// <param name="urlEncodedFields">URL encoded HTTP body to perform the HTTP request with.</param>
        /// <param name="dateRfc2616">The date and time of the request formatted in rfc 2616.</param>
        /// <param name="nonce">A unique cryptographic nonce for the request.</param>
        /// <param name="userAgent">User Agent associated with the request.</param>
        /// <param name="contentType">Content type of the request.</param>
        /// <returns>A dictionary of HTTP headers to be applied to the request.</returns>

        Dictionary<string, string> GenerateHeaders(
            string customerId,
            string apiKey,
            string methodName,
            string resource,
            string urlEncodedFields,
            string dateRfc2616,
            string nonce,
            string userAgent,
            string contentType = null);
    }
}
