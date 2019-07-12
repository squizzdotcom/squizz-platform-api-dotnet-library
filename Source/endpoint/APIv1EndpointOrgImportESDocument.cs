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
    ///     Class handles calling the SQUIZZ.com API endpoint to push and import different kinds of organisational data into the platform such as products, customer accounts, and many other data types. See the full list at https://www.squizz.com/docs/squizz/Platform-API.html#section843
    ///     The data being pushed must be wrapped up in a Ecommerce Standards Document (ESD) that contains records storing data of a particular type
    /// </summary>
    public class APIv1EndpointOrgImportESDocument
    {
        public const int IMPORT_TYPE_ID_TAXCODES = 1;
        public const int IMPORT_TYPE_ID_PRICE_LEVELS = 2;
        public const int IMPORT_TYPE_ID_PRODUCTS = 3;
        public const int IMPORT_TYPE_ID_PRODUCT_PRICE_LEVEL_UNIT_PRICING = 4;
        public const int IMPORT_TYPE_ID_PRODUCT_PRICE_LEVEL_QUANTITY_PRICING = 6;
        public const int IMPORT_TYPE_ID_PRODUCT_CUSTOMER_ACCOUNT_PRICING = 7;
		public const int IMPORT_TYPE_ID_CATEGORIES = 8;
        public const int IMPORT_TYPE_ID_ALTERNATE_CODES = 9;
        public const int IMPORT_TYPE_ID_PRODUCT_STOCK_QUANTITIES = 10;
		public const int IMPORT_TYPE_ID_ATTRIBUTES = 11;
        public const int IMPORT_TYPE_ID_SALES_REPRESENTATIVES = 16;
        public const int IMPORT_TYPE_ID_CUSTOMER_ACCOUNTS = 17;
        public const int IMPORT_TYPE_ID_SUPPLIER_ACCOUNTS = 18;
        public const int IMPORT_TYPE_ID_CUSTOMER_ACCOUNT_CONTRACTS = 19;
        public const int IMPORT_TYPE_ID_CUSTOMER_ACCOUNT_ADDRESSES = 20;
        public const int IMPORT_TYPE_ID_LOCATIONS = 23;
        public const int IMPORT_TYPE_ID_PURCHASERS = 25;
        public const int IMPORT_TYPE_ID_SURCHARGES = 26;
        public const int IMPORT_TYPE_ID_PAYMENT_TYPES = 27;
        public const int IMPORT_TYPE_ID_SELL_UNITS = 28;
        public const int IMPORT_TYPE_ID_MAKERS = 44;
        public const int IMPORT_TYPE_ID_MAKER_MODELS = 45;
        public const int IMPORT_TYPE_ID_MAKER_MODEL_MAPPINGS = 46;
        
        /// <summary>Calls the platform's API endpoint and pushes up and import organisation data in a Ecommerce Standards Document of a specified type</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="importTypeID">ID of the of the type of data to import</param>
        /// <param name="esDocument">Ecommerce Standards Document that contains records and data to to upload. Ensure the document matches the import type given</param>
        /// <returns>response from calling the API endpoint</returns>
        public static APIv1EndpointResponseESD<ESDocument> call(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, int importTypeID, ESDocument esDocument)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            requestHeaders.Add(new KeyValuePair<string, string>(APIv1HTTPRequest.HTTP_HEADER_CONTENT_ENCODING, APIv1HTTPRequest.HTTP_HEADER_CONTENT_ENCODING_GZIP));
            requestHeaders.Add(new KeyValuePair<string, string>(APIv1HTTPRequest.HTTP_HEADER_CONTENT_TYPE, APIv1HTTPRequest.HTTP_HEADER_CONTENT_TYPE_JSON));
            APIv1EndpointResponseESD<ESDocument> endpointResponse = new APIv1EndpointResponseESD<ESDocument>();
        
            try{
                //set endpoint parameters
                String endpointParams = "import_type_id="+importTypeID;
            
                //make a HTTP request to the platform's API endpoint to push the ESDocument data up
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest(APIv1Constants.HTTP_REQUEST_METHOD_POST, APIv1Constants.API_ORG_ENDPOINT_IMPORT_ESD+APIv1Constants.API_PATH_SLASH+apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", esDocument, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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
                    if(endpointResponse.result.ToUpper()==APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_SESSION_INVALID){
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