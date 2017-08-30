![alt tag](https://www.squizz.com/ui/resources/images/logos/squizz_logo_mdpi.png)

# SQUIZZ.com Platform API .NET Library

The [SQUIZZ.com](https://www.squizz.com) Platform API .NET Library can be used by .NET applications to access the SQUIZZ.com platform's Application Programming Interface (API), allowing data to be pushed and pulled from the API's endpoints in a clean and elegant way. The kinds of data pushed and pulled from the API using the library can include organisational data such as products, sales orders, purchase orders, customer accounts, supplier accounts, notifications, and other data that the platform supports.

This library removes the need for .NET software developers to write boilerplate code for connecting and accessing the platform's API, allowing .NET software using the platform's API to be writen faster and simpler. The library provides classes and objects that can be directly referenced within a .NET application, making it easy to manipulate data retreived from the platform, or create and send data to platform.

If you are a software developer writing a .NET application then we recommend that you use this library instead of directly calling the platform's APIs, since it will simplify your development times and allow you to easily incorporate new functionality from the API by simplying updating this library.

- You can find more information about the SQUIZZ.com platform by visiting [https://www.squizz.com/docs/squizz](https://www.squizz.com/docs/squizz)
- To find more information about developing software for the SQUIZZ.com visit [https://www.squizz.com/docs/squizz/Integrate-Software-Into-SQUIZZ.com-Platform.html](https://www.squizz.com/docs/squizz/Integrate-Software-Into-SQUIZZ.com-Platform.html)
- To find more information about the platform's API visit [https://www.squizz.com/docs/squizz/Platform-API.html](https://www.squizz.com/docs/squizz/Platform-API.html)

## Contents

  * [Getting Started](#getting-started)
  * [Example Usages](#example-usages)
    * [Create Organisation API Session Endpoint](#create-organisation-api-session-endpoint)
	* [Send and Procure Purchase Order From Supplier Endpoint](#send-and-procure-purchase-order-from-supplier-endpoint)
    * [Retrieve Organisation Data Endpoint](#retrieve-organisation-data-endpoint)
		* [Retrieve Organisation Product Data Example](#retrieve-organisation-product-data-example)
		* [Retrieve Organisation Pricing Data Example](#retrieve-organisation-pricing-data-example)
		* [Retrieve Organisation Stock Availability Example](#retrieve-organisation-stock-availability-example)
	* [Import Organisation Data Endpoint](#import-organisation-data-endpoint)
    * [Create Organisation Notification Endpoint](#create-organisation-notification-endpoint)
    * [Validate Organisation API Session Endpoint](#validate-organisation-api-session-endpoint)
    * [Validate/Create Organisation API Session Endpoint](#validatecreate-organisation-api-session-endpoint)
    * [Destroy Organisation API Session Endpoint](#destroy-organisation-api-session-endpoint)

## Getting Started

To get started using the library within .NET applications, you can download the API library and its dependent libraries into your Visual Studio solution from [NuGET](https://www.nuget.org/) package manager. The library is hosted at [NuGet Squizz.Platform.API](https://www.nuget.org/packages/Squizz.Platform.API/) package. You can install the NuGET hosted package with the command line below, or visually find and install the package using [Visual Studio NuGET Package Manager](https://marketplace.visualstudio.com/items?itemName=NuGetTeam.NuGetPackageManager) plugin.
```
Install-Package Squizz.Platform.API -Version 1.0.0
```
Alternatively you can download and add the required files direct from the [Release page](https://github.com/squizzdotcom/squizz-platform-api-dotnet-library/releases). Once done then add references to the DLL files in your visual studio solution.
The library contains dependencies on [Newtonsoft's Json.NET Library](https://www.newtonsoft.com/json) as well as the [Ecommerce Standards Documents .NET Library](https://github.com/squizzdotcom/ecommerce-standards-documents-dotnet-library)
Once this library is referenced within your .NET application then to use it within a .NET class you can use the following "using" syntax:

```
using Squizz.Platform.API.v1;
using Squizz.Platform.API.v1.endpoint;
```

## Example Usages
## Create Organisation API Session Endpoint
To start using the SQUIZZ.com platform's API a session must first be created. A session can only be created after credentials for a specified organisation have been given to the API and have been verified.
Once the session has been created then all other endpoints in the API can be called.
Read [https://www.squizz.com/docs/squizz/Platform-API.html#section840](https://www.squizz.com/docs/squizz/Platform-API.html#section840) for more documentation about the endpoint.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squizz.Platform.API.v1;
using Squizz.Platform.API.v1.endpoint;

namespace Squizz.Platform.API.Examples.APIv1
{
    /// <summary>Class runs a console application that shows an example on how create a session in the platform's API</summary>
    public class APIv1ExampleRunnerCreateSession
    {
        public static void runAPIv1ExampleRunnerCreateSession()
        {
            Console.WriteLine("Example - Creating An Organisation API Session");
            Console.WriteLine("");

            //obtain or load in an organisation's API credentials, in this example from the user in the console
            Console.WriteLine("Enter Organisation ID:");
            string orgID = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Key:");
            string orgAPIKey = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Password:");
            string orgAPIPass = Console.ReadLine();
            
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

            //next steps
            //call API endpoints...
            //destroy API session when done...

            Console.WriteLine("Example Finished.");
        }
    }
}
```

## Send and Procure Purchase Order From Supplier Endpoint

The SQUIZZ.com platform's API has an endpoint that allows an orgnisation to import a purchase order. and have it procured/converted into a sales order of a designated supplier organisation. 
This endpoint allows a customer organisation to commit to buy goods and services of an organisation, and have the order processed, and delivered by the supplier organisation.
The endpoint relies upon a connection first being setup between organisations within the SQUIZZ.com platform.
The endpoint relies upon being able to find matching supplier products as with what has been ordered.
The endpoint has a number of other requirements. See the endpoint documentation for more details on these requirements.

Each purchase order needs to be imported within a "Ecommerce Standards Document" that contains a record for each purchase order. Use the Ecommerce Standards library to easily create these documents and records.
It is recommended to only import one purchase order at a time, since if an array of purchase orders is imported and one order failed to be procured, then no other orders in the list will be attempted to import.
Read [https://www.squizz.com/docs/squizz/Platform-API.html#section961](https://www.squizz.com/docs/squizz/Platform-API.html#section961) for more documentation about the endpoint and its requirements.
See the example below on how the call the Send and Procure Purchase order From Supplier endpoint. Note that a session must first be created in the API before calling the endpoint.

![alt tag](https://attach.squizz.com/doc_centre/1/files/images/masters/SQUIZZ-Customer-Purchase-Order-Procurement-Supplier[124].png)

```csharp
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
     * Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then sends a organisation's purchase order data to supplier
     */
    public class APIv1ExampleRunnerProcurePurchaseOrderFromSupplier
    {
        public static void runAPIv1ExampleRunnerProcurePurchaseOrderFromSupplier()
        {
            Console.WriteLine("Example - Procure Purchase Order From Supplier API Session");
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

            //sand and procure purchsae order if the API was successfully created
            if (apiOrgSession.doesSessionExist())
            {
                //create purchase order record to import
                ESDRecordOrderPurchase purchaseOrderRecord = new ESDRecordOrderPurchase();

                //set data within the purchase order
                purchaseOrderRecord.keyPurchaseOrderID = "111";
                purchaseOrderRecord.purchaseOrderCode = "POEXAMPLE-345";
                purchaseOrderRecord.purchaseOrderNumber = "345";
                purchaseOrderRecord.instructions = "Leave goods at the back entrance";
                purchaseOrderRecord.keySupplierAccountID = "2";
                purchaseOrderRecord.supplierAccountCode = "ACM-002";

                //set delivery address that ordered goods will be delivered to
                purchaseOrderRecord.deliveryAddress1 = "32";
                purchaseOrderRecord.deliveryAddress2 = "Main Street";
                purchaseOrderRecord.deliveryAddress3 = "Melbourne";
                purchaseOrderRecord.deliveryRegionName = "Victoria";
                purchaseOrderRecord.deliveryCountryName = "Australia";
                purchaseOrderRecord.deliveryPostcode = "3000";
                purchaseOrderRecord.deliveryOrgName = "Acme Industries";
                purchaseOrderRecord.deliveryContact = "Jane Doe";

                //set billing address that the order will be billed to for payment
                purchaseOrderRecord.billingAddress1 = "43";
                purchaseOrderRecord.billingAddress2 = " High Street";
                purchaseOrderRecord.billingAddress3 = "Melbourne";
                purchaseOrderRecord.billingRegionName = "Victoria";
                purchaseOrderRecord.billingCountryName = "Australia";
                purchaseOrderRecord.billingPostcode = "3000";
                purchaseOrderRecord.billingOrgName = "Acme Industries International";
                purchaseOrderRecord.billingContact = "John Citizen";

                //create an array of purchase order lines
                List<ESDRecordOrderPurchaseLine> orderLines = new List<ESDRecordOrderPurchaseLine>();

                //create purchase order line record 1
                ESDRecordOrderPurchaseLine orderProduct = new ESDRecordOrderPurchaseLine();
                orderProduct.lineType = ESDocumentConstants.ORDER_LINE_TYPE_PRODUCT;
                orderProduct.productCode = "TEA-TOWEL-GREEN";
                orderProduct.productName = "Green tea towel - 30 x 6 centimetres";
                orderProduct.keySellUnitID = "2";
                orderProduct.unitName = "EACH";
                orderProduct.quantity = 4;
                orderProduct.sellUnitBaseQuantity = 4;
                orderProduct.priceExTax = (decimal)5.00;
                orderProduct.priceIncTax = (decimal)5.50;
                orderProduct.priceTax = (decimal)0.50;
                orderProduct.priceTotalIncTax = (decimal)22.00;
                orderProduct.priceTotalExTax = (decimal)20.00;
                orderProduct.priceTotalTax = (decimal)2.00;
                //specify supplier's product code in salesOrderProductCode if it is different to the customer line's productCode field
                orderProduct.salesOrderProductCode = "ACME-TTGREEN";

                //add 1st order line to the order lines list
                orderLines.Add(orderProduct);

                //create purchase order line record 2
                orderProduct = new ESDRecordOrderPurchaseLine();
                orderProduct.lineType = ESDocumentConstants.ORDER_LINE_TYPE_PRODUCT;
                orderProduct.productCode = "TEA-TOWEL-BLUE";
                orderProduct.productName = "Blue tea towel - 20 x 6 centimetres";
                orderProduct.keySellUnitID = "1";
                orderProduct.unitName = "EACH";
                orderProduct.quantity = 10;
                orderProduct.sellUnitBaseQuantity = 10;
                orderProduct.salesOrderProductCode = "ACME-TTBLUE";

                //add 2nd order line to the order lines list
                orderLines.Add(orderProduct);

                //add order lines to the order
                purchaseOrderRecord.lines = orderLines;

                //create purchase order records list and add purchase order to it
                List<ESDRecordOrderPurchase> purchaseOrderRecords = new List<ESDRecordOrderPurchase>();
                purchaseOrderRecords.Add(purchaseOrderRecord);

                //after 60 seconds give up on waiting for a response from the API when sending the order
                int timeoutMilliseconds = 60000;

                //create purchase order Ecommerce Standards document and add purchse order records to the document
                ESDocumentOrderPurchase orderPurchaseESD = new ESDocumentOrderPurchase(ESDocumentConstants.RESULT_SUCCESS, "successfully obtained data", purchaseOrderRecords.ToArray(), new Dictionary<string, string>());

                //send purchase order document to the API for procurement by the supplier organisation
                APIv1EndpointResponseESD<ESDocumentOrderSale> endpointResponseESD = APIv1EndpointOrgProcurePurchaseOrderFromSupplier.call(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", orderPurchaseESD);
                ESDocumentOrderSale esDocumentOrderSale = endpointResponseESD.esDocument;

                //check the result of procuring the purchase orders
                if (endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS) {
                    Console.WriteLine("SUCCESS - organisation purchase orders have successfully been sent to supplier organisation.");

                    //iterate through each of the returned supplier's sales orders and output the details of the sales orders
                    if (esDocumentOrderSale.dataRecords != null) {
                        foreach(ESDRecordOrderSale salesOrderRecord in esDocumentOrderSale.dataRecords) {
                            Console.WriteLine("\nSales Order Returned, Order Details: ");
                            Console.WriteLine("Sales Order Code: " + salesOrderRecord.salesOrderCode);
                            Console.WriteLine("Sales Order Total Cost: " + salesOrderRecord.totalPriceIncTax + " (" + salesOrderRecord.currencyISOCode + ")");
                            Console.WriteLine("Sales Order Total Taxes: " + salesOrderRecord.totalTax + " (" + salesOrderRecord.currencyISOCode + ")");
                            Console.WriteLine("Sales Order Customer Account: " + salesOrderRecord.customerAccountCode);
                            Console.WriteLine("Sales Order Total Lines: " + salesOrderRecord.totalLines);
                        }
                    }
                } else {
                    Console.WriteLine("FAIL - organisation purchase orders failed to be processed. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);

                    //if one or more products in the purchase order could not match a product for the supplier organisation then find out the order lines caused the problem
                    if (endpointResponseESD.result_code.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_ORDER_PRODUCT_NOT_MAPPED && esDocumentOrderSale != null)
                    {
                        //get a list of order lines that could not be mapped
                        List<KeyValuePair<int, int>> unmappedLines = APIv1EndpointOrgProcurePurchaseOrderFromSupplier.getUnmappedOrderLines(esDocumentOrderSale);

                        //iterate through each unmapped order line
                        foreach(KeyValuePair<int, int> unmappedLine in unmappedLines){
                            //get the index of the purchase order and line that contained the unmapped product
                            int orderIndex = unmappedLine.Key;
                            int lineIndex = unmappedLine.Value;

                            //check that the order can be found that contains the problematic line
                            if (orderIndex < orderPurchaseESD.dataRecords.Length && lineIndex < orderPurchaseESD.dataRecords[orderIndex].lines.Count) {
                                Console.WriteLine("For purchase order: " + orderPurchaseESD.dataRecords[orderIndex].purchaseOrderCode + " a matching supplier product for line number: " + (lineIndex + 1) + " could not be found.");
                            }
                        }
                    }
                    //if one or more products in the purchase order could not be priced by the supplier organisation then find the order line that caused the problem
                    else if (endpointResponseESD.result_code.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_ORDER_MAPPED_PRODUCT_PRICE_NOT_FOUND && esDocumentOrderSale != null)
                    {
                        if (esDocumentOrderSale.configs.ContainsKey(APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNPRICED_LINES))
                        {
                            //get a list of order lines that could not be priced
                            List<KeyValuePair<int, int>> unmappedLines = APIv1EndpointOrgProcurePurchaseOrderFromSupplier.getUnpricedOrderLines(esDocumentOrderSale);

                            //iterate through each unpriced order line
                            foreach(KeyValuePair<int, int> unmappedLine in unmappedLines){
                                //get the index of the purchase order and line that contained the unpriced product
                                int orderIndex = unmappedLine.Key;
                                int lineIndex = unmappedLine.Value;

                                //check that the order can be found that contains the problematic line
                                if (orderIndex < orderPurchaseESD.dataRecords.Length && lineIndex < orderPurchaseESD.dataRecords[orderIndex].lines.Count) {
                                    Console.WriteLine("For purchase order: " + orderPurchaseESD.dataRecords[orderIndex].purchaseOrderCode + " the supplier has not set pricing for line number: " + (lineIndex + 1));
                                }
                            }
                        }
                    }
                }
            }

            //next steps
            //call other API endpoints...
            //destroy API session when done...

            Console.WriteLine("Example Finished.");
        }
    }
}
```

## Retrieve Organisation Data Endpoint
The SQUIZZ.com platform's API has an endpoint that allows a variety of different types of data to be retrieved from another organisation stored on the platform.
The organisational data that can be retrieved includes products, product stock quantities, and product pricing.
The data retrieved can be used to allow an organisation to set additional information about products being bought or sold, as well as being used in many other ways.
Each kind of data retrieved from endpoint is formatted as JSON data conforming to the "Ecommerce Standards Document" standards, with each document containing an array of zero or more records. Use the Ecommerce Standards library to easily read through these documents and records, to find data natively using .NET classes.
Read [https://www.squizz.com/docs/squizz/Platform-API.html#section969](https://www.squizz.com/docs/squizz/Platform-API.html#section969) for more documentation about the endpoint and its requirements.
See examples below on how the call the Retrieve Organisation ESD Data endpoint. Note that a session must first be created in the API before calling the endpoint.

### Retrieve Organisation Product Data Example
```csharp
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
    public class APIv1ExampleRunnerRetrieveOrgESDDataProduct
    {
        public static void runAPIv1ExampleRunnerRetrieveOrgESDDataProduct()
        {
            Console.WriteLine("Example - Retrieve Supplier Organisation Product Data");
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

            //import organisation data if the API was successfully created
            if (apiOrgSession.doesSessionExist())
            {
                //after 60 seconds give up on waiting for a response from the API when creating the notification
                int timeoutMilliseconds = 60000;

                //loop through retrieving pages of records from the API
                bool hasMoreRecordsToRetrieve = true;
                int recordStartIndex = 0;
                while(hasMoreRecordsToRetrieve)
                {
                    //call the platform's API to get the supplier organisation's product data
                    APIv1EndpointResponseESD<ESDocumentProduct> endpointResponseESD = APIv1EndpointOrgRetrieveESDocumentProduct.call(apiOrgSession, timeoutMilliseconds, supplierOrgID, recordStartIndex, APIv1EndpointOrgRetrieveESDocumentProduct.MAX_RECORDS_PER_REQUEST);
                    ESDocumentProduct esDocumentProduct = (ESDocumentProduct)endpointResponseESD.esDocument;

                    //check that the data successfully imported
                    if (endpointResponseESD.result.ToUpper()==APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                    {
                        Console.WriteLine("SUCCESS - organisation data successfully obtained from the platform");
                        Console.WriteLine("\nProduct Records Returned: " + esDocumentProduct.totalDataRecords);

                        //check that records have been placed into the standards document
                        if (esDocumentProduct.dataRecords != null) {
                            Console.WriteLine("Product Records:");

                            //iterate through each product record stored within the standards document
                            int i = 0;
                            foreach(ESDRecordProduct productRecord in esDocumentProduct.dataRecords)
                            {
                                //output details of the product record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("  Product Record #: " + i);
                                Console.WriteLine("  Key Product ID: " + productRecord.keyProductID);
                                Console.WriteLine("    Product Code: " + productRecord.productCode);
                                Console.WriteLine("            Name: " + productRecord.name);
                                Console.WriteLine("         Barcode: " + productRecord.barcode);
                                Console.WriteLine(" Stock Available: " + productRecord.stockQuantity);
                                Console.WriteLine("           Brand: " + productRecord.brand);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);

                                i++;
                            }
                        }

                        //check to see if a full page of records were retrieved and if there is more records to get
                        if (esDocumentProduct.totalDataRecords >= APIv1EndpointOrgRetrieveESDocumentProduct.MAX_RECORDS_PER_REQUEST) {
                            recordStartIndex += APIv1EndpointOrgRetrieveESDocumentProduct.MAX_RECORDS_PER_REQUEST;
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
```

### Retrieve Organisation Pricing Data Example
```csharp
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

            //import organisation data if the API was successfully created
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
                    APIv1EndpointResponseESD<ESDocumentPrice> endpointResponseESD = APIv1EndpointOrgRetrieveESDocumentPrice.call(apiOrgSession, timeoutMilliseconds, supplierOrgID, customerAccountCode, recordStartIndex, APIv1EndpointOrgRetrieveESDocumentPrice.MAX_RECORDS_PER_REQUEST);
                    ESDocumentPrice esDocumentPrice = (ESDocumentPrice)endpointResponseESD.esDocument;

                    //check that the data successfully imported
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
                        if (esDocumentPrice.totalDataRecords >= APIv1EndpointOrgRetrieveESDocumentPrice.MAX_RECORDS_PER_REQUEST) {
                            recordStartIndex += APIv1EndpointOrgRetrieveESDocumentPrice.MAX_RECORDS_PER_REQUEST;
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
```

### Retrieve Organisation Stock Availability Example
```csharp
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
    public class APIv1ExampleRunnerRetrieveOrgESDDataProductStock
    {
        public static void runAPIv1ExampleRunnerRetrieveOrgESDDataProductStock()
        {
            Console.WriteLine("Example - Retrieve Supplier Organisation Product Stock Data");
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

            //import organisation data if the API was successfully created
            if (apiOrgSession.doesSessionExist())
            {
                //after 60 seconds give up on waiting for a response from the API when creating the notification
                int timeoutMilliseconds = 60000;

                //loop through retrieving pages of records from the API
                bool hasMoreRecordsToRetrieve = true;
                int recordStartIndex = 0;
                while(hasMoreRecordsToRetrieve)
                {
                    //call the platform's API to get the supplier organisation's product stock data
                    APIv1EndpointResponseESD<ESDocumentStockQuantity> endpointResponseESD = APIv1EndpointOrgRetrieveESDocumentProductStock.call(apiOrgSession, timeoutMilliseconds, supplierOrgID, recordStartIndex, APIv1EndpointOrgRetrieveESDocumentProduct.MAX_RECORDS_PER_REQUEST);
                    ESDocumentStockQuantity esDocumentStockQuantity = (ESDocumentStockQuantity)endpointResponseESD.esDocument;

                    //check that the data successfully imported
                    if (endpointResponseESD.result.ToUpper()==APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                    {
                        Console.WriteLine("SUCCESS - organisation data successfully obtained from the platform");
                        Console.WriteLine("Stock Records Returned: " + esDocumentStockQuantity.totalDataRecords);

                        //check that records have been placed into the standards document
                        if (esDocumentStockQuantity.dataRecords != null) {
                            Console.WriteLine("Stock Quantity Records:");

                            //iterate through each stock quantity record stored within the standards document
                            int i = 0;
                            foreach(ESDRecordStockQuantity stockRecord in esDocumentStockQuantity.dataRecords)
                            {
                                //output details of the stock quantity record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("  Stock Record #: " + i);
                                Console.WriteLine("  Key Product ID: " + stockRecord.keyProductID);
                                Console.WriteLine(" Stock Available: " + stockRecord.qtyAvailable);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);

                                i++;
                            }
                        }

                        //check to see if a full page of records were retrieved and if there is more records to get
                        if (esDocumentStockQuantity.totalDataRecords >= APIv1EndpointOrgRetrieveESDocumentProductStock.MAX_RECORDS_PER_REQUEST) {
                            recordStartIndex += APIv1EndpointOrgRetrieveESDocumentProductStock.MAX_RECORDS_PER_REQUEST;
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
```

## Import Organisation Data Endpoint
The SQUIZZ.com platform's API has an endpoint that allows a wide variety of different types of data to be imported into the platform against an organisation. 
This organisational data includes taxcodes, products, customer accounts, supplier accounts. pricing, price levels, locations, and many other kinds of data.
This data is used to allow the organisation to buy and sell products, as well manage customers, suppliers, employees, and other people.
Each type of data needs to be imported as an "Ecommerce Standards Document" that contains one or more records. Use the Ecommerce Standards library to easily create these documents and records.
When importing one type of organisational data, it is important to import the full data set, otherwise the platform will deactivate un-imported data.
For example if 3 products are imported, then a another products import is run that only imports 2 records, then 1 product will become deactivated and no longer be able to be sold.
Read [https://www.squizz.com/docs/squizz/Platform-API.html#section843](https://www.squizz.com/docs/squizz/Platform-API.html#section843) for more documentation about the endpoint and its requirements.
See the example below on how the call the Import Organisation ESD Data endpoint. Note that a session must first be created in the API before calling the endpoint.

```csharp
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
    public class APIv1ExampleRunnerImportOrgESDData
    {
        public static void runAPIv1ExampleRunnerImportOrgESDData()
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
```

## Create Organisation Notification Endpoint
The SQUIZZ.com platform's API has an endpoint that allows organisation notifications to be created in the platform. allowing people assigned to an organisation's notification category to receive a notification. 
This can be used to advise such people of events happening external to the platform, such as sales, enquires, tasks completed through websites and other software.
See the example below on how the call the Create Organisation Notification endpoint. Note that a session must first be created in the API before calling the endpoint.
Read [https://www.squizz.com/docs/squizz/Platform-API.html#section854](https://www.squizz.com/docs/squizz/Platform-API.html#section854) for more documentation about the endpoint.

```csharp
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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then creating an organisation notification</summary>
    public class APIv1ExampleRunnerCreateOrgNotification
    {
        public static void runAPIv1ExampleRunnerCreateOrgNotification()
        {
            Console.WriteLine("Example - Create Organisation Notification");
            Console.WriteLine("");

            //obtain or load in an organisation's API credentials, in this example from the user in the console
            Console.WriteLine("Enter Organisation ID:");
            string orgID = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Key:");
            string orgAPIKey = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Password:");
            string orgAPIPass = Console.ReadLine();

            //obtain or load in an organisation's API credentials, in this example from command line arguments
            int sessionTimeoutMilliseconds = 20000;

            //create an API session instance
            APIv1OrgSession apiOrgSession = new APIv1OrgSession(orgID, orgAPIKey, orgAPIPass, sessionTimeoutMilliseconds, APIv1Constants.SUPPORTED_LOCALES_EN_AU);

            //call the platform's API to request that a session is created
            APIv1EndpointResponse endpointResponse = apiOrgSession.createOrgSession();

            //check if the organisation's credentials were correct and that a session was created in the platform's API
            if (endpointResponse.result.ToUpper() == (APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS))
            {
                //session has been created so now can call other API endpoints
                Console.WriteLine("SUCCESS - API session has successfully been created.");
            }
            else
            {
                //session failed to be created
                Console.WriteLine("FAIL - API session failed to be created. Reason: " + endpointResponse.result_message + " Error Code: " + endpointResponse.result_code);
            }

            //create organisation notification if the API was successfully created
            if (apiOrgSession.doesSessionExist())
            {
                //set the notification category that the organisation will display under in the platform, in this case the sales order category
                String notifyCategory = APIv1EndpointOrgCreateNotification.NOTIFY_CATEGORY_ORDER_SALE;

                //after 20 seconds give up on waiting for a response from the API when creating the notification
                int timeoutMilliseconds = 20000;

                //set the message that will appear in the notification, note the placeholders {1} and {2} that will be replaced with data values
                String message = "A new {1} was created in {2} Website";

                //set labels and links to place within the placeholders of the message
                String[] linkLabels = new String[] { "Sales Order", "Acme Industries" };
                String[] linkURLs = new String[] { "", "http://www.example.com/acmeindustries"};

                //call the platform's API to create the organistion notification and have people assigned to organisation's notification category receive it
                APIv1EndpointResponseESD<ESDocument> endpointResponseESD = APIv1EndpointOrgCreateNotification.call(apiOrgSession, timeoutMilliseconds, notifyCategory, message, linkURLs, linkLabels);

                if (endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS) {
                    Console.WriteLine("SUCCESS - organisation notification successfully created in the platform");
                } else {
                    Console.WriteLine("FAIL - organisation notification failed to be created. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);
                }

                //next steps
                //call other API endpoints...
                //destroy API session when done...
                apiOrgSession.destroyOrgSession();
            }

            Console.WriteLine("Example Finished.");
        }
    }
}
```

## Validate Organisation API Session Endpoint

After a session has been created with SQUIZZ.com platform's API, if the same session is persistently being used over a long period time, then its worth validating that the session has not been destroyed by the API.
The SQUIZZ.com platform's API will automatically expire and destory sessions that have existed for a long period of time.
Read [https://www.squizz.com/docs/squizz/Platform-API.html#section842](https://www.squizz.com/docs/squizz/Platform-API.html#section842) for more documentation about the endpoint.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squizz.Platform.API.v1;
using Squizz.Platform.API.v1.endpoint;

namespace Squizz.Platform.API.Examples.APIv1
{
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then validates if the user's session is still valid</summary>
    public class APIv1ExampleRunnerValidateOrgSession 
    {
        public static void runAPIv1ExampleRunnerValidateOrgSession()
        {
            Console.WriteLine("Example - Validating An Organisation API Session");
            Console.WriteLine("");

            //obtain or load in an organisation's API credentials, in this example from the user in the console
            Console.WriteLine("Enter Organisation ID:");
            string orgID = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Key:");
            string orgAPIKey = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Password:");
            string orgAPIPass = Console.ReadLine();

            //create an API session instance
            int sessionTimeoutMilliseconds = 20000;
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
		
		    //next steps
		    //call API endpoints...
		
		    //check if the session still is valid
		    endpointResponse = apiOrgSession.validateOrgSession();
		
		    //check the result of validating the session
		    if(endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS){
                Console.WriteLine("SUCCESS - API session successfully validated.");
		    }else{
                Console.WriteLine("FAIL - API session failed to be validated. Reason: " + endpointResponse.result_message  + " Error Code: " + endpointResponse.result_code);
		    }

            //destroy API session when finished with it
            apiOrgSession.destroyOrgSession();

            Console.WriteLine("Example Finished.");
        }
    }
}
```

## Validate/Create Organisation API Session Endpoint

After a session has been created with SQUIZZ.com platform's API, if the same session is persistently being used over a long period time, then a helper method in the library can be used to check if the API session is still valid, then if not have a new session be created.
The SQUIZZ.com platform's API will automatically expire and destory sessions that have existed for a long period of time.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squizz.Platform.API.v1;
using Squizz.Platform.API.v1.endpoint;

/// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then validates if the user's session is still valid or creates a new session</summary>
namespace Squizz.Platform.API.Examples.APIv1
{
    public class APIv1ExampleRunnerValidateCreateOrgSession
    {
        public static void runAPIv1ExampleRunnerValidateCreateOrgSession()
        {
            Console.WriteLine("Example - Validating/Creating An Organisation API Session");
            Console.WriteLine("");

            //obtain or load in an organisation's API credentials, in this example from the user in the console
            Console.WriteLine("Enter Organisation ID:");
            string orgID = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Key:");
            string orgAPIKey = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Password:");
            string orgAPIPass = Console.ReadLine();

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

            //next steps
            //call API endpoints...

            //check if the session still is valid, if not have a new session created with the same organisation API credentials
            endpointResponse = apiOrgSession.validateCreateOrgSession();

            //check the result of validating or creating a new session
            if (endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
            {
                Console.WriteLine("SUCCESS - API session successfully validated/created.");
            }
            else {
                Console.WriteLine("FAIL - API session failed to be validated or created. Reason: " + endpointResponse.result_message + " Error Code: " + endpointResponse.result_code);
            }

            //destroy API session when done...
            apiOrgSession.destroyOrgSession();

            Console.WriteLine("Example Finished.");
        }
    }
}
```

## Destroy Organisation API Session Endpoint

After a session has been created with SQUIZZ.com platform's API, if after calling other endpoints there no need for the session anymore, then it's advisable to destroy the session as soon as possible.
Read [https://www.squizz.com/docs/squizz/Platform-API.html#section841](https://www.squizz.com/docs/squizz/Platform-API.html#section841) for more documentation about the endpoint.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squizz.Platform.API.v1;
using Squizz.Platform.API.v1.endpoint;

/// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then destroys the session</summary>
public class APIv1ExampleRunnerDestroyOrgSession 
{
    public static void runAPIv1ExampleRunnerDestroyOrgSession()
    {
        Console.WriteLine("Example - Destroying An Organisation API Session");
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
            Console.WriteLine("FAIL - API session failed to be created. Reason: " + endpointResponse.result_message  + " Error Code: " + endpointResponse.result_code);
		}
		
		//next steps
		//call API endpoints...
		
		//destroy the session in the platform's API
        endpointResponse = apiOrgSession.destroyOrgSession();
		
		//check the result of destroying the session
		if(endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS){
            Console.WriteLine("SUCCESS - API session successfully destroyed.");
		}else{
            Console.WriteLine("FAIL - API session failed to be destroyed. Reason: " + endpointResponse.result_message  + " Error Code: " + endpointResponse.result_code);
		}

        Console.WriteLine("Example Finished.");
    }
}
```
