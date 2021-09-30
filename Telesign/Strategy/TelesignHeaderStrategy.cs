namespace Telesign.Strategy
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    public class TelesignHeaderStrategy : IHeadersStrategy
    {
        public Dictionary<string, string> GenerateHeaders(
            string customerId,
            string apiKey,
            string methodName,
            string resource,
            string urlEncodedFields,
            string dateRfc2616,
            string nonce,
            string userAgent, 
            string contentType = null)
        {
            if (dateRfc2616 == null)
            {
                dateRfc2616 = DateTime.UtcNow.ToString("r");
            }

            if (nonce == null)
            {
                nonce = Guid.NewGuid().ToString();
            }

            if (contentType == null)
            {
                if (methodName == "POST" || methodName == "PUT")
                    contentType = "application/x-www-form-urlencoded";
                else
                    contentType = "";
            }

            string authMethod = "HMAC-SHA256";

            StringBuilder stringToSignBuilder = new StringBuilder();

            stringToSignBuilder.Append($"{methodName}");
            stringToSignBuilder.Append($"\n{contentType}");
            stringToSignBuilder.Append($"\n{dateRfc2616}");
            stringToSignBuilder.Append($"\nx-ts-auth-method:{authMethod}");
            stringToSignBuilder.Append($"\nx-ts-nonce:{nonce}");

            if (!string.IsNullOrEmpty(contentType) && !string.IsNullOrEmpty(urlEncodedFields))
            {
                stringToSignBuilder.Append($"\n{urlEncodedFields}");
            }

            stringToSignBuilder.Append($"\n{resource}");

            string stringToSign = stringToSignBuilder.ToString();

            HMAC hasher = new HMACSHA256(Convert.FromBase64String(apiKey));

            string signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            string authorization = $"TSA {customerId}:{signature}";

            Dictionary<string, string> headers = new Dictionary<string, string>();

            headers.Add("Authorization", authorization);
            headers.Add("Date", dateRfc2616);
            headers.Add("Content-Type", contentType);
            headers.Add("x-ts-auth-method", authMethod);
            headers.Add("x-ts-nonce", nonce);

            if (userAgent != null)
            {
                headers.Add("User-Agent", userAgent);
            }

            return headers;
        }
    }
}
