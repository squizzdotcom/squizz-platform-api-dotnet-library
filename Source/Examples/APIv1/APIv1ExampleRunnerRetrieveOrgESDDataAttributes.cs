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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then retrieves Attribute data from a connected organisation in the platform</summary>
    public class APIv1ExampleRunnerRetrieveOrgESDDataAttributes
	{
        public static void runAPIv1ExampleRunnerRetrieveOrgESDDataAttribute()
        {
            Console.WriteLine("Example - Retrieve Supplier Organisation Attribute Data");
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
			int attributeProfileRecordNumber = 1;
			int attributesRecordNumber = 1;
			int pageNumber = 0;
			string result = "FAIL";
			Dictionary<string, ESDRecordAttributeProfile> attributeProfilesRecordIndex = new Dictionary<string, ESDRecordAttributeProfile>();
            Dictionary<string, ESDRecordAttribute> attributesRecordIndex = new Dictionary<string, ESDRecordAttribute>();
            Dictionary<string, ESDRecordProduct> productsRecordIndex = new Dictionary<string, ESDRecordProduct>();
			
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
			
			//retrieve product data, since it is used to find details of products associated to retrieved attribute values
			if(apiOrgSession.doesSessionExist())
			{
                Console.WriteLine("Attempting to obtain product data.");
				recordsStartIndex = 0;
				pageNumber = 0;
				getMoreRecords = true;
				
				//after 120 seconds give up on waiting for a response from the API
				int timeoutMilliseconds = 120000;

				//get the next page of records if needed
				while(getMoreRecords)
				{
					pageNumber++;
					getMoreRecords = false;
					
					//call the platform's API to retrieve the organisation's product data
					APIv1EndpointResponseESD<ESDocumentProduct> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrieveProducts(apiOrgSession, timeoutMilliseconds, supplierOrgID, recordsMaxAmount, recordsStartIndex, "");

                    Console.WriteLine("Attempt made to obtain product data. Page number: " +pageNumber);

					//check that the data successfully retrieved
					if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
					{	
						ESDocumentProduct esDocumentProduct = (ESDocumentProduct)endpointResponseESD.esDocument;
						
						//check that records have been placed into the standards document
						if(esDocumentProduct.dataRecords != null)
						{
							//iterate through each product record stored within the standards document
							foreach (ESDRecordProduct productRecord in esDocumentProduct.dataRecords){
								productsRecordIndex[productRecord.keyProductID] = productRecord;
							}

							//check if there are more records to retrieve
							if(esDocumentProduct.dataRecords.Length >= recordsMaxAmount)
							{
								recordsStartIndex = recordsStartIndex + recordsMaxAmount;
								getMoreRecords = true;
							}
						}else{
                            Console.WriteLine("No more records obtained. Page number: " +pageNumber);
						}
						
						result = "SUCCESS";
					}else{
						result = "FAIL";
                        Console.WriteLine("FAIL - not all organisation product data could be obtained from the platform. Reason: " + endpointResponseESD.result_message  + " Error Code: " + endpointResponseESD.result_code);
					}
				}
			}
			
			//retrieve attribute data if a session was successfully created
			if(result.ToUpper() == "SUCCESS")
			{
                Console.WriteLine("Attempting to obtain attribute data.");
				recordsStartIndex = 0;
				getMoreRecords = true;
				
				//after 120 seconds give up on waiting for a response from the API
				int timeoutMilliseconds = 120000;

				//get the next page of records if needed
				while(getMoreRecords)
				{
					getMoreRecords = false;
					pageNumber++;
					
					//call the platform's API to retrieve the organisation's attribute data
					APIv1EndpointResponseESD<ESDocumentAttribute> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrieveAttributes(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", recordsMaxAmount, recordsStartIndex, "");
                    Console.WriteLine("SUCCESS - attribute data successfully obtained from the platform for page number: " +pageNumber);

					//check that the data successfully retrieved
					if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
					{
						ESDocumentAttribute esDocumentAttribute = (ESDocumentAttribute)endpointResponseESD.esDocument;

						//check that attribute profile records have been placed into the standards document
						if(recordsStartIndex == 0 && esDocumentAttribute.attributeProfiles != null)
						{
							//iterate through each attribute record stored within the standards document
							foreach (ESDRecordAttributeProfile attributeProfileRecord in esDocumentAttribute.attributeProfiles){
								attributeProfilesRecordIndex[attributeProfileRecord.keyAttributeProfileID] = attributeProfileRecord;

                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("Key Attribute Profile ID: " + attributeProfileRecord.keyAttributeProfileID);
                                Console.WriteLine("            Profile Name: " + attributeProfileRecord.name);

								//iterate through each attribute assigned to the attribute profile
								foreach (ESDRecordAttribute attributeRecord in attributeProfileRecord.attributes){
									attributesRecordIndex[attributeRecord.keyAttributeID] = attributeRecord;

                                    Console.WriteLine();
                                    Console.WriteLine("        Key Attribute ID: " + attributeRecord.keyAttributeID);
                                    Console.WriteLine("          Attribute Name: " + attributeRecord.name);
                                    Console.WriteLine("     Attribute Data Type: " + attributeRecord.dataType);
								}

                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
							}
						}else{
                            Console.WriteLine("No more records obtained.");
						}
						
						//check that product attribute value records have been placed into the standards document
						if(esDocumentAttribute.dataRecords != null)
						{
							//iterate through each attribute value record stored within the standards document
							foreach (ESDRecordAttributeValue attributeValueRecord in esDocumentAttribute.dataRecords)
							{
								String attributeValue;
								String productName = "";
								String productCode = "";
								String attributeName = "";
								String attributeProfileName = "";
								String attributeDataType = "";

								//lookup the product assigned to the attribute value and gets its name (or any other product details you wish)
								if(productsRecordIndex.ContainsKey(attributeValueRecord.keyProductID)){
									productCode = productsRecordIndex[attributeValueRecord.keyProductID].productCode;
									productName = productsRecordIndex[attributeValueRecord.keyProductID].name;
								}

								//get details of the attribute profile the attribute value is linked to
								if(attributeProfilesRecordIndex.ContainsKey(attributeValueRecord.keyAttributeProfileID)){
									attributeProfileName = attributeProfilesRecordIndex[attributeValueRecord.keyAttributeProfileID].name;
								}

								//get details of the attribute that the attribute value is linked to
								if(attributesRecordIndex.ContainsKey(attributeValueRecord.keyAttributeID)){
									attributeName = attributesRecordIndex[attributeValueRecord.keyAttributeID].name;
									attributeDataType = attributesRecordIndex[attributeValueRecord.keyAttributeID].dataType;
								}

								// get the attribute value based on the attribute's data type
								if(attributeDataType.ToUpper() == ESDRecordAttribute.DATA_TYPE_NUMBER){
									attributeValue = attributeValueRecord.numberValue.ToString();
								}else{
									attributeValue = attributeValueRecord.stringValue;
								}

                                //output details of the product attribute value record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("Product Attribute Value Record #: " + recordNumber);
                                Console.WriteLine("                  Key Product ID: " + attributeValueRecord.keyProductID);
                                Console.WriteLine("                    Product Code: " + productCode);
                                Console.WriteLine("                    Product Name: " + productName);
                                Console.WriteLine("        Key Attribute Profile ID: " + attributeValueRecord.keyAttributeProfileID);
                                Console.WriteLine("                    Profile Name: " + attributeProfileName);
                                Console.WriteLine("                Key Attribute ID: " + attributeValueRecord.keyAttributeID);
                                Console.WriteLine("                  Attribute Name: " + attributeName);
                                Console.WriteLine("                 Attribute Value: " + attributeValue);

								recordNumber++;
							}

							//check if there are more records to retrieve
							if(esDocumentAttribute.dataRecords.Length >= recordsMaxAmount)
							{
								recordsStartIndex = recordsStartIndex + recordsMaxAmount;
								getMoreRecords = true;
							}
						}

						result = "SUCCESS";
					}else{
						result = "FAIL";
                        Console.WriteLine("FAIL - not all organisation attribute data could be obtained from the platform. Reason: " + endpointResponseESD.result_message  + " Error Code: " + endpointResponseESD.result_code);
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