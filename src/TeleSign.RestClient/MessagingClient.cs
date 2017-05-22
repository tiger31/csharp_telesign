﻿using System;
using System.Collections.Generic;
using System.Net;

namespace Telesign.Sdk
{
    class MessagingClient : RestClient
    {
        private const string MESSAGING_RESOURCE = "/v1/messaging";
        private const string MESSAGING_STATUS_RESOURCE = "/v1/messaging/{0}";

        public MessagingClient(string customerId, 
                               string apiKey, 
                               string restEndPoint, 
                               int? timeout = null, 
                               int? readWriteTimeout = null, 
                               WebProxy proxy = null, 
                               string proxyUsername = null, 
                               string proxyPassword = null) 
            : base(customerId, 
                   apiKey, 
                   restEndPoint, 
                   timeout, 
                   readWriteTimeout, 
                   proxy, 
                   proxyUsername, 
                   proxyPassword) { }

        /// <summary>
        /// Send a message to the target phone_number.See <a href ="https://developer.telesign.com/v2.0/docs/messaging-api">for detailed API documentation</a>.         
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        /// <param name="messageParams"></param>
        /// <returns></returns>
        public TeleSignResponse Message(string phoneNumber, string message, string messageType, Dictionary<string, string> parameters = null)
        {
            if (null == parameters)
                parameters = new Dictionary<string, string>();

            parameters.Add("phone_number", phoneNumber);
            parameters.Add("message", message);
            parameters.Add("message_type", messageType);
                        
            return Post(MESSAGING_RESOURCE, parameters);
        }

        /// <summary>
        /// Retrieves the current status of the message. See <a href="https://developer.telesign.com/v2.0/docs/messaging-api"> for detailed API documentation</a>.
        /// </summary>
        /// <param name="referenceId"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TeleSignResponse Status(string referenceId, Dictionary<string, string> parameters = null)
        {
            return Get(string.Format(MESSAGING_STATUS_RESOURCE, referenceId), parameters);
        }
    }
}
