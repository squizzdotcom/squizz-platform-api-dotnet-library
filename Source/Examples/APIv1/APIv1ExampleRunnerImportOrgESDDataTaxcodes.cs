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
using Squizz.Platform.API.v1;
using Squizz.Platform.API.v1.endpoint;
using EcommerceStandardsDocuments;

namespace Squizz.Platform.API.Examples.APIv1
{
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then imports organisation data into the platform</summary>
    public class APIv1ExampleRunnerImportOrgESDDataTaxcodes
    {
        public static void runAPIv1ExampleRunnerImportOrgESDTaxcodeData()
        {
            Console.WriteLine("Example - Import Organisation Taxcode Data");
            Console.WriteLine("");

            //obtain or load in an organisation's API credentials, in this example from the user in the console
            Console.WriteLine("Enter Organisation ID:");
            string orgID = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Key:");
            string orgAPIKey = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Password:");
            string orgAPIPass = Console.ReadLine();
            int sessionTimeoutMilliseconds = 20000;

            //create an API session instance
            APIv1OrgSession apiOrgSession = new APIv1OrgSession(orgID, orgAPIKey, orgAPIPass, sessionTimeoutMilliseconds, APIv1Constants.SUPPORTED_LOCALES_EN_AU);

            //call the platform's API to request that a session is created
            APIv1EndpointResponse endpointResponse = apiOrgSession.createOrgSession();

            //check if the organisation's credentials were correct and that a session was created in the platform's API
            if (endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
            {
                //session has been created so now can call other API endpoints
                Console.WriteLine("SUCCESS - API session has successfully been created.");
            }
            else
            {
                //session failed to be created
                Console.WriteLine("FAIL - API session failed to be created. Reason: " + endpointResponse.result_message + " Error Code: " + endpointResponse.result_code);
            }

            //import organisation data if the API was successfully created
            if (apiOrgSession.doesSessionExist())
            {
                //create taxcode records
                List<ESDRecordTaxcode> taxcodeRecords = new List<ESDRecordTaxcode>();
                ESDRecordTaxcode taxcodeRecord = new ESDRecordTaxcode();
                taxcodeRecord.keyTaxcodeID = "1";
                taxcodeRecord.taxcode = "GST";
                taxcodeRecord.taxcodeLabel = "GST";
                taxcodeRecord.description = "Goods And Services Tax";
                taxcodeRecord.taxcodePercentageRate = 10;
                taxcodeRecords.Add(taxcodeRecord);

                taxcodeRecord = new ESDRecordTaxcode();
                taxcodeRecord.keyTaxcodeID = "2";
                taxcodeRecord.taxcode = "FREE";
                taxcodeRecord.taxcodeLabel = "Tax Free";
                taxcodeRecord.description = "Free from Any Taxes";
                taxcodeRecord.taxcodePercentageRate = 0;
                taxcodeRecords.Add(taxcodeRecord);

                taxcodeRecord = new ESDRecordTaxcode();
                taxcodeRecord.keyTaxcodeID = "3";
                taxcodeRecord.taxcode = "NZGST";
                taxcodeRecord.taxcodeLabel = "New Zealand GST Tax";
                taxcodeRecord.description = "New Zealand Goods and Services Tax";
                taxcodeRecord.taxcodePercentageRate = 15;
                taxcodeRecords.Add(taxcodeRecord);

                //create a hashmap containing configurations of the organisation taxcode data
                Dictionary<string, string> configs = new Dictionary<string, string>();

                //add a dataFields attribute that contains a comma delimited list of tacode record fields that the API is allowed to insert, update in the platform
                configs.Add("dataFields", "keyTaxcodeID,taxcode,taxcodeLabel,description,taxcodePercentageRate");

                //create a Ecommerce Standards Document that stores an array of taxcode records
                ESDocumentTaxcode taxcodeESD = new ESDocumentTaxcode(ESDocumentConstants.RESULT_SUCCESS, "successfully obtained data", taxcodeRecords.ToArray(), configs);

                //after 30 seconds give up on waiting for a response from the API when creating the notification
                int timeoutMilliseconds = 30000;

                //call the platform's API to import in the organisation's data
                APIv1EndpointResponseESD<ESDocument> endpointResponseESD = APIv1EndpointOrgImportESDocument.call(apiOrgSession, timeoutMilliseconds, APIv1EndpointOrgImportESDocument.IMPORT_TYPE_ID_TAXCODES, taxcodeESD);

                //check that the data successfully imported
                if (endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS) {
                    Console.WriteLine("SUCCESS - organisation data successfully imported into the platform");
                } else {
                    Console.WriteLine("FAIL - organisation data failed to be imported into the platform. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);
                }

                //next steps
                //call other API endpoints...
                //destroy API session when done...
                apiOrgSession.destroyOrgSession();
            }
        }
    }
}