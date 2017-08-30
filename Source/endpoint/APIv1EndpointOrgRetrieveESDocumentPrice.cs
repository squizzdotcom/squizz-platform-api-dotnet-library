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
    ///     Class handles calling the SQUIZZ.com API endpoint to get a supplier organisation's pricing data from a connected organisation in the platform. More details about the endpoint can found at https://www.squizz.com/docs/squizz/Platform-API.html#section843
    ///     The data being retrieved is wrapped up in a Ecommerce Standards Document (ESD) that contains pricing records
    /// </summary>
    public class APIv1EndpointOrgRetrieveESDocumentPrice
    {
        public const int RETRIEVE_TYPE_ID_PRICING = 37;
        public const int MAX_RECORDS_PER_REQUEST = 5000;
        
        /// <summary>Calls the platform's API endpoint and gets an organisation's pricing data in a Ecommerce Standards Document</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="customerAccountCode">code of the supplier organisation's customer account. Customer account only needs to be set if the supplier organisation has assigned multiple accounts to the organisation logged into the API session (customer org) and account specific data is being obtained</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing pricing records</returns>
        public static APIv1EndpointResponseESD<ESDocumentPrice> call(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string supplierOrgID, string customerAccountCode, int recordsStartIndex, int recordsMaxAmount)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string,string>>();
            APIv1EndpointResponseESD<ESDocumentPrice> endpointResponse = new APIv1EndpointResponseESD<ESDocumentPrice>();
        
            try{
                //set endpoint parameters
                string endpointParams = "data_type_id="+ RETRIEVE_TYPE_ID_PRICING + "&supplier_org_id=" + HttpUtility.UrlEncode(supplierOrgID) + "&customer_account_code="+ HttpUtility.UrlEncode(customerAccountCode) + "&records_start_index=" + recordsStartIndex + "&records_max_amount=" + recordsMaxAmount;

                //make a HTTP request to the platform's API endpoint to retrieve the specified organisation data contained in the Ecommerce Standards Document
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_GET, APIv1Constants.API_ORG_ENDPOINT_RETRIEVE_ESD+APIv1Constants.API_PATH_SLASH+apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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

                //check that the data was successfully pushed up
                if (endpointResponse.result.ToUpper() != APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                {
                    //check if the session still exists
                    if(endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_SESSION_INVALID){
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