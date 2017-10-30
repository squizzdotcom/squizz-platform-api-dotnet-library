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
    /// <summary>Represents the response returned from an endpoint in the platform's API, containing a type of Ecommerce Standards Document</summary>
    [DataContract]
    public class APIv1EndpointResponseESD<T>
    {
        public const string ESD_CONFIG_ORDERS_WITH_UNMAPPED_LINES = "orders_with_unmapped_lines";
        public const string ESD_CONFIG_ORDERS_WITH_UNPRICED_LINES = "orders_with_unpriced_lines";
        public const string ESD_CONFIG_ORDERS_WITH_UNSTOCKED_LINES = "orders_with_unstocked_lines";

        //set default values for the response
        [DataMember]
        public string result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;

        [DataMember]
        public string result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_UNKNOWN;

        [DataMember]
        public string result_message = "";

        [DataMember]
        public string api_version = "1.0.0.0";

        [DataMember]
        public string session_id = "";

        [DataMember]
        public string session_valid = "";

        [DataMember]
        public T esDocument;
    }
}
