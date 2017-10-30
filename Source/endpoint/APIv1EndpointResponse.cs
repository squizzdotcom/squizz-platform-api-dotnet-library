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
using System.Runtime.Serialization;
namespace Squizz.Platform.API.v1.endpoint
{
    /// <summary>Represents the response returned from an endpoint in the platform's API</summary>
    [DataContract]
    public class APIv1EndpointResponse: ESDocument
    {
        public const string ENDPOINT_RESULT_SUCCESS = "SUCCESS";
        public const string ENDPOINT_RESULT_FAILURE = "FAILURE";
        public const string ENDPOINT_RESULT_CODE_SUCCESS = "SERVER_SUCCESS";
        public const string ENDPOINT_RESULT_CODE_ERROR_UNKNOWN = "SERVER_ERROR_UNKNOWN";
        public const string ENDPOINT_RESULT_CODE_ERROR_MALFORMED_URL = "SERVER_ERROR_MALFORMED_URL";
        public const string ENDPOINT_RESULT_CODE_ERROR_RESPONSE = "SERVER_ERROR_RESPONSE";
        public const string ENDPOINT_RESULT_CODE_ERROR_REQUEST_PROTOCOL = "SERVER_ERROR_REQUEST_PROTOCOL";
        public const string ENDPOINT_RESULT_CODE_ERROR_CONNECTION = "SERVER_ERROR_CONNECTION";
        public const string ENDPOINT_RESULT_CODE_ERROR_IO = "SERVER_ERROR_IO";
        public const string ENDPOINT_RESULT_CODE_ERROR_ORG_NOT_FOUND = "SERVER_ERROR_ORG_NOT_FOUND";
        public const string ENDPOINT_RESULT_CODE_ERROR_INCORRECT_API_CREDENTIALS = "SERVER_ERROR_INCORRECT_API_CREDENTIALS";
        public const string ENDPOINT_RESULT_CODE_ERROR_ORG_INACTIVE = "SERVER_ERROR_ORG_INACTIVE";
        public const string ENDPOINT_RESULT_CODE_ERROR_SESSION_INVALID = "SERVER_ERROR_SESSION_INVALID";
        public const string ENDPOINT_RESULT_CODE_ERROR_INVALID_NOTIFICATION_CATEGORY = "SERVER_ERROR_INVALID_NOTIFICATION_CATEGORY";
        public const string ENDPOINT_RESULT_CODE_ERROR_NO_ORG_PEOPLE_TO_NOTIFY = "SERVER_ERROR_NO_ORG_PEOPLE_TO_NOTIFY";
        public const string ENDPOINT_RESULT_CODE_ERROR_INSUFFICIENT_CREDIT = "SERVER_ERROR_INSUFFICIENT_CREDIT";
        public const string ENDPOINT_RESULT_CODE_ERROR_SECURITY_CERTIFICATE_NOT_FOUND = "SERVER_ERROR_SECURITY_CERTIFICATE_NOT_FOUND";
        public const string ENDPOINT_RESULT_CODE_ERROR_SENDER_DOES_NOT_MATCH_CERTIFICATE_COMMON_NAME = "SERVER_ERROR_SENDER_DOES_NOT_MATCH_CERTIFICATE_COMMON_NAME";
        public const string ENDPOINT_RESULT_CODE_ERROR_INVALID_API_ACTION = "SERVER_ERROR_INVALID_API_ACTION";
        public const string ENDPOINT_RESULT_CODE_ERROR_PERMISSION_DENIED = "SERVER_ERROR_PERMISSION_DENIED";
    
        //import esd server errors
        public const string ENDPOINT_RESULT_CODE_ERROR_DATA_IMPORT_MISSING_IMPORT_TYPE = "SERVER_ERROR_DATA_IMPORT_MISSING_IMPORT_TYPE";
        public const string ENDPOINT_RESULT_CODE_ERROR_DATA_IMPORT_MAX_IMPORTS_RUNNING = "SERVER_ERROR_DATA_IMPORT_MAX_IMPORTS_RUNNING";
        public const string ENDPOINT_RESULT_CODE_ERROR_DATA_IMPORT_BUSY = "SERVER_ERROR_DATA_IMPORT_BUSY";
        public const string ENDPOINT_RESULT_CODE_ERROR_DATA_IMPORT_NOT_FOUND = "SERVER_ERROR_DATA_IMPORT_NOT_FOUND"; 
        public const string ENDPOINT_RESULT_CODE_ERROR_DATA_JSON_WRONG_CONTENT_TYPE = "SERVER_ERROR_DATA_JSON_WRONG_CONTENT_TYPE";
        public const string ENDPOINT_RESULT_CODE_ERROR_DATA_JSON_MALFORMED = "SERVER_ERROR_DATA_JSON_MALFORMED";
        public const string ENDPOINT_RESULT_CODE_ERROR_ESD_DOCUMENT_HEADER_MALFORMED = "SERVER_ERROR_ESD_DOCUMENT_HEADER_MALFORMED";
        public const string ENDPOINT_RESULT_CODE_ERROR_ESD_DOCUMENT_HEADER_MISSING_ATTRIBUTES = "SERVER_ERROR_ESD_DOCUMENT_HEADER_MISSING_ATTRIBUTES";
        public const string ENDPOINT_RESULT_CODE_ERROR_DATA_IMPORT_ABORTED = "SERVER_ERROR_DATA_IMPORT_ABORTED";
        public const string ENDPOINT_RESULT_CODE_ERROR_ESD_DOCUMENT_UNSUCCESSFUL = "SERVER_ERROR_ESD_DOCUMENT_UNSUCCESSFUL";
        public const string ENDPOINT_RESULT_CODE_ERROR_ESD_DOCUMENT_NO_RECORD = "SERVER_ERROR_ESD_DOCUMENT_NO_RECORD";
    
        public const string ENDPOINT_RESULT_CODE_ERROR_ORG_DOES_NOT_EXIST = "SERVER_ERROR_ORG_DOES_NOT_EXIST";
        public const string ENDPOINT_RESULT_CODE_ERROR_ORG_NOT_SELLING = "SERVER_ERROR_ORG_NOT_SELLING";
        public const string ENDPOINT_RESULT_CODE_ERROR_ORG_NOT_ENOUGH_CREDITS = "SERVER_ERROR_ORG_NOT_ENOUGH_CREDITS";
        public const string ENDPOINT_RESULT_CODE_ERROR_NO_ORG_CUSTOMER_ACCOUNT_SET = "SERVER_ERROR_NO_ORG_CUSTOMER_ACCOUNT_SET";
        public const string ENDPOINT_RESULT_CODE_ERROR_NO_ORG_CUSTOMER_ACCOUNT_ASSIGNED = "SERVER_ERROR_NO_ORG_CUSTOMER_ACCOUNT_ASSIGNED";
        public const string ENDPOINT_RESULT_CODE_ERROR_CUSTOMER_ACCOUNT_NO_ACCOUNT_PAYMENT_TYPE = "SERVER_ERROR_CUSTOMER_ACCOUNT_NO_ACCOUNT_PAYMENT_TYPE";
        public const string ENDPOINT_RESULT_CODE_ERROR_ORDER_PRODUCT_NOT_MAPPED = "SERVER_ERROR_ORDER_PRODUCT_NOT_MAPPED";
        public const string ENDPOINT_RESULT_CODE_ERROR_ORDER_MAPPED_PRODUCT_STOCK_NOT_AVAILABLE = "SERVER_ERROR_ORDER_MAPPED_PRODUCT_STOCK_NOT_AVAILABLE";
        public const string ENDPOINT_RESULT_CODE_ERROR_ORDER_MAPPED_PRODUCT_PRICE_NOT_FOUND = "SERVER_ERROR_ORDER_MAPPED_PRODUCT_PRICE_NOT_FOUND";
        public const string ENDPOINT_RESULT_CODE_ERROR_CUSTOMER_ACCOUNT_ON_HOLD = "SERVER_ERROR_CUSTOMER_ACCOUNT_ON_HOLD";
        public const string ENDPOINT_RESULT_CODE_ERROR_CUSTOMER_ACCOUNT_OUTSIDE_BALANCE_LIMIT = "SERVER_ERROR_CUSTOMER_ACCOUNT_OUTSIDE_BALANCE_LIMIT";
        public const string ENDPOINT_RESULT_CODE_ERROR_INCORRECT_DATA_TYPE = "SERVER_ERROR_INCORRECT_DATA_TYPE";

        //set default values for the response
        [DataMember]
        public string result = ENDPOINT_RESULT_FAILURE;

        [DataMember]
        public string result_code = ENDPOINT_RESULT_CODE_ERROR_UNKNOWN;

        [DataMember]
        public string result_message = "";

        [DataMember]
        public string api_version = "1.0.0.0";

        [DataMember]
        public string session_id = "";

        [DataMember]
        public string session_valid = "";
    }
}