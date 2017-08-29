/**
* Copyright (C) 2017 Squizz PTY LTD
* This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with this program.  If not, see http://www.gnu.org/licenses/.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcommerceStandardsDocuments;
using System.Web;

namespace Squizz.Platform.API.v1.endpoint
{
    /// <summary>
    ///     Class handles calling the SQUIZZ.com API endpoint for raising organisation notifications within the platform.  
    ///     Organisation notifications are sent to selected people assigned to the organisation's notification categories
    /// </summary>
    public class APIv1EndpointOrgCreateNotification 
    {
        public const string NOTIFY_CATEGORY_ORG = "org";
        public const string NOTIFY_CATEGORY_ACCOUNT = "account";
        public const string NOTIFY_CATEGORY_ORDER_SALE = "order_sale";
        public const string NOTIFY_CATEGORY_ORDER_PURCHASE = "order_purchase";
        public const string NOTIFY_CATEGORY_FEED = "feed";
        public const int MAX_MESSAGE_PLACEHOLDERS = 5;
    
        /**
         * Calls the platform's API endpoint to create an organisation notification and notify selected people assigned to an organisation's notification category
         * To allow notifications to be sent to the platform the organisation must have sufficient trading tokens
         * @param apiOrgSession existing organisation API session
         * @param endpointTimeoutMilliseconds amount of milliseconds to wait after calling the the API before giving up, set a positive number
         * @param notifyCategory notification category that the notification appears within for the organisation's people. Set to one of the NOTIFY_CATEGORY_ constants
         * @param message message to display in the notification. Put placeholders in message {1}, {2}, {3}, {4}, {5} to replace with links or labels
         * @param linkURLs ordered array of URLs to replace in each of the place holders of the message. Set empty strings to ignore placing values into place holders
         * @param linkLabels ordered array of labels to replace in each of the place holders of the message. Set empty strings to ignore placing values into place holders
         * @return response from calling the API endpoint
         */
        public static APIv1EndpointResponseESD<ESDocument> call(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string notifyCategory, string message, string[] linkURLs, string[] linkLabels)
        {
            String endpointParams = "";
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            requestHeaders.Add(new KeyValuePair<string, string>(APIv1HTTPRequest.HTTP_HEADER_CONTENT_TYPE, APIv1HTTPRequest.HTTP_HEADER_CONTENT_TYPE_FORM_URL_ENCODED));
            APIv1EndpointResponseESD<ESDocument> endpointResponse = new APIv1EndpointResponseESD<ESDocument>();
        
            try{
                String linkURLParams = "";
                String linkLabelParams = "";
            
                //generate parameters for link URLs to be placed in the message
                for(int i=0; i < linkURLs.Length && i < MAX_MESSAGE_PLACEHOLDERS; i++){
                    if(!String.IsNullOrWhiteSpace(linkURLs[i].Trim())){
                        linkURLParams += "&link"+(i+1)+"_url="+ HttpUtility.UrlEncode(linkURLs[i]);
                    }
                }
            
                //generate parameters for link labels to be placed in the message
                for(int i=0; i < linkLabels.Length && i < MAX_MESSAGE_PLACEHOLDERS; i++){
                    if (!String.IsNullOrWhiteSpace(linkLabels[i].Trim()))
                    {
                        linkLabelParams += "&link"+(i+1)+"_label="+ HttpUtility.UrlEncode(linkLabels[i]);
                    }
                }
            
                //set notification parameters
                String requestPostBody = "notify_category="+ HttpUtility.UrlEncode(notifyCategory)+"&message="+ HttpUtility.UrlEncode(message) + linkURLParams + linkLabelParams;

                //make a HTTP request to the platform's API endpoint to create the organisation notifications
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_POST, APIv1Constants.API_ORG_ENDPOINT_CREATE_NOTIFCATION+APIv1Constants.API_PATH_SLASH+apiOrgSession.getSessionID(), endpointParams, requestHeaders, requestPostBody, null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

                //get result status and result code from document
                if (endpointResponse.esDocument != null)
                {
                    //get the result status from the esDocument
                    if (endpointResponse.esDocument.resultStatus == ESDocumentConstants.RESULT_SUCCESS)
                    {
                        endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS;
                    }

                    //get the result code from the ESdocument's configs if possible
                    if (endpointResponse.esDocument.configs != null)
                    {
                        string resultCode = "";
                        if (!endpointResponse.esDocument.configs.TryGetValue(APIv1Constants.API_ORG_ENDPOINT_ATTRIBUTE_RESULT_CODE, out resultCode))
                        {
                            resultCode = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_UNKNOWN;
                        }
                        endpointResponse.result_code = resultCode;
                    }
                }

                //check that the notification were successfully sent
                if (endpointResponse.result.ToUpper() != APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                {
                    //check if the session still exists
                    if(endpointResponse.result_code.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_SESSION_INVALID){
                        //mark that the session has expired
                        apiOrgSession.markSessionExpired();
                    }
                }
            }
            catch(Exception ex)
            {
                endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_UNKNOWN;
			    endpointResponse.result_message = apiOrgSession.getLangBundle().GetString(endpointResponse.result_code, apiOrgSession.languageLocale) + "\n" + ex.Message;
            }
        
            return endpointResponse;
        }
    }
}