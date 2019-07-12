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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then retrieves Maker Model data from a connected organisation in the platform</summary>
    public class APIv1ExampleRunnerRetrieveOrgESDDataMakerModels
	{
        public static DateTime EPOCH_DATE_TIME = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static string RECORD_DATE_FORMAT = "dd-MM-yyyy";

        public static void runAPIv1ExampleRunnerRetrieveOrgESDDataMakerModels()
        {
            Console.WriteLine("Example - Retrieve Supplier Organisation Maker Model Data");
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
			bool result = false;
			Dictionary<string, ESDRecordMaker> makersRecordIndex = new Dictionary<string, ESDRecordMaker>();
            Dictionary<string, ESDRecordAttributeProfile> attributeProfilesRecordIndex = new Dictionary<string, ESDRecordAttributeProfile>();
            Dictionary<string, ESDRecordAttribute> attributesRecordIndex = new Dictionary<string, ESDRecordAttribute>();
			
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
				Console.WriteLine("Attempting to obtain maker data.");
				
				//after 120 seconds give up on waiting for a response from the API
				int timeoutMilliseconds = 120000;

				//get the next page of records if needed
				while(getMoreRecords)
				{
					pageNumber++;
					getMoreRecords = false;
					
					//call the platform's API to retrieve the organisation's maker data
					APIv1EndpointResponseESD<ESDocumentMaker> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrieveMakers(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", recordsMaxAmount, recordsStartIndex, "");

					Console.WriteLine("Attempt made to obtain maker data. Page number: "+pageNumber);

					//check that the data successfully retrieved
					if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
					{	
						ESDocumentMaker esDocumentMaker = (ESDocumentMaker)endpointResponseESD.esDocument;
						
						//check that records have been placed into the standards document
						if(esDocumentMaker.dataRecords != null)
						{
							//iterate through each maker record stored within the standards document
							foreach (ESDRecordMaker makerRecord in esDocumentMaker.dataRecords){
								makersRecordIndex[makerRecord.keyMakerID] = makerRecord;
							}

							//check if there are more records to retrieve
							if(esDocumentMaker.dataRecords.Length >= recordsMaxAmount)
							{
								recordsStartIndex = recordsStartIndex + recordsMaxAmount;
								getMoreRecords = true;
							}
						}else{
							Console.WriteLine("No more records obtained. Page number: "+pageNumber);
						}
						
						result = true;
					}else{
						result = false;
						Console.WriteLine("FAIL - not all organisation maker data could be obtained from the platform. Reason: " + endpointResponseESD.result_message  + " Error Code: " + endpointResponseESD.result_code);
					}
				}
			}
			
			//retrieve attribute data, since it may be used if maker models contains attributes assigned to them
			if(result)
			{
				Console.WriteLine("Attempting to obtain attribute data.");
				
				result = false;
				recordsStartIndex = 0;
				
				//after 120 seconds give up on waiting for a response from the API
				int timeoutMilliseconds = 120000;

				//call the platform's API to retrieve the organisation's attribute profile data, ignore getting product attribute value records
				APIv1EndpointResponseESD<ESDocumentAttribute> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrieveAttributes(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", recordsMaxAmount, recordsStartIndex, "ignore_products=Y");
				Console.WriteLine("Attempt made to obtain attribute data.");

				//check that the data successfully retrieved
				if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
				{
					ESDocumentAttribute esDocumentAttribute = (ESDocumentAttribute)endpointResponseESD.esDocument;

					//check that attribute profile records have been placed into the standards document
					if(esDocumentAttribute.attributeProfiles != null)
					{
						//iterate through each attribute record stored within the standards document
						foreach (ESDRecordAttributeProfile attributeProfileRecord in esDocumentAttribute.attributeProfiles){
							attributeProfilesRecordIndex[attributeProfileRecord.keyAttributeProfileID] = attributeProfileRecord;
							
							//iterate through each attribute assigned to the attribute profile
							foreach (ESDRecordAttribute attributeRecord in attributeProfileRecord.attributes){
								attributesRecordIndex[attributeRecord.keyAttributeID] = attributeRecord;
							}
						}
					}else{
						Console.WriteLine("No more records obtained.");
					}
					
					result = true;
				}else{
					result = false;
					Console.WriteLine("FAIL - not all organisation attribute data could be obtained from the platform. Reason: " + endpointResponseESD.result_message  + " Error Code: " + endpointResponseESD.result_code);
				}
			}
			
			//retrieve organisation maker model data if maker and attribute data retrieved
			if(result)
			{
				Console.WriteLine("Attempting to obtain maker model data.");
				
				recordsStartIndex = 0;
				pageNumber = 0;
				getMoreRecords = true;
				
				//after 60 seconds give up on waiting for a response from the API when creating the notification
				int timeoutMilliseconds = 60000;
				
				//get the next page of records if needed
				while(getMoreRecords)
				{
					getMoreRecords = false;
					pageNumber++;
					
					//call the platform's API to retrieve the organisation's maker model data
					APIv1EndpointResponseESD<ESDocumentMakerModel> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrieveMakerModels(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", recordsMaxAmount, recordsStartIndex, "");

					//check that the data successfully retrieved
					if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS){
						Console.WriteLine("SUCCESS - maker model data successfully obtained from the platform for page number: "+pageNumber);

						//process and output maker records
						ESDocumentMakerModel esDocumentMakerModel = (ESDocumentMakerModel)endpointResponseESD.esDocument;
						Console.WriteLine("Maker Model Records Returned: " + esDocumentMakerModel.totalDataRecords);

						//check that records have been placed into the standards document
						if(esDocumentMakerModel.dataRecords != null){
							Console.WriteLine("Maker Model Records:");

							//iterate through each maker model record stored within the standards document
							foreach(ESDRecordMakerModel makerModelRecord in esDocumentMakerModel.dataRecords)
							{    
								recordNumber++;
								
								//lookup the maker assigned to the model and gets its name (or any other maker details you wish)
								String makerName = "";
								if(makersRecordIndex.ContainsKey(makerModelRecord.keyMakerID)){
									makerName = makersRecordIndex[makerModelRecord.keyMakerID].name;
								}
								
								//output details of the maker model record
								Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
								Console.WriteLine("    Maker Model Record #: " + recordNumber);
								Console.WriteLine("      Key Maker Model ID: " + makerModelRecord.keyMakerModelID);
								Console.WriteLine("              Model Code: " + makerModelRecord.modelCode);
								Console.WriteLine("          Model Sub Code: " + makerModelRecord.modelSubCode);
								Console.WriteLine("              Model Name: " + makerModelRecord.name);
								Console.WriteLine("            Key Maker ID: " + makerModelRecord.keyMakerID);
								Console.WriteLine("              Maker Name: " + makerName);
								Console.WriteLine("       Model Search Code: " + makerModelRecord.modelSearchCode);
								Console.WriteLine("           Released Date: " + EPOCH_DATE_TIME.AddMilliseconds(makerModelRecord.releasedDate).ToString(RECORD_DATE_FORMAT));
								Console.WriteLine("            Created Date: " + EPOCH_DATE_TIME.AddMilliseconds(makerModelRecord.createdDate).ToString(RECORD_DATE_FORMAT));

                                //check if the model contains any attributes
                                if (makerModelRecord.attributes.Count > 0)
								{
									//output attributes
									Console.WriteLine("       Attributes Values: " + makerModelRecord.attributes.Count);

									//output each attribute value
									int attributValueCount = 0;
									foreach (ESDRecordAttributeValue attributeValueRecord in makerModelRecord.attributes)
									{						
										attributValueCount++;
										
										//check that the attribute has been obtained
										if(attributesRecordIndex.ContainsKey(attributeValueRecord.keyAttributeID)){

											ESDRecordAttribute attributeRecord = attributesRecordIndex[attributeValueRecord.keyAttributeID];
											String attributeName = attributeRecord.name;
											String attributeDataType = attributeRecord.dataType;
											String attributeProfileName = "";
											String attributeValue = "";

											//get the name of the attribute profile that the attribute is linked to
											if(attributeProfilesRecordIndex.ContainsKey(attributeValueRecord.keyAttributeProfileID)){
												attributeProfileName = attributeProfilesRecordIndex[attributeValueRecord.keyAttributeProfileID].name;
											}
											
											// get the model's attribute value based on its data type
											if(attributeDataType.ToUpper() == ESDRecordAttribute.DATA_TYPE_NUMBER){
												attributeValue = attributeValueRecord.numberValue.ToString();
											}else{
												attributeValue = attributeValueRecord.stringValue;
											}
											
											//out attribute value data
											Console.WriteLine("");
											Console.WriteLine("       Attribute Value #: " + attributValueCount);
											Console.WriteLine("Key Attribute Profile ID: " + attributeValueRecord.keyAttributeProfileID);
											Console.WriteLine("  Attribute Profile Name: " + attributeProfileName);
											Console.WriteLine("        Key Attribute ID: " + attributeValueRecord.keyAttributeID);
											Console.WriteLine("          Attribute Name: " + attributeName);
											Console.WriteLine("         Attribute Value: " + attributeValue);
										}
									}
								}

								Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
							}
							
							//check if there are more records to retrieve
							if(esDocumentMakerModel.dataRecords.Length >= recordsMaxAmount){
								recordsStartIndex = recordsStartIndex + recordsMaxAmount;
								getMoreRecords = true;
							}
						}
					}else{
						Console.WriteLine("FAIL - not all organisation maker model data could be obtained from the platform. Reason: " + endpointResponseESD.result_message  + " Error Code: " + endpointResponseESD.result_code);
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