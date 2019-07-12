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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then imports Attribute data into the platform against a specified organisation</summary>
    public class APIv1ExampleRunnerImportOrgESDDataAttributes
	{
        public static void runAPIv1ExampleRunnerImportOrgESDAttributeData()
        {
            Console.WriteLine("Example - Import Organisation Attribute Data");
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
			
			//import attribute data if the API was successfully created
			if(apiOrgSession.doesSessionExist())
			{
				//create attribute profile records
				List<ESDRecordAttributeProfile> attributeProfileRecords = new List<ESDRecordAttributeProfile>();

				//create first attribute profile record
				ESDRecordAttributeProfile attributeProfileRecord = new ESDRecordAttributeProfile();
				attributeProfileRecord.keyAttributeProfileID = "PAP002";
				attributeProfileRecord.name = "Clothing Styling";
				attributeProfileRecord.description = "View the styling details of clothes";
				attributeProfileRecords.Add(attributeProfileRecord);
				List<ESDRecordAttribute> attributes = new List<ESDRecordAttribute>();

				//add attribute record to the 1st attribute profile
				ESDRecordAttribute attributeRecord = new ESDRecordAttribute();
				attributeRecord.keyAttributeID = "PAP002-1";
				attributeRecord.name = "Colour";
				attributeRecord.dataType = ESDRecordAttribute.DATA_TYPE_STRING;
				attributes.Add(attributeRecord);

				//add 2nd attribute record to the 1st attribute profile
				attributeRecord = new ESDRecordAttribute();
				attributeRecord.keyAttributeID = "PAP002-2";
				attributeRecord.name = "Size";
				attributeRecord.dataType = ESDRecordAttribute.DATA_TYPE_NUMBER;
				attributes.Add(attributeRecord);

				//add 3rd attribute record to the 1st attribute profile
				attributeRecord = new ESDRecordAttribute();
				attributeRecord.keyAttributeID = "PAP002-3";
				attributeRecord.name = "Texture";
				attributeRecord.dataType = ESDRecordAttribute.DATA_TYPE_STRING;
				attributes.Add(attributeRecord);
				
				//add list of attributes to the attribute profile
				attributeProfileRecord.attributes = attributes.ToArray();
				attributes.Clear();

				//create 2nd attribute profile record
				attributeProfileRecord = new ESDRecordAttributeProfile();
				attributeProfileRecord.keyAttributeProfileID = "MAKEMODELCAR";
				attributeProfileRecord.name = "Make/Model Vehicle Details";
				attributeProfileRecord.description = "Details about the characteristics of automotive vehicles";
				attributeProfileRecords.Add(attributeProfileRecord);

				//add 1st attribute record to the 2nd attribute profile
				attributeRecord = new ESDRecordAttribute();
				attributeRecord.keyAttributeID = "MMCAR-TYPE";
				attributeRecord.name = "Vehicle Type";
				attributeRecord.dataType = ESDRecordAttribute.DATA_TYPE_STRING;
				attributes.Add(attributeRecord);

				//add 2nd attribute record to the 2nd attribute profile
				attributeRecord = new ESDRecordAttribute();
				attributeRecord.keyAttributeID = "MMCAR-ENGINE-CYLINDERS";
				attributeRecord.name = "Number of Cyclinders";
				attributeRecord.dataType = ESDRecordAttribute.DATA_TYPE_NUMBER;
				attributes.Add(attributeRecord);

				//add 3rd attribute record to the 2nd attribute profile
				attributeRecord = new ESDRecordAttribute();
				attributeRecord.keyAttributeID = "MMCAR-FUEL-TANK-LITRES";
				attributeRecord.name = "Fuel Tank Size (Litres)";
				attributeRecord.dataType = ESDRecordAttribute.DATA_TYPE_NUMBER;
				attributes.Add(attributeRecord);

				//add 4th attribute record to the 2nd attribute profile
				attributeRecord = new ESDRecordAttribute();
				attributeRecord.keyAttributeID = "MMCAR-WHEELSIZE-RADIUS-INCH";
				attributeRecord.name = "Wheel Size (Inches)";
				attributeRecord.dataType = ESDRecordAttribute.DATA_TYPE_NUMBER;
				attributes.Add(attributeRecord);

				//add 5th attribute record to the 2nd attribute profile
				attributeRecord = new ESDRecordAttribute();
				attributeRecord.keyAttributeID = "MMCAR-WHEELSIZE-TREAD";
				attributeRecord.name = "Tyre Tread";
				attributeRecord.dataType = ESDRecordAttribute.DATA_TYPE_STRING;
				attributes.Add(attributeRecord);
				
				//add the list of attributes to the 2nd attribute profile
				attributeProfileRecord.attributes = attributes.ToArray();
                attributes.Clear();

				//create product attribute values array
				List<ESDRecordAttributeValue> productAttributeValueRecords = new List<ESDRecordAttributeValue>();

				//set attribute values for products, for product PROD-001 set the clothing attributes colour=red, size=8 and 10, texture = soft
				ESDRecordAttributeValue attributeValueRecord = new ESDRecordAttributeValue();
				attributeValueRecord.keyProductID = "PROD-001";
				attributeValueRecord.keyAttributeProfileID = "PAP002";
				attributeValueRecord.keyAttributeID = "PAP002-1";
				attributeValueRecord.stringValue = "Red";
				productAttributeValueRecords.Add(attributeValueRecord);

				attributeValueRecord = new ESDRecordAttributeValue();
				attributeValueRecord.keyProductID = "PROD-001";
				attributeValueRecord.keyAttributeProfileID = "PAP002";
				attributeValueRecord.keyAttributeID = "PAP002-2";
				attributeValueRecord.numberValue = 8;
				productAttributeValueRecords.Add(attributeValueRecord);

				attributeValueRecord = new ESDRecordAttributeValue();
				attributeValueRecord.keyProductID = "PROD-001";
				attributeValueRecord.keyAttributeProfileID = "PAP002";
				attributeValueRecord.keyAttributeID = "PAP002-2";
				attributeValueRecord.numberValue = 10;
				productAttributeValueRecords.Add(attributeValueRecord);

				attributeValueRecord = new ESDRecordAttributeValue();
				attributeValueRecord.keyProductID = "PROD-001";
				attributeValueRecord.keyAttributeProfileID = "PAP002";
				attributeValueRecord.keyAttributeID = "PAP002-3";
				attributeValueRecord.stringValue = "soft";
				productAttributeValueRecords.Add(attributeValueRecord);
				
				//create a hashmap containing configurations of the organisation attribute data
				Dictionary<String, String> configs = new Dictionary<String, String>();
				
				//add a dataFields attribute that contains a comma delimited list of attribute record fields that the API is allowed to insert and update in the platform
				configs["dataFields"] = "keyProductID,keyAttributeProfileID,keyAttributeID,stringValue,numberValue";
				
				//create a Ecommerce Standards Document that stores an array of attribute records
				ESDocumentAttribute attributeESD = new ESDocumentAttribute(ESDocumentConstants.RESULT_SUCCESS, "successfully obtained data", attributeProfileRecords.ToArray(), productAttributeValueRecords.ToArray(), configs);
				
				//after 30 seconds give up on waiting for a response from the API when creating the notification
				int timeoutMilliseconds = 30000;
				
				//call the platform's API to import in the organisation's attribute data
				APIv1EndpointResponseESD<ESDocument> endpointResponseESD = APIv1EndpointOrgImportESDocument.call(apiOrgSession, timeoutMilliseconds, APIv1EndpointOrgImportESDocument.IMPORT_TYPE_ID_ATTRIBUTES, attributeESD);
				
				//check that the data successfully imported
				if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS){
                    Console.WriteLine("SUCCESS - attribute data successfully imported into the platform against the organisation");
                }
                else{
                    Console.WriteLine("FAIL - attribute data failed to be imported into the platform against the organisation. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);
                }
			}
			
			//next steps
			//call other API endpoints...
			//destroy API session when done
			apiOrgSession.destroyOrgSession();
		}
	}
}