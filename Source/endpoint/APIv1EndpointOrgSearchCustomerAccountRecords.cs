/**
* Copyright (C) 2018 Squizz PTY LTD
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
    ///     Class handles calling the SQUIZZ.com API endpoint to search for and retrieve records (such as invoices, sales orders, back orders, payments, credits, transactions) associated to a supplier organisation's customer account. See the full list at https://www.squizz.com/docs/squizz/Platform-API.html#section1035
    ///     The data being retrieved is wrapped up in a Ecommerce Standards Document (ESD) that contains records storing data of a particular type
    /// </summary>
    public class APIv1EndpointOrgSearchCustomerAccountRecords
    {
        /// <summary>Calls the platform's API endpoint and searches for a connected organisation's customer account records retrieved live from their connected business system</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="recordType">type of record data to search for.</param>
        /// <param name="supplierOrgID"> unique ID of the organisation in the SQUIZZ.com platform that has supplies the customer account</param>
        /// <param name="customerAccountCode"> code of the account organisation's customer account. Customer account only needs to be set if the supplier organisation has assigned multiple accounts to the organisation logged into the API session (customer org) and account specific data is being obtained</param>
        /// <param name="beginDateTime">earliest date time to search for records for. Date time set as milliseconds since 1/1/1970 12am UTC epoch</param>
        /// <param name="endDateTime">latest date time to search for records up to.Date time set as milliseconds since 1/1/1970 12am UTC epoch</param>
        /// <param name="pageNumber">page number to obtain records from</param>
        /// <param name="recordsMaxAmount">maximum number of records to return</param>
        /// <param name="outstandingRecords">if true then only search for records that are marked as outstanding (such as unpaid invoices)</param>
        /// <param name="searchString">search text to match records on</param>
        /// <param name="keyRecordIDs">comma delimited list of records unique key record ID to match on.Each Key Record ID value needs to be URI encoded</param>
        /// <param name="searchType">specifies the field to search for records on, matching the record's field with the search string given</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing customer account enquiry records</returns>
        public static APIv1EndpointResponseESD<ESDocumentCustomerAccountEnquiry> call(
            APIv1OrgSession apiOrgSession,
            int endpointTimeoutMilliseconds,
            string recordType,
            string supplierOrgID,
            string customerAccountCode,
            long beginDateTime,
            long endDateTime,
            int pageNumber,
            int recordsMaxAmount,
            bool outstandingRecords,
            string searchString,
            string keyRecordIDs,
            string searchType)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            APIv1EndpointResponseESD<ESDocumentCustomerAccountEnquiry> endpointResponse = new APIv1EndpointResponseESD<ESDocumentCustomerAccountEnquiry>();

            try
            {
                //set endpoint parameters
                String endpointParams =
                    "record_type=" + recordType +
                    "&supplier_org_id=" + HttpUtility.UrlEncode(supplierOrgID) +
                    "&customer_account_code=" + HttpUtility.UrlEncode(customerAccountCode) +
                    "&begin_date_time=" + beginDateTime +
                    "&end_date_time=" + endDateTime +
                    "&page_number=" + pageNumber +
                    "&records_max_amount=" + recordsMaxAmount +
                    "&outstanding_records=" + (outstandingRecords ? "Y" : "N") +
                    "&search_string=" + HttpUtility.UrlEncode(searchString) +
                    "&key_record_ids=" + HttpUtility.UrlEncode(keyRecordIDs) +
                    "&search_type=" + HttpUtility.UrlEncode(searchType);

                //make a HTTP request to the platform's API endpoint to search for the customer account enquiry records
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_GET, APIv1Constants.API_ORG_ENDPOINT_SEARCH_CUSTOMER_ACCOUNT_RECORDS_ESD + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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

                //check that the data was successfully retrieved
                if (endpointResponse.result.ToUpper() != APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                {
                    //check if the session still exists
                    if (endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_SESSION_INVALID)
                    {
                        //mark that the session has expired
                        apiOrgSession.markSessionExpired();
                    }
                }
            }
            catch (Exception ex)
            {
                endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_UNKNOWN;
                endpointResponse.result_message = apiOrgSession.getLangBundle().GetString(endpointResponse.result_code, apiOrgSession.languageLocale) + "\n" + ex.Message;
            }

            return endpointResponse;
        }
    }
}