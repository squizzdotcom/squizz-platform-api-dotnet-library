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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then retrieves Category data from a connected organisation in the platform</summary>
    public class APIv1ExampleRunnerRetrieveOrgESDDataCategories
	{
        public static void runAPIv1ExampleRunnerRetrieveOrgESDDataCategories()
        {
            Console.WriteLine("Example - Retrieve Supplier Organisation Category Data");
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
			
			//retrieve product data, since it is used to find details of products assigned to categories
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

                    Console.WriteLine("Attempt made to obtain product data. Page number: "+pageNumber);

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
                            Console.WriteLine("No more records obtained. Page number: "+pageNumber);
						}
						
						result = true;
					}else{
						result = false;
                        Console.WriteLine("FAIL - not all organisation product data could be obtained from the platform. Reason: " + endpointResponseESD.result_message  + " Error Code: " + endpointResponseESD.result_code);
					}
				}
			}
			
			//retrieve category data. Initially index it so that lookups can be done between linked categories, regardless of the order in which category records were obtained
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
					
					//call the platform's API to retrieve the organisation's category data
					APIv1EndpointResponseESD<ESDocumentCategory> endpointResponseESD = APIv1EndpointOrgRetrieveESDocument.callRetrieveCategories(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", recordsMaxAmount, recordsStartIndex, "");

                    Console.WriteLine("Attempt made to obtain category data. Page number: "+pageNumber);

					//check that the data successfully retrieved
					if(endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
					{
                        Console.WriteLine("SUCCESS - maker model mapping data successfully obtained from the platform for page number: "+pageNumber);
						
						ESDocumentCategory esDocumentCategory = (ESDocumentCategory)endpointResponseESD.esDocument;
						
						//check that records have been placed into the standards document
						if(esDocumentCategory.dataRecords != null)
						{
                            Console.WriteLine("Category Records Returned: " + esDocumentCategory.totalDataRecords);
							
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
			
			//output the details of each category
			if(result)
			{
                Console.WriteLine("Outputting category data.");
				
				//iterate through and output each category previously obtained
				foreach(String keyCategoryID in categoriesRecordIndex.Keys)
				{
					ESDRecordCategory categoryRecord = categoriesRecordIndex[keyCategoryID];
					String parentCategoryCode = "";

					//find the parent category that the category may be assigned to
					if(categoryRecord.keyCategoryParentID != null && categoriesRecordIndex.ContainsKey(categoryRecord.keyCategoryParentID)){
						parentCategoryCode = categoriesRecordIndex[categoryRecord.keyCategoryParentID].categoryCode;
					}

					//output details of the category record
					Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
					Console.WriteLine("     Category Record #: " + recordNumber);
					Console.WriteLine("       Key Category ID: " + categoryRecord.keyCategoryID);
					Console.WriteLine("         Category Code: " + categoryRecord.categoryCode);
					Console.WriteLine("         Category Name: " + categoryRecord.name);
					Console.WriteLine("Key Category Parent ID: " + categoryRecord.keyCategoryParentID);
					Console.WriteLine("  Parent Category Code: " + parentCategoryCode);
					Console.WriteLine("          Description1: " + categoryRecord.description1);
					Console.WriteLine("          Description2: " + categoryRecord.description2);
					Console.WriteLine("          Description3: " + categoryRecord.description3);
					Console.WriteLine("          Description4: " + categoryRecord.description4);
					Console.WriteLine("            Meta Title: " + categoryRecord.metaTitle);
					Console.WriteLine("         Meta Keywords: " + categoryRecord.metaKeywords);
					Console.WriteLine("      Meta Description: " + categoryRecord.metaDescription);
					Console.WriteLine("              Ordering: " + categoryRecord.ordering);
					
					//check if the category contains any products
					if(categoryRecord.keyProductIDs != null && categoryRecord.keyProductIDs.Length > 0)
					{
						Console.WriteLine("        Products Count: " + categoryRecord.keyProductIDs.Length);

						//output each product assigned to the category
						int productCount = 0;
						foreach (String keyProductID in categoryRecord.keyProductIDs)
						{
							productCount++;
							
							//check that the product has been previously obtained
							if(productsRecordIndex.ContainsKey(keyProductID))
							{
								ESDRecordProduct productRecord = productsRecordIndex[keyProductID];

								//output details of the product assigned to the category
								Console.WriteLine();
								Console.WriteLine("             Product #: " + productCount);
								Console.WriteLine("        Key Product ID: " + keyProductID);
								Console.WriteLine("          Product Code: " + productRecord.productCode);
								Console.WriteLine("          Product Name: " + productRecord.name);
							}
						}
					}

					recordNumber++;
				}
			}
			
			//next steps
			//call other API endpoints...
			//destroy API session when done
			apiOrgSession.destroyOrgSession();
		}
	}
}