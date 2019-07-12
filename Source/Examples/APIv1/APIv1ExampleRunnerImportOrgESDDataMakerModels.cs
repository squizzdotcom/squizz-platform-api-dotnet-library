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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then imports Maker Model data into the platform against a specified organisation</summary>
    public class APIv1ExampleRunnerImportOrgESDDataMakerModels
	{
        public static void runAPIv1ExampleRunnerImportOrgESDMakerModelData()
        {
            Console.WriteLine("Example - Import Organisation Maker Model Data");
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
			
			//import make model data if the API was successfully created
			if(apiOrgSession.doesSessionExist())
			{
				//create maker model records
				List<ESDRecordMakerModel> makerModelRecords = new List<ESDRecordMakerModel>();
				ESDRecordMakerModel makerModelRecord = new ESDRecordMakerModel();
				makerModelRecord.keyMakerModelID = "2";
				makerModelRecord.keyMakerID = "2";
				makerModelRecord.modelCode = "SEDAN1";
				makerModelRecord.modelSubCode = "1ABC";
				makerModelRecord.name = "Sahara Luxury Sedan 2016";
				makerModelRecord.modelSearchCode = "Car-Manufacturer-A-Saraha-Luxury-Sedan-2016";
				makerModelRecord.groupClass = "SEDAN";
				makerModelRecord.releasedDate = 1456750800000l;
				makerModelRecord.createdDate = 1430748000000l;
				makerModelRecord.attributes = new List<ESDRecordAttributeValue>();

				//add attribute value records against the model record
				ESDRecordAttributeValue attributeValueRecord = new ESDRecordAttributeValue();
				attributeValueRecord.keyAttributeProfileID = "MAKEMODELCAR";
				attributeValueRecord.keyAttributeID = "MMCAR-TYPE";
				attributeValueRecord.stringValue = "Sedan";
				makerModelRecord.attributes.Add(attributeValueRecord);

				//add 2nd attribute value to the model
				attributeValueRecord = new ESDRecordAttributeValue();
				attributeValueRecord.keyAttributeProfileID = "MAKEMODELCAR";
				attributeValueRecord.keyAttributeID = "MMCAR-ENGINE-CYLINDERS";
				attributeValueRecord.numberValue = 4;
				makerModelRecord.attributes.Add(attributeValueRecord);

				//add 3rd attribute value to the model
				attributeValueRecord = new ESDRecordAttributeValue();
				attributeValueRecord.keyAttributeProfileID = "MAKEMODELCAR";
				attributeValueRecord.keyAttributeID = "MMCAR-FUEL-TANK-LITRES";
				attributeValueRecord.numberValue = (decimal)80.5;
				makerModelRecord.attributes.Add(attributeValueRecord);

				//add model record to the list of models
				makerModelRecords.Add(makerModelRecord);

				//create 2nd maker model record
				makerModelRecord = new ESDRecordMakerModel();
				makerModelRecord.keyMakerModelID = "3";
				makerModelRecord.keyMakerID = "2";
				makerModelRecord.modelCode = "TRUCK22";
				makerModelRecord.modelSubCode = "EX";
				makerModelRecord.name = "City Truck 2016";
				makerModelRecord.modelSearchCode = "Car-Manufacturer-A-City-Truck-2016";
				makerModelRecord.groupClass = "TRUCK";
				makerModelRecord.releasedDate = 1456750800000l;
				makerModelRecord.createdDate = 1430748000000l;
				makerModelRecord.attributes  = new List<ESDRecordAttributeValue>();

				//add attribute value records against the model record
				attributeValueRecord = new ESDRecordAttributeValue();
				attributeValueRecord.keyAttributeProfileID = "MAKEMODELCAR";
				attributeValueRecord.keyAttributeID = "MMCAR-TYPE";
				attributeValueRecord.stringValue = "Truck";
				makerModelRecord.attributes.Add(attributeValueRecord);

				//add 2nd attribute value to the model
				attributeValueRecord = new ESDRecordAttributeValue();
				attributeValueRecord.keyAttributeProfileID = "MAKEMODELCAR";
				attributeValueRecord.keyAttributeID = "MMCAR-ENGINE-CYLINDERS";
				attributeValueRecord.numberValue = 6;
				makerModelRecord.attributes.Add(attributeValueRecord);

				//add 3rd attribute value to the model
				attributeValueRecord = new ESDRecordAttributeValue();
				attributeValueRecord.keyAttributeProfileID = "MAKEMODELCAR";
				attributeValueRecord.keyAttributeID = "MMCAR-FUEL-TANK-LITRES";
				attributeValueRecord.numberValue = 140;
				makerModelRecord.attributes.Add(attributeValueRecord);
				
				//add the 2nd model record to the list of models
				makerModelRecords.Add(makerModelRecord);
				
				//create a hashmap containing configurations of the organisation maker model data
				Dictionary<string, string> configs = new Dictionary<string, string>();
				
				//add a dataFields attribute that contains a comma delimited list of maker model record fields that the API is allowed to insert and update in the platform
				configs["dataFields"] = "keyMakerModelID,keyMakerID,modelCode,modelSubCode,name,modelSearchCode,groupClass,releasedDate,createdDate,attributes";
				
				//create a Ecommerce Standards Document that stores an array of maker model records
				ESDocumentMakerModel makerModelESD = new ESDocumentMakerModel(ESDocumentConstants.RESULT_SUCCESS, "successfully obtained data", makerModelRecords.ToArray(), configs);
				
				//after 30 seconds give up on waiting for a response from the API when creating the notification
				int timeoutMilliseconds = 30000;
				
				//call the platform's API to import in the organisation's maker model data
				APIv1EndpointResponseESD<ESDocument> endpointResponseESD = APIv1EndpointOrgImportESDocument.call(apiOrgSession, timeoutMilliseconds, APIv1EndpointOrgImportESDocument.IMPORT_TYPE_ID_MAKER_MODELS, makerModelESD);
				
				//check that the data successfully imported
				if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS){
                    Console.WriteLine("SUCCESS - maker model data successfully imported into the platform against the organisation");
                }
                else{
                    Console.WriteLine("FAIL - maker model data failed to be imported into the platform against the organisation. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);
                }
			}
			
			//next steps
			//call other API endpoints...
			//destroy API session when done
			apiOrgSession.destroyOrgSession();
		}
	}
}