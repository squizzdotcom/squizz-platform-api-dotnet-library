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
namespace Squizz.Platform.API.v1
{
    /// <summary>Stores constant variables required for accessing the platform's API</summary>
    public class APIv1Constants
    {
        /// <summary>HTTP protocol to use when calling the platform's API</summary>
        public const string API_PROTOCOL = "https://";

        /// <summary>internet domain to use when calling the platform's API</summary>
        public const string API_DOMAIN = "api.squizz.com";

        /// <summary>URL path within the platform's API to call organisation endpoints</summary>
        public const string API_ORG_PATH = "/rest/1/org/";

        /// <summary>slash character to set within URL path to API endpoint requests</summary>
        public const string API_PATH_SLASH = "/";

        /// <summary>name of the platform's API endpoint to call to create a session in the API for an organisation</summary>
        public const string API_ORG_ENDPOINT_CREATE_SESSION = "create_session";

        /// <summary>name of the platform's API endpoint to call to destroy a session in the API for an organisation</summary>
        public const string API_ORG_ENDPOINT_DESTROY_SESSION = "destroy_session";

        /// <summary>name of the platform's API endpoint to call to validate a session in the API for an organisation</summary>
        public const string API_ORG_ENDPOINT_VALIDATE_SESSION = "validate_session";

        /// <summary>name of the platform's API endpoint to call to create an organisation notification</summary>
        public const string API_ORG_ENDPOINT_CREATE_NOTIFCATION = "create_notification";

        /// <summary>name of the platform's API endpoint to call to create an organisation notification</summary>
        public const string API_ORG_ENDPOINT_VALIDATE_CERT = "validate_cert";

        /// <summary>name of the platform's API endpoint to call to import organisation data stored within an Ecommerce Standards Documents</summary>
        public const string API_ORG_ENDPOINT_IMPORT_ESD = "import_esd";

        /// <summary>name of the platform's API endpoint to call to send a purchase order to a supplier organisation for procurement</summary>
        public const string API_ORG_ENDPOINT_PROCURE_PURCHASE_ORDER_FROM_SUPPLIER = "procure_purchase_order_from_supplier";

        /// <summary>name of the platform's API endpoint to call to get organisation data returned in an Ecommerce Standards Document from a connected organisation</summary>
        public const string API_ORG_ENDPOINT_RETRIEVE_ESD = "retrieve_esd";

        /// <summary>name of the endpoint attribute in the API endpoint response that contains the result code</summary>
        public const string API_ORG_ENDPOINT_ATTRIBUTE_RESULT_CODE = "result_code";

        /// <summary>HTTP request method used to post data</summary>
        public const string HTTP_REQUEST_METHOD_POST = "POST";

        /// <summary>HTTP request method to get data</summary>
        public const string HTTP_REQUEST_METHOD_GET = "GET";

        /// <summary>HTTP request content type header name</summary>
        public const string HTTP_REQUEST_CONTENT_TYPE = "Content-Type";

        /// <summary>HTTP request content type header for specifying that the request body consists of JSON data</summary>
        public const string HTTP_REQUEST_CONTENT_TYPE_JSON = "application/json";

        /// <summary>English australian locale that the API supports returning messages in</summary>
        public const string SUPPORTED_LOCALES_EN_AU = "en-AU";

        /// <summary>Name of the package that contains the language bundles used for storing locale languages</summary>
        public const string LANG_BUNDLE_NAME = "Strings";
    }
}