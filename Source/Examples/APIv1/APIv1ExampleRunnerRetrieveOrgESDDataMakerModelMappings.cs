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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then retrieves Maker Model Mapping data from a connected organisation in the platform</summary>
    public class APIv1ExampleRunnerRetrieveOrgESDDataMakerModelMappings
	{
        public static void runAPIv1ExampleRunnerRetrieveOrgESDDataMakerModelMappings()
        {
            Console.WriteLine("Example - Retrieve Supplier Organisation Maker Model Mapping Data");
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
			Dictionary<string, ESDRecordMakerModel> makerModelsRecordIndex = new Dictionary<string, ESDRecordMakerModel>();
            Dictionary<string, ESDRecordAttributeProfile> attributeProfilesRecordIndex = new Dictionary<string, ESDRecordAttributeProfile>();
            Dictionary<string, ESDRecordAttribute> attributesRecordIndex = new Dictionary<string, ESDRecordAttribute>();
            Dictionary<string, ESDRecordProduct> productsRecordIndex = new Dictionary<string, ESDRecordProduct>();
            Dictionary<string, ESDRecordCategory> categoriesRecordIndex = new Dictionary<string, ESDRecordCategory>();
			
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
			
			//retrieve organisation maker model data if the API was successfully created
			if(apiOrgSession.doesSessionExist())
			{
                Console.WriteLine("Attempting to obtain maker model data.");
				
				//after 120 seconds give up on waiting for a response from the API
				int timeoutMilliseconds = 120000;

				//get the next page of records if needed
				while(getMoreRecords)
				{
					pageNumber++;
					getMoreRecords = false;
					
					//call the platform's API to retrieve the organisation's maker model data
					APIv1EndpointResponseESD<ESDocumentMakerModel> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrieveMakerModels(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", recordsMaxAmount, recordsStartIndex, "");

					Console.WriteLine("Attempt made to obtain maker model data. Page number: "+pageNumber);

					//check that the data successfully retrieved
					if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
					{	
						ESDocumentMakerModel esDocumentMakerModel = (ESDocumentMakerModel)endpointResponseESD.esDocument;
						
						//check that records have been placed into the standards document
						if(esDocumentMakerModel.dataRecords != null)
						{
							//iterate through each maker model record stored within the standards document
							foreach (ESDRecordMakerModel makerModelRecord in esDocumentMakerModel.dataRecords){
								makerModelsRecordIndex[makerModelRecord.keyMakerID] = makerModelRecord;
							}

							//check if there are more records to retrieve
							if(esDocumentMakerModel.dataRecords.Length >= recordsMaxAmount)
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
						Console.WriteLine("FAIL - not all organisation maker model data could be obtained from the platform. Reason: " + endpointResponseESD.result_message  + " Error Code: " + endpointResponseESD.result_code);
					}
				}
			}
			
			//retrieve attribute data, since it may be used if maker model mappings contains attributes assigned to them
			if(result)
			{
				Console.WriteLine("Attempting to obtain attribute data.");
				
				result = false;
				recordsStartIndex = 0;
				
				//after 120 seconds give up on waiting for a response from the API
				int timeoutMilliseconds = 120000;

				//call the platform's API to retrieve the organisation's attribute data, ignore getting product attribute value data
				APIv1EndpointResponseESD<ESDocumentAttribute> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrieveAttributes(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", recordsMaxAmount, recordsStartIndex, "ignore_products=Y");
				Console.WriteLine("Attempt made to obtain attribute data.");

				//check that the data successfully retrieved
				if(endpointResponseESD.result.ToUpper() ==APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
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
			
			//retrieve product data, since it is used to find details of products assigned within make model mapping records
			if(result)
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

					Console.WriteLine("Attempt made to obtain product data. Page number: "+pageNumber);

					//check that the data successfully retrieved
					if(endpointResponseESD.result.ToUpper() ==APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
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
							Console.WriteLine("No more records obtained. Page number: "+pageNumber);
						}
						
						result = true;
					}else{
						result = false;
						Console.WriteLine("FAIL - not all organisation product data could be obtained from the platform. Reason: " + endpointResponseESD.result_message  + " Error Code: " + endpointResponseESD.result_code);
					}
				}
			}
			
			//retrieve category data, since it is used to find details of categories assigned within make model mapping records
			if(result)
			{
				Console.WriteLine("Attempting to obtain category data.");
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
					
					//call the platform's API to retrieve the organisation's category data, ignore getting products assigned to each category
					APIv1EndpointResponseESD<ESDocumentCategory> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrieveCategories(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", recordsMaxAmount, recordsStartIndex, "ignore_products=Y");

					Console.WriteLine("Attempt made to obtain category data. Page number: "+pageNumber);

					//check that the data successfully retrieved
					if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
					{	
						ESDocumentCategory esDocumentCategory = (ESDocumentCategory)endpointResponseESD.esDocument;
						
						//check that records have been placed into the standards document
						if(esDocumentCategory.dataRecords != null)
						{
							//iterate through each category record stored within the standards document
							foreach (ESDRecordCategory categoryRecord in esDocumentCategory.dataRecords){
								categoriesRecordIndex[categoryRecord.keyCategoryID] = categoryRecord;
							}

							//check if there are more records to retrieve
							if(esDocumentCategory.dataRecords.Length >= recordsMaxAmount)
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
						Console.WriteLine("FAIL - not all organisation category data could be obtained from the platform. Reason: " + endpointResponseESD.result_message  + " Error Code: " + endpointResponseESD.result_code);
					}
				}
			}
			
			//retrieve organisation maker model mapping data if maker model, product, category and attribute data retrieved
			if(result)
			{
				Console.WriteLine("Attempting to obtain maker model mapping data.");
				
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
					
					//call the platform's API to retrieve the organisation's maker model mapping data
					APIv1EndpointResponseESD<ESDocumentMakerModelMapping> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrieveMakerModelMappings(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", recordsMaxAmount, recordsStartIndex, "");

					//check that the data successfully retrieved
					if(endpointResponseESD.result.ToUpper() ==APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS){
						Console.WriteLine("SUCCESS - maker model mapping data successfully obtained from the platform for page number: "+pageNumber);

						//process and output maker model mapping records
						ESDocumentMakerModelMapping esDocumentMakerModelMapping = (ESDocumentMakerModelMapping)endpointResponseESD.esDocument;
						Console.WriteLine("Maker Model Mapping Records Returned: " + esDocumentMakerModelMapping.totalDataRecords);

						//check that records have been placed into the standards document
						if(esDocumentMakerModelMapping.dataRecords != null){
							Console.WriteLine("Maker Model Mapping Records:");

							//iterate through each maker model record stored within the standards document
							foreach (ESDRecordMakerModelMapping makerModelMappingRecord in esDocumentMakerModelMapping.dataRecords)
							{    
								recordNumber++;
								
								String modelCode = "";
								String modelName = "";
								String categoryCode = "";
								String categoryName = "";
								String productCode = "";
								String productName = "";

								//lookup the mapping's model and gets the model name and code
								if(makerModelsRecordIndex.ContainsKey(makerModelMappingRecord.keyMakerModelID)){
									modelCode = makerModelsRecordIndex[makerModelMappingRecord.keyMakerModelID].modelCode;
									modelName = makerModelsRecordIndex[makerModelMappingRecord.keyMakerModelID].name;
								}

								//lookup the mapping's category and gets the category name and code
								if(categoriesRecordIndex.ContainsKey(makerModelMappingRecord.keyCategoryID)){
									categoryCode = categoriesRecordIndex[makerModelMappingRecord.keyCategoryID].categoryCode;
									categoryName = categoriesRecordIndex[makerModelMappingRecord.keyCategoryID].name;
								}

								//lookup the mapping's product and gets the product name and code
								if(productsRecordIndex.ContainsKey(makerModelMappingRecord.keyProductID)){
									productCode = productsRecordIndex[makerModelMappingRecord.keyProductID].productCode;
									productName = productsRecordIndex[makerModelMappingRecord.keyProductID].name;
								}
								
								//output details of the maker model mapping record
								Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
								Console.WriteLine("Maker Model Mapping Record #: " + recordNumber);
								Console.WriteLine("          Key Maker Model ID: " + makerModelMappingRecord.keyMakerModelID);
								Console.WriteLine("                  Model Code: " + modelCode);
								Console.WriteLine("                  Model Name: " + modelName);
								Console.WriteLine("             Key Category ID: " + makerModelMappingRecord.keyCategoryID);
								Console.WriteLine("               Category Code: " + categoryCode);
								Console.WriteLine("               Category Name: " + categoryName);
								Console.WriteLine("              Key Product ID: " + makerModelMappingRecord.keyProductID);
								Console.WriteLine("                Product Code: " + productCode);
								Console.WriteLine("                Product Name: " + productName);
								Console.WriteLine("                    Quantity: " + makerModelMappingRecord.quantity);
								
								//check if the mapping contains any attribute values
								if(makerModelMappingRecord.attributes.Count > 0)
								{
									//output attributes
									Console.WriteLine("           Attributes Values: " + makerModelMappingRecord.attributes.Count);

									//output each attribute value
									int attributValueCount = 0;
									foreach (ESDRecordAttributeValue attributeValueRecord in makerModelMappingRecord.attributes)
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
											
											// get the mapping's attribute value based on its data type
											if(attributeDataType.ToUpper() ==ESDRecordAttribute.DATA_TYPE_NUMBER){
												attributeValue = attributeValueRecord.numberValue.ToString();
											}else{
												attributeValue = attributeValueRecord.stringValue;
											}
											
											//out attribute value data
											Console.WriteLine("");
											Console.WriteLine("           Attribute Value #: " + attributValueCount);
											Console.WriteLine("    Key Attribute Profile ID: " + attributeValueRecord.keyAttributeProfileID);
											Console.WriteLine("      Attribute Profile Name: " + attributeProfileName);
											Console.WriteLine("            Key Attribute ID: " + attributeValueRecord.keyAttributeID);
											Console.WriteLine("              Attribute Name: " + attributeName);
											Console.WriteLine("             Attribute Value: " + attributeValue);
										}
									}
								}

								Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
							}
							
							//check if there are more records to retrieve
							if(esDocumentMakerModelMapping.dataRecords.Length >= recordsMaxAmount){
								recordsStartIndex = recordsStartIndex + recordsMaxAmount;
								getMoreRecords = true;
							}
						}
					}else{
						Console.WriteLine("FAIL - not all organisation maker model mapping data could be obtained from the platform. Reason: " + endpointResponseESD.result_message  + " Error Code: " + endpointResponseESD.result_code);
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