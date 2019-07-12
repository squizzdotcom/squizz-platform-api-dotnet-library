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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then retrieves Maker data from a conencted organisation in the platform</summary>
    public class APIv1ExampleRunnerRetrieveOrgESDDataMakers
	{
        public static void runAPIv1ExampleRunnerRetrieveOrgESDDataMakers()
        {
            Console.WriteLine("Example - Retrieve Supplier Organisation Maker Data");
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

            int sessionTimeoutMilliseconds = 20000;
			int recordsMaxAmount = 5000;
			int recordsStartIndex = 0;
			bool getMoreRecords = true;
			int recordNumber = 0;
			int pageNumber = 0;
			
			//create an API session instance
			APIv1OrgSession apiOrgSession = new APIv1OrgSession(orgID, orgAPIKey, orgAPIPass, sessionTimeoutMilliseconds, APIv1Constants.SUPPORTED_LOCALES_EN_AU);
			
			//call the platform's API to request that a session is created
			APIv1EndpointResponse endpointResponse = apiOrgSession.createOrgSession();
			
			//check if the organisation's credentials were correct and that a session was created in the platform's API
			if(endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
			{
				//session has been created so now can call other API endpoints
				Console.WriteLine("SUCCESS - API session has successfully been created.");
			}
			else
			{
				//session failed to be created
				Console.WriteLine("FAIL - API session failed to be created. Reason: " + endpointResponse.result_message  + " Error Code: " + endpointResponse.result_code);
			}
			
			//retrieve organisation maker data if the API was successfully created
			if(apiOrgSession.doesSessionExist())
			{   
				//after 60 seconds give up on waiting for a response from the API when creating the notification
				int timeoutMilliseconds = 60000;
				
				//get the next page of records if needed
				while(getMoreRecords)
				{
					getMoreRecords = false;
					
					//call the platform's API to retrieve the organisation's maker data
					APIv1EndpointResponseESD<ESDocumentMaker> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrieveMakers(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", recordsMaxAmount, recordsStartIndex, "");

					//check that the data successfully retrieved
					if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS){
						Console.WriteLine("SUCCESS - organisation data successfully obtained from the platform for page number: "+pageNumber);
						pageNumber++;

						//process and output maker records
						ESDocumentMaker esDocumentMaker = (ESDocumentMaker)endpointResponseESD.esDocument;
						Console.WriteLine("Maker Records Returned: " + esDocumentMaker.totalDataRecords);

						//check that records have been placed into the standards document
						if(esDocumentMaker.dataRecords != null){
							Console.WriteLine("Maker Records:");

							//iterate through each maker record stored within the standards document
							
							foreach(ESDRecordMaker makerRecord in esDocumentMaker.dataRecords)
							{    
								recordNumber++;
								
								//output details of the maker record
								Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
								Console.WriteLine("       Maker Record #: " + recordNumber);
								Console.WriteLine("         Key Maker ID: " + makerRecord.keyMakerID);
								Console.WriteLine("           Maker Code: " + makerRecord.makerCode);
								Console.WriteLine("           Maker Name: " + makerRecord.name);
								Console.WriteLine("    Maker Search Code: " + makerRecord.makerSearchCode);
								Console.WriteLine("             Ordering: " + makerRecord.ordering);
								Console.WriteLine("          Group Class: " + makerRecord.groupClass);
								Console.WriteLine("            Org. Name: " + makerRecord.orgName);
								Console.WriteLine("      Authority Label: " + (makerRecord.authorityNumberLabels != null && makerRecord.authorityNumberLabels.Length>0? makerRecord.authorityNumberLabels[0]: ""));
								Console.WriteLine("     Authority Number: " + (makerRecord.authorityNumbers != null && makerRecord.authorityNumbers.Length>0? makerRecord.authorityNumbers[0]: ""));
								Console.WriteLine("Authority Number Type: " + (makerRecord.authorityNumberTypes != null && makerRecord.authorityNumberTypes.Length>0? makerRecord.authorityNumberTypes[0].ToString(): ""));

								Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
							}
							
							//check if there are more records to retrieve
							if(esDocumentMaker.dataRecords.Length >= recordsMaxAmount){
								recordsStartIndex = recordsStartIndex + recordsMaxAmount;
								getMoreRecords = true;
							}
						}
					}else{
						Console.WriteLine("FAIL - not all organisation maker data could be obtained from the platform. Reason: " + endpointResponseESD.result_message  + " Error Code: " + endpointResponseESD.result_code);
					}
				}
			}
			
			//next steps
			//call other API endpoints...
			//destroy API session when done...
			apiOrgSession.destroyOrgSession();
		}
	}
}