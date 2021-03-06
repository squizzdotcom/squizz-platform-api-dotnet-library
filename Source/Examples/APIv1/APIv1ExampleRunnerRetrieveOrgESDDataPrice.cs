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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then retrieves Ecommerce data from a conencted organisation in the platform</summary>
    public class APIv1ExampleRunnerRetrieveOrgESDDataPrice
    {
        public static void runAPIv1ExampleRunnerRetrieveOrgESDDataPrice()
        {
            Console.WriteLine("Example - Retrieve Supplier Organisation Pricing Data");
            Console.WriteLine("");

            //obtain or load in an organisation's API credentials, in this example from the user in the console
            Console.WriteLine("Enter Organisation ID:");
            string orgID = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Key:");
            string orgAPIKey = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Password:");
            string orgAPIPass = Console.ReadLine();
            Console.WriteLine("Enter Supplier Organisation ID:");
            string supplierOrgID = Console.ReadLine();
            Console.WriteLine("(Optioanally) Enter Supplier's Customer Account Code:");
            string customerAccountCode = Console.ReadLine();

            //create an API session instance
            int sessionTimeoutMilliseconds = 20000;
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

            //attempt retrieving supplying product pricing data if the API was successfully created
            if (apiOrgSession.doesSessionExist())
            {
                //after 60 seconds give up on waiting for a response from the API when creating the notification
                int timeoutMilliseconds = 60000;

                //loop through retrieving pages of records from the API
                bool hasMoreRecordsToRetrieve = true;
                int recordStartIndex = 0;
                while(hasMoreRecordsToRetrieve)
                {
                    //call the platform's API to get the supplier organisation's pricing data
                    APIv1EndpointResponseESD<ESDocumentPrice> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrievePrices(apiOrgSession, timeoutMilliseconds, supplierOrgID, customerAccountCode, APIv1EndpointOrgRetrieveESDocument.MAX_RECORDS_PER_REQUEST, recordStartIndex, "");
                    ESDocumentPrice esDocumentPrice = (ESDocumentPrice)endpointResponseESD.esDocument;

                    //check that the data successfully retrieved
                    if (endpointResponseESD.result.ToUpper()==APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                    {
                        Console.WriteLine("SUCCESS - organisation data successfully obtained from the platform");
                        Console.WriteLine("Pricing Records Returned: " + esDocumentPrice.totalDataRecords);

                        //check that records have been placed into the standards document
                        if (esDocumentPrice.dataRecords != null) {
                            Console.WriteLine("Product Records:");

                            //iterate through each price record stored within the standards document
                            int i = 0;
                            foreach(ESDRecordPrice priceRecord in esDocumentPrice.dataRecords)
                            {
                                //output details of the product record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("  Product Record #: " + i);
                                Console.WriteLine("  Key Product ID: " + priceRecord.keyProductID);
                                Console.WriteLine("Key Sell Unit ID: " + priceRecord.keySellUnitID);
                                Console.WriteLine("        Quantity: " + priceRecord.quantity);
                                Console.WriteLine("           Price: " + priceRecord.price);
                                if(priceRecord.taxRate != 0){
                                    Console.WriteLine("        Tax Rate: " + priceRecord.taxRate);
                                }
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);

                                i++;
                            }
                        }

                        //check to see if a full page of records were retrieved and if there is more records to get
                        if (esDocumentPrice.totalDataRecords >= APIv1EndpointOrgRetrieveESDocument.MAX_RECORDS_PER_REQUEST) {
                            recordStartIndex += APIv1EndpointOrgRetrieveESDocument.MAX_RECORDS_PER_REQUEST;
                        }else{
                            hasMoreRecordsToRetrieve = false;
                        }
                    } else {
                        Console.WriteLine("FAIL - organisation data failed to be obtained from the platform. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);
                        break;
                    }
                }

                //next steps
                //call other API endpoints...
                //destroy API session when done...
                apiOrgSession.destroyOrgSession();
            }
        }
    }
}
