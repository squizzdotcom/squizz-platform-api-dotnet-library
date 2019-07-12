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
	/**
	 * Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then imports Maker data into the platform against a specified organisation
	 */
	public class APIv1ExampleRunnerImportOrgESDDataMakers
	{
        public static void runAPIv1ExampleRunnerImportOrgESDMakerData()
        {
            Console.WriteLine("Example - Import Organisation Maker Data");
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
			if(endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
			{
                //session has been created so now can call other API endpoints
                Console.WriteLine("SUCCESS - API session has successfully been created.");
            }
			else
			{
                //session failed to be created
                Console.WriteLine("FAIL - API session failed to be created. Reason: " + endpointResponse.result_message + " Error Code: " + endpointResponse.result_code);
            }
			
			//import make data if the API was successfully created
			if(apiOrgSession.doesSessionExist())
			{
				//create maker records
				List<ESDRecordMaker> makerRecords = new List<ESDRecordMaker>();
				
				//create 1st maker record
				ESDRecordMaker makerRecord = new ESDRecordMaker();
				makerRecord.keyMakerID = "1";
				makerRecord.makerCode = "CAR1";
				makerRecord.name = "Car Manufacturer X";
				makerRecord.makerSearchCode = "Car-Manufacturer-X-Sedans-Wagons-Trucks";
				makerRecord.groupClass = "POPULAR CARS";
				makerRecord.ordering = 3;
				makerRecord.establishedDate = 1449132083087l;
				makerRecord.orgName = "Car Manufacturer X";
				makerRecord.authorityNumbers = new String[]{"988776643221"};
				makerRecord.authorityNumberLabels = new String[]{"Australian Business Number"};
				makerRecord.authorityNumberTypes = new int[]{ESDocumentConstants.AUTHORITY_NUM_AUS_ABN};
				makerRecords.Add(makerRecord);

				//add 2nd maker record
				makerRecord = new ESDRecordMaker();
				makerRecord.keyMakerID = "2";
				makerRecord.makerCode = "CAR2";
				makerRecord.name = "Car Manufacturer A";
				makerRecord.makerSearchCode = "Car-Manufacturer-A";
				makerRecord.groupClass = "POPULAR CARS";
				makerRecord.ordering = 2;
				makerRecord.establishedDate = 1449132083084l;
				makerRecord.orgName = "Car Manufacturer A";
				makerRecord.authorityNumbers = new String[]{"123456789 1234"};
				makerRecord.authorityNumberLabels = new String[]{"Australian Business Number"};
				makerRecord.authorityNumberTypes = new int[]{ESDocumentConstants.AUTHORITY_NUM_AUS_ABN};
				makerRecords.Add(makerRecord);

				//add 3rd maker record
				makerRecord = new ESDRecordMaker();
				makerRecord.keyMakerID = "3";
				makerRecord.makerCode = "CAR3";
				makerRecord.name = "Car Manufacturer B";
				makerRecord.makerSearchCode = "Car-Manufacturer-B-Sedans-Wagons";
				makerRecord.groupClass = "CUSTOM CARS";
				makerRecord.ordering = 1;
				makerRecord.establishedDate = 1449132083085l;
				makerRecord.orgName = "Car Manufacturer B";
				makerRecord.authorityNumbers = new String[]{"98877664322"};
				makerRecord.authorityNumberLabels = new String[]{"Australian Business Number"};
				makerRecord.authorityNumberTypes = new int[]{ESDocumentConstants.AUTHORITY_NUM_AUS_ABN};
				makerRecords.Add(makerRecord);
				
				//create a hashmap containing configurations of the organisation maker data
				Dictionary<String, String> configs = new Dictionary<String, String>();
				
				//add a dataFields attribute that contains a comma delimited list of maker record fields that the API is allowed to insert and update in the platform
				configs["dataFields"] = "keyMakerID,makerCode,name,makerSearchCode,groupClass,ordering,establishedDate,orgName,authorityNumbers,authorityNumberLabels,authorityNumberTypes";
				
				//create a Ecommerce Standards Document that stores an array of maker records
				ESDocumentMaker makerESD = new ESDocumentMaker(ESDocumentConstants.RESULT_SUCCESS, "successfully obtained data", makerRecords.ToArray(), configs);
				
				//after 30 seconds give up on waiting for a response from the API when creating the notification
				int timeoutMilliseconds = 30000;
				
				//call the platform's API to import in the organisation's maker data
				APIv1EndpointResponseESD<ESDocument> endpointResponseESD = APIv1EndpointOrgImportESDocument.call(apiOrgSession, timeoutMilliseconds, APIv1EndpointOrgImportESDocument.IMPORT_TYPE_ID_MAKERS, makerESD);
				
				//check that the data successfully imported
				if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS){
                    Console.WriteLine("SUCCESS - maker organisation data successfully imported into the platform");
                }else{
                    Console.WriteLine("FAIL - maker organisation data failed to be imported into the platform. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);
                }
			}
			
			//next steps
			//call other API endpoints...
			//destroy API session when done
			apiOrgSession.destroyOrgSession();
		}
	}
}