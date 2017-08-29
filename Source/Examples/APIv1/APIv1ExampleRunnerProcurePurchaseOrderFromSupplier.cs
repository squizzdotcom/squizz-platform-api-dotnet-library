/**
* Copyright (C) 2017 Squizz PTY LTD
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
                //specify supplier's product code in salesOrderProductCode if it is different to the line's productCode field
                orderProduct.salesOrderProductCode = "ACME-TTGREEN";

                //add 1st order line to lines list
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

                //add 2nd order line to lines list
                orderLines.Add(orderProduct);

                //add order lines to the order
                purchaseOrderRecord.lines = orderLines;

                //create purchase order records list and add purchase order to it
                List<ESDRecordOrderPurchase> purchaseOrderRecords = new List<ESDRecordOrderPurchase>();
                purchaseOrderRecords.Add(purchaseOrderRecord);

                //after 60 seconds give up on waiting for a response from the API when creating the notification
                int timeoutMilliseconds = 60000;

                //create purchase order Ecommerce Standards document and add purchse order records to the document
                ESDocumentOrderPurchase orderPurchaseESD = new ESDocumentOrderPurchase(ESDocumentConstants.RESULT_SUCCESS, "successfully obtained data", purchaseOrderRecords.ToArray(), new Dictionary<string, string>());

                //send purchase order document to the API for procurement by the supplier organisation
                APIv1EndpointResponseESD<ESDocumentOrderSale> endpointResponseESD = APIv1EndpointOrgProcurePurchaseOrderFromSupplier.call(apiOrgSession, timeoutMilliseconds, supplierOrgID, "", orderPurchaseESD);
                ESDocumentOrderSale esDocumentOrderSale = endpointResponseESD.esDocument;

                //check the result of procuring the purchase orders
                if (endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS) {
                    Console.WriteLine("SUCCESS - organisation purchase orders have successfully been sent to supplier organisation.");

                    //iterate through each of the returned sales orders and output the details of the sales orders
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

                //next steps
                //call other API endpoints...
                //destroy API session when done...
                apiOrgSession.destroyOrgSession();
            }
            
            Console.WriteLine("Example Finished.");
        }
    }
}