/**
* Copyright (C) 2019 Squizz PTY LTD
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
    ///     Class handles calling the SQUIZZ.com API endpoint to get a supplier organisation's product data from a connected organisation. See more details about the endpoint at https://www.squizz.com/docs/squizz/Platform-API.html#section843
    ///     The data being retrieved is wrapped up in a Ecommerce Standards Document (ESD) that contains product records
    /// </summary>
    public class APIv1EndpointOrgRetrieveESDocument
    {
        public const int RETRIEVE_TYPE_ID_PRODUCTS = 3;
        public const int RETRIEVE_TYPE_ID_PRICING = 37;
        public const int RETRIEVE_TYPE_ID_PRODUCT_STOCK = 10;
        public const int RETRIEVE_TYPE_ID_PRODUCT_IMAGES = 12;
        public const int RETRIEVE_TYPE_ID_PRODUCT_COMBINATIONS = 15;
        public const int RETRIEVE_TYPE_ID_CATEGORIES = 8;
        public const int RETRIEVE_TYPE_ID_ATTRIBUTES = 11;
        public const int RETRIEVE_TYPE_ID_CUSTOMER_CONTRACTS = 19;
        public const int RETRIEVE_TYPE_ID_MAKERS = 44;
        public const int RETRIEVE_TYPE_ID_MAKER_MODELS = 45;
        public const int RETRIEVE_TYPE_ID_MAKER_MODEL_MAPPINGS = 46;

        // defines the types of pricing records than can be retrieved
        public const string RETRIEVE_PRICE_TYPE_CUSTOMER_ACCOUNT_PRICING = "customer_account_pricing";
        public const string RETRIEVE_PRICE_TYPE_PRICE_LEVEL_UNIT_PRICING = "price_level_unit_pricing";
        public const string RETRIEVE_PRICE_TYPE_PRICE_LEVEL_QUANTITY_PRICING = "price_level_quantity_pricing";
        public const string RETRIEVE_PRICE_TYPE_PRICE_GROUPS = "price_groups";

        public const int MAX_RECORDS_PER_REQUEST = 5000;

        /// <summary>Calls the platform's API endpoint and gets an organisation's product data in a Ecommerce Standards Document</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="recordsMaxAmount">maximum number of records to retrieve in one request to the platform's endpoint. If the number is higher than the allowed amount this the records returned will be less than this number</param>
        /// <param name="recordsStartIndex">index number of the record to obtain records from. Set 0 for first record</param>
        /// <param name="extraParameters">any additional URL parameters to append to the requesting URL. Ensure parameter values are URL escaped.</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing product records</returns>
        public static APIv1EndpointResponseESD<ESDocumentProduct> callRetrieveProducts(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string supplierOrgID, int recordsMaxAmount, int recordsStartIndex, string extraParameters)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string,string>>();
            APIv1EndpointResponseESD<ESDocumentProduct> endpointResponse = new APIv1EndpointResponseESD<ESDocumentProduct>();
        
            try{
                //set endpoint parameters
                string endpointParams = buildRequestParametersForURL(RETRIEVE_TYPE_ID_PRODUCTS, supplierOrgID, "", recordsStartIndex, recordsMaxAmount, extraParameters);

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

        /// <summary>Calls the platform's API endpoint and gets an organisation's product data in a Ecommerce Standards Document</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="customerAccountCode">code of the supplier organisation's customer account. Customer account only needs to be set if the supplier organisation has assigned multiple accounts to the organisation logged into the API session (customer org) and account specific data is being obtained</param>
        /// <param name="recordsMaxAmount">maximum number of records to retrieve in one request to the platform's endpoint. If the number is higher than the allowed amount this the records returned will be less than this number</param>
        /// <param name="recordsStartIndex">index number of the record to obtain records from. Set 0 for first record</param>
        /// <param name="extraParameters">any additional URL parameters to append to the requesting URL. Ensure parameter values are URL escaped.</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing product records</returns>
        public static APIv1EndpointResponseESD<ESDocumentStockQuantity> callRetrieveStockQuantities(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string supplierOrgID, int recordsMaxAmount, int recordsStartIndex, string extraParameters)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            APIv1EndpointResponseESD<ESDocumentStockQuantity> endpointResponse = new APIv1EndpointResponseESD<ESDocumentStockQuantity>();

            try
            {
                //set endpoint parameters
                string endpointParams = buildRequestParametersForURL(RETRIEVE_TYPE_ID_PRODUCT_STOCK, supplierOrgID, "", recordsStartIndex, recordsMaxAmount, extraParameters);

                //make a HTTP request to the platform's API endpoint to retrieve the specified organisation data contained in the Ecommerce Standards Document
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_GET, APIv1Constants.API_ORG_ENDPOINT_RETRIEVE_ESD + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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


        /// <summary>Calls the platform's API endpoint and gets an organisation's product combination data in a Ecommerce Standards Document</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="recordsMaxAmount">maximum number of records to retrieve in one request to the platform's endpoint. If the number is higher than the allowed amount this the records returned will be less than this number</param>
        /// <param name="recordsStartIndex">index number of the record to obtain records from. Set 0 for first record</param>
        /// <param name="extraParameters">any additional URL parameters to append to the requesting URL. Ensure parameter values are URL escaped.</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing product combination records</returns>
        public static APIv1EndpointResponseESD<ESDocumentProductCombination> callRetrieveProductCombinations(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string supplierOrgID, int recordsMaxAmount, int recordsStartIndex, string extraParameters)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            APIv1EndpointResponseESD<ESDocumentProductCombination> endpointResponse = new APIv1EndpointResponseESD<ESDocumentProductCombination>();

            try
            {
                //set endpoint parameters
                string endpointParams = buildRequestParametersForURL(RETRIEVE_TYPE_ID_PRODUCT_COMBINATIONS, supplierOrgID, "", recordsStartIndex, recordsMaxAmount, extraParameters);

                //make a HTTP request to the platform's API endpoint to retrieve the specified organisation data contained in the Ecommerce Standards Document
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_GET, APIv1Constants.API_ORG_ENDPOINT_RETRIEVE_ESD + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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

        /// <summary>Calls the platform's API endpoint and gets an organisation's pricing data in a Ecommerce Standards Document</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="customerAccountCode">code of the supplier organisation's customer account. Customer account only needs to be set if the supplier organisation has assigned multiple accounts to the organisation logged into the API session (customer org) and account specific data is being obtained</param>
        /// <param name="recordsMaxAmount">maximum number of records to retrieve in one request to the platform's endpoint. If the number is higher than the allowed amount this the records returned will be less than this number</param>
        /// <param name="recordsStartIndex">index number of the record to obtain records from. Set 0 for first record</param>
        /// <param name="extraParameters">any additional URL parameters to append to the requesting URL. Ensure parameter values are URL escaped.</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing pricing records</returns>
        public static APIv1EndpointResponseESD<ESDocumentPrice> callRetrievePrices(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string supplierOrgID, string customerAccountCode, int recordsMaxAmount, int recordsStartIndex, string extraParameters)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            APIv1EndpointResponseESD<ESDocumentPrice> endpointResponse = new APIv1EndpointResponseESD<ESDocumentPrice>();

            try
            {
                //set endpoint parameters
                string endpointParams = buildRequestParametersForURL(RETRIEVE_TYPE_ID_PRICING, supplierOrgID, customerAccountCode, recordsStartIndex, recordsMaxAmount, extraParameters);

                //make a HTTP request to the platform's API endpoint to retrieve the specified organisation data contained in the Ecommerce Standards Document
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_GET, APIv1Constants.API_ORG_ENDPOINT_RETRIEVE_ESD + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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

        /// <summary>Calls the platform's API endpoint and gets an organisation's attribute data in a Ecommerce Standards Document</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="customerAccountCode">code of the supplier organisation's customer account. Customer account only needs to be set if the supplier organisation has assigned multiple accounts to the organisation logged into the API session (customer org) and account specific data is being obtained</param>
        /// <param name="recordsMaxAmount">maximum number of records to retrieve in one request to the platform's endpoint. If the number is higher than the allowed amount this the records returned will be less than this number</param>
        /// <param name="recordsStartIndex">index number of the record to obtain records from. Set 0 for first record</param>
        /// <param name="extraParameters">any additional URL parameters to append to the requesting URL. Ensure parameter values are URL escaped.</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing attribute records</returns>
        public static APIv1EndpointResponseESD<ESDocumentAttribute> callRetrieveAttributes(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string supplierOrgID, string customerAccountCode, int recordsMaxAmount, int recordsStartIndex, string extraParameters)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            APIv1EndpointResponseESD<ESDocumentAttribute> endpointResponse = new APIv1EndpointResponseESD<ESDocumentAttribute>();

            try
            {
                //set endpoint parameters
                string endpointParams = buildRequestParametersForURL(RETRIEVE_TYPE_ID_ATTRIBUTES, supplierOrgID, customerAccountCode, recordsStartIndex, recordsMaxAmount, extraParameters);

                //make a HTTP request to the platform's API endpoint to retrieve the specified organisation data contained in the Ecommerce Standards Document
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_GET, APIv1Constants.API_ORG_ENDPOINT_RETRIEVE_ESD + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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

        /// <summary>Calls the platform's API endpoint and gets an organisation's customer contract data in a Ecommerce Standards Document</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="customerAccountCode">code of the supplier organisation's customer account. Customer account only needs to be set if the supplier organisation has assigned multiple accounts to the organisation logged into the API session (customer org) and account specific data is being obtained</param>
        /// <param name="recordsMaxAmount">maximum number of records to retrieve in one request to the platform's endpoint. If the number is higher than the allowed amount this the records returned will be less than this number</param>
        /// <param name="recordsStartIndex">index number of the record to obtain records from. Set 0 for first record</param>
        /// <param name="extraParameters">any additional URL parameters to append to the requesting URL. Ensure parameter values are URL escaped.</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing customer account contract records</returns>
        public static APIv1EndpointResponseESD<ESDocumentCustomerAccountContract> callRetrieveCustomerContracts(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string supplierOrgID, string customerAccountCode, int recordsMaxAmount, int recordsStartIndex, string extraParameters)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            APIv1EndpointResponseESD<ESDocumentCustomerAccountContract> endpointResponse = new APIv1EndpointResponseESD<ESDocumentCustomerAccountContract>();

            try
            {
                //set endpoint parameters
                string endpointParams = buildRequestParametersForURL(RETRIEVE_TYPE_ID_CUSTOMER_CONTRACTS, supplierOrgID, customerAccountCode, recordsStartIndex, recordsMaxAmount, extraParameters);

                //make a HTTP request to the platform's API endpoint to retrieve the specified organisation data contained in the Ecommerce Standards Document
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_GET, APIv1Constants.API_ORG_ENDPOINT_RETRIEVE_ESD + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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

        /// <summary>Calls the platform's API endpoint and gets an organisation's category data in a Ecommerce Standards Document</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="customerAccountCode">code of the supplier organisation's customer account. Customer account only needs to be set if the supplier organisation has assigned multiple accounts to the organisation logged into the API session (customer org) and account specific data is being obtained</param>
        /// <param name="recordsMaxAmount">maximum number of records to retrieve in one request to the platform's endpoint. If the number is higher than the allowed amount this the records returned will be less than this number</param>
        /// <param name="recordsStartIndex">index number of the record to obtain records from. Set 0 for first record</param>
        /// <param name="extraParameters">any additional URL parameters to append to the requesting URL. Ensure parameter values are URL escaped.</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing category records</returns>
        public static APIv1EndpointResponseESD<ESDocumentCategory> callRetrieveCategories(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string supplierOrgID, string customerAccountCode, int recordsMaxAmount, int recordsStartIndex, string extraParameters)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            APIv1EndpointResponseESD<ESDocumentCategory> endpointResponse = new APIv1EndpointResponseESD<ESDocumentCategory>();

            try
            {
                //set endpoint parameters
                string endpointParams = buildRequestParametersForURL(RETRIEVE_TYPE_ID_CATEGORIES, supplierOrgID, customerAccountCode, recordsStartIndex, recordsMaxAmount, extraParameters);

                //make a HTTP request to the platform's API endpoint to retrieve the specified organisation data contained in the Ecommerce Standards Document
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_GET, APIv1Constants.API_ORG_ENDPOINT_RETRIEVE_ESD + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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

        /// <summary>Calls the platform's API endpoint and gets an organisation's maker data in a Ecommerce Standards Document</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="customerAccountCode">code of the supplier organisation's customer account. Customer account only needs to be set if the supplier organisation has assigned multiple accounts to the organisation logged into the API session (customer org) and account specific data is being obtained</param>
        /// <param name="recordsMaxAmount">maximum number of records to retrieve in one request to the platform's endpoint. If the number is higher than the allowed amount this the records returned will be less than this number</param>
        /// <param name="recordsStartIndex">index number of the record to obtain records from. Set 0 for first record</param>
        /// <param name="extraParameters">any additional URL parameters to append to the requesting URL. Ensure parameter values are URL escaped.</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing category records</returns>
        public static APIv1EndpointResponseESD<ESDocumentMaker> callRetrieveMakers(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string supplierOrgID, string customerAccountCode, int recordsMaxAmount, int recordsStartIndex, string extraParameters)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            APIv1EndpointResponseESD<ESDocumentMaker> endpointResponse = new APIv1EndpointResponseESD<ESDocumentMaker>();

            try
            {
                //set endpoint parameters
                string endpointParams = buildRequestParametersForURL(RETRIEVE_TYPE_ID_MAKERS, supplierOrgID, customerAccountCode, recordsStartIndex, recordsMaxAmount, extraParameters);

                //make a HTTP request to the platform's API endpoint to retrieve the specified organisation data contained in the Ecommerce Standards Document
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_GET, APIv1Constants.API_ORG_ENDPOINT_RETRIEVE_ESD + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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

        /// <summary>Calls the platform's API endpoint and gets an organisation's maker model data in a Ecommerce Standards Document</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="customerAccountCode">code of the supplier organisation's customer account. Customer account only needs to be set if the supplier organisation has assigned multiple accounts to the organisation logged into the API session (customer org) and account specific data is being obtained</param>
        /// <param name="recordsMaxAmount">maximum number of records to retrieve in one request to the platform's endpoint. If the number is higher than the allowed amount this the records returned will be less than this number</param>
        /// <param name="recordsStartIndex">index number of the record to obtain records from. Set 0 for first record</param>
        /// <param name="extraParameters">any additional URL parameters to append to the requesting URL. Ensure parameter values are URL escaped.</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing category records</returns>
        public static APIv1EndpointResponseESD<ESDocumentMakerModel> callRetrieveMakerModels(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string supplierOrgID, string customerAccountCode, int recordsMaxAmount, int recordsStartIndex, string extraParameters)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            APIv1EndpointResponseESD<ESDocumentMakerModel> endpointResponse = new APIv1EndpointResponseESD<ESDocumentMakerModel>();

            try
            {
                //set endpoint parameters
                string endpointParams = buildRequestParametersForURL(RETRIEVE_TYPE_ID_MAKER_MODELS, supplierOrgID, customerAccountCode, recordsStartIndex, recordsMaxAmount, extraParameters);

                //make a HTTP request to the platform's API endpoint to retrieve the specified organisation data contained in the Ecommerce Standards Document
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_GET, APIv1Constants.API_ORG_ENDPOINT_RETRIEVE_ESD + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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

        /// <summary>Calls the platform's API endpoint and gets an organisation's maker model mapping data in a Ecommerce Standards Document</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="customerAccountCode">code of the supplier organisation's customer account. Customer account only needs to be set if the supplier organisation has assigned multiple accounts to the organisation logged into the API session (customer org) and account specific data is being obtained</param>
        /// <param name="recordsMaxAmount">maximum number of records to retrieve in one request to the platform's endpoint. If the number is higher than the allowed amount this the records returned will be less than this number</param>
        /// <param name="recordsStartIndex">index number of the record to obtain records from. Set 0 for first record</param>
        /// <param name="extraParameters">any additional URL parameters to append to the requesting URL. Ensure parameter values are URL escaped.</param>
        /// <returns>response from calling the API endpoint with the obtained Ecommerce Standards Document containing category records</returns>
        public static APIv1EndpointResponseESD<ESDocumentMakerModelMapping> callRetrieveMakerModelMappings(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, string supplierOrgID, string customerAccountCode, int recordsMaxAmount, int recordsStartIndex, string extraParameters)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            APIv1EndpointResponseESD<ESDocumentMakerModelMapping> endpointResponse = new APIv1EndpointResponseESD<ESDocumentMakerModelMapping>();

            try
            {
                //set endpoint parameters
                string endpointParams = buildRequestParametersForURL(RETRIEVE_TYPE_ID_MAKER_MODEL_MAPPINGS, supplierOrgID, customerAccountCode, recordsStartIndex, recordsMaxAmount, extraParameters);

                //make a HTTP request to the platform's API endpoint to retrieve the specified organisation data contained in the Ecommerce Standards Document
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_GET, APIv1Constants.API_ORG_ENDPOINT_RETRIEVE_ESD + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", null, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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

        /// <summary>builds a query string contain parameters formatted for requests to the API</summary>
        /// <param name="retrieveType">type of data being retrieved</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform to obtain data from</param>
        /// <param name="customerAccountCode">code of the customer account belonging to the organisation supplying the data</param>
        /// <param name="recordsStartIndex">index number of the record to obtain records from. Set 0 for first record</param>
        /// <param name="recordsMaxAmount">maximum number of records to retrieve in one request to the platform's endpoint. If the number is higher than the allowed amount this the records returned will be less than this number</param>
        /// <param name="extraParameters">any additional URL parameters to append to the requesting URL. Ensure parameter values are URL escaped.</param>
        /// <returns>built URL request query string</returns>
        private static string buildRequestParametersForURL(int retrieveType, string supplierOrgID, string customerAccountCode, int recordsStartIndex, int recordsMaxAmount, string extraParameters)
        {
            return "data_type_id=" + retrieveType + "&supplier_org_id=" + HttpUtility.UrlEncode(supplierOrgID) + "&customer_account_code=" + HttpUtility.UrlEncode(customerAccountCode) + "&records_start_index=" + recordsStartIndex + "&records_max_amount=" + recordsMaxAmount + (!String.IsNullOrWhiteSpace(extraParameters) ? "&" + extraParameters : "");
        }
    }
}