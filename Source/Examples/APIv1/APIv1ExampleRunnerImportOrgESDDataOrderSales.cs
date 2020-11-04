/**
* Copyright (C) Squizz PTY LTD
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
    public class APIv1ExampleRunnerImportOrgESDDataOrderSales
    {
        public static void runAPIv1ExampleRunnerImportOrgESDDataOrderSales()
        {
            Console.WriteLine("Example - Import Organisation's Sales Order");
            Console.WriteLine("");

            //obtain or load in an organisation's API credentials, in this example from the user in the console
            Console.WriteLine("Enter Organisation ID:");
            string orgID = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Key:");
            string orgAPIKey = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Password:");
            string orgAPIPass = Console.ReadLine();
            Console.WriteLine("Enter Reprice Order (set Y, or N):");
            string repriceOrder = Console.ReadLine();

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

            //import sales order if the API session was successfully created
            if (apiOrgSession.doesSessionExist())
            {
                //create sales order record to import
                ESDRecordOrderSale salesOrderRecord = new ESDRecordOrderSale();

                //set data within the sales order
                salesOrderRecord.keySalesOrderID = "111";
                salesOrderRecord.salesOrderCode = "SOEXAMPLE-678";
                salesOrderRecord.salesOrderNumber = "678";
                salesOrderRecord.salesOrderNumber = "678";
                salesOrderRecord.instructions = "Leave goods at the back entrance";
                salesOrderRecord.keyCustomerAccountID = "3";
                salesOrderRecord.customerAccountCode = "CUS-003";
                salesOrderRecord.customerAccountName = "Acme Industries";
                salesOrderRecord.customerEntity = ESDocumentConstants.ENTITY_TYPE_ORG;
                salesOrderRecord.customerOrgName = "Acme Industries Pty Ltd";

                //set delivery address that ordered goods will be delivered to
                salesOrderRecord.deliveryAddress1 = "32";
                salesOrderRecord.deliveryAddress2 = "Main Street";
                salesOrderRecord.deliveryAddress3 = "Melbourne";
                salesOrderRecord.deliveryRegionName = "Victoria";
                salesOrderRecord.deliveryCountryName = "Australia";
                salesOrderRecord.deliveryPostcode = "3000";
                salesOrderRecord.deliveryOrgName = "Acme Industries";
                salesOrderRecord.deliveryContact = "Jane Doe";

                //set billing address that the order will be billed to for payment
                salesOrderRecord.billingAddress1 = "43";
                salesOrderRecord.billingAddress2 = " High Street";
                salesOrderRecord.billingAddress3 = "Melbourne";
                salesOrderRecord.billingRegionName = "Victoria";
                salesOrderRecord.billingCountryName = "Australia";
                salesOrderRecord.billingPostcode = "3000";
                salesOrderRecord.billingOrgName = "Acme Industries International";
                salesOrderRecord.billingContact = "John Citizen";

                //create an array of sales order lines
                List<ESDRecordOrderSaleLine> orderLines = new List<ESDRecordOrderSaleLine>();

                //create sales order line record
                ESDRecordOrderSaleLine orderProduct = new ESDRecordOrderSaleLine();
                orderProduct.lineType = ESDocumentConstants.ORDER_LINE_TYPE_PRODUCT;
                orderProduct.productCode = "TEA-TOWEL-GREEN";
                orderProduct.productName = "Green tea towel - 30 x 6 centimetres";
                orderProduct.unitName = "EACH";
                orderProduct.keySellUnitID = "EA";
                orderProduct.quantity = 4;
                orderProduct.sellUnitBaseQuantity = 4;
                //pricing data only needs to be set if the order isn't being repriced
                orderProduct.priceExTax = (decimal)5.00;
                orderProduct.priceIncTax = (decimal)5.50;
                orderProduct.priceTax = (decimal)0.50;
                orderProduct.priceTotalIncTax = (decimal)22.00;
                orderProduct.priceTotalExTax = (decimal)20.00;
                orderProduct.priceTotalTax = (decimal)2.00;
                orderLines.Add(orderProduct);

                //add taxes to the line
                orderProduct.taxes = new List<ESDRecordOrderLineTax>();
                ESDRecordOrderLineTax orderProductTax = new ESDRecordOrderLineTax();
                orderProductTax.keyTaxcodeID = "TAXCODE-1";
                orderProductTax.taxcode = "GST";
                orderProductTax.taxcodeLabel = "Goods And Services Tax";
                //pricing data only needs to be set if the order isn't being repriced
                orderProductTax.priceTax = (decimal)0.50;
                orderProductTax.taxRate = (decimal)10;
                orderProductTax.quantity = (decimal)4;
                orderProductTax.priceTotalTax = (decimal)2.00;
                orderProduct.taxes.Add(orderProductTax);

                //add a 2nd sales order line record that is a text line
                orderProduct = new ESDRecordOrderSaleLine();
                orderProduct.lineType = ESDocumentConstants.ORDER_LINE_TYPE_TEXT;
                orderProduct.productCode = "TEA-TOWEL-BLUE";
                orderProduct.textDescription = "Please bundle tea towels into a box";
                orderLines.Add(orderProduct);

                //add a 3rd sales order line record
                orderProduct = new ESDRecordOrderSaleLine();
                orderProduct.lineType = ESDocumentConstants.ORDER_LINE_TYPE_PRODUCT;
                orderProduct.productCode = "TEA-TOWEL-BLUE";
                orderProduct.productName = "Blue tea towel - 30 x 6 centimetres";
                orderProduct.unitName = "BOX";
                orderProduct.keySellUnitID = "BOX-OF-10";
                orderProduct.quantity = 2;
                orderProduct.sellUnitBaseQuantity = 20;
                //pricing data only needs to be set if the order isn't being repriced
                orderProduct.priceExTax = (decimal)10.00;
                orderProduct.priceIncTax = (decimal)11.00;
                orderProduct.priceTax = (decimal)1.00;
                orderProduct.priceTotalIncTax = (decimal)22.00;
                orderProduct.priceTotalExTax = (decimal)20.00;
                orderProduct.priceTotalTax = (decimal)2.00;
                orderLines.Add(orderProduct);

                //add taxes to the line
                orderProduct.taxes = new List<ESDRecordOrderLineTax>();
                orderProductTax = new ESDRecordOrderLineTax();
                orderProductTax.keyTaxcodeID = "TAXCODE-1";
                orderProductTax.taxcode = "GST";
                orderProductTax.taxcodeLabel = "Goods And Services Tax";
                orderProductTax.quantity = 2;
                //pricing data only needs to be set if the order isn't being repriced
                orderProductTax.priceTax = (decimal)1.00;
                orderProductTax.taxRate = (decimal)10;
                orderProductTax.priceTotalTax = (decimal)2.00;
                orderProduct.taxes.Add(orderProductTax);

                //add order lines to the order
                salesOrderRecord.lines = orderLines;

                //create an array of sales order surcharges
                List<ESDRecordOrderSurcharge> orderSurcharges = new List<ESDRecordOrderSurcharge>();

                //create sales order surcharge record
                ESDRecordOrderSurcharge orderSurcharge = new ESDRecordOrderSurcharge();
                orderSurcharge.surchargeCode = "FREIGHT-FEE";
                orderSurcharge.surchargeLabel = "Freight Surcharge";
                orderSurcharge.surchargeDescription = "Cost of freight delivery";
                orderSurcharge.keySurchargeID = "SURCHARGE-1";
                //pricing data only needs to be set if the order isn't being repriced
                orderSurcharge.priceExTax = (decimal)3.00;
                orderSurcharge.priceIncTax = (decimal)3.30;
                orderSurcharge.priceTax = (decimal)0.30;
                orderSurcharges.Add(orderSurcharge);

                //add taxes to the surcharge
                orderSurcharge.taxes = new List<ESDRecordOrderLineTax>();
                ESDRecordOrderLineTax orderSurchargeTax = new ESDRecordOrderLineTax();
                orderSurchargeTax.keyTaxcodeID = "TAXCODE-1";
                orderSurchargeTax.taxcode = "GST";
                orderSurchargeTax.taxcodeLabel = "Goods And Services Tax";
                orderSurchargeTax.quantity = 1;
                //pricing data only needs to be set if the order isn't being repriced
                orderSurchargeTax.priceTax = (decimal)0.30;
                orderSurchargeTax.taxRate = (decimal)10;
                orderSurchargeTax.priceTotalTax = (decimal)0.30;
                orderSurcharge.taxes.Add(orderSurchargeTax);

                //create 2nd sales order surcharge record
                orderSurcharge = new ESDRecordOrderSurcharge();
                orderSurcharge.surchargeCode = "PAYMENT-FEE";
                orderSurcharge.surchargeLabel = "Credit Card Surcharge";
                orderSurcharge.surchargeDescription = "Cost of Credit Card Payment";
                orderSurcharge.keySurchargeID = "SURCHARGE-2";
                //pricing data only needs to be set if the order isn't being repriced
                orderSurcharge.priceExTax = (decimal)5.00;
                orderSurcharge.priceIncTax = (decimal)5.50;
                orderSurcharge.priceTax = (decimal)0.50;
                orderSurcharges.Add(orderSurcharge);

                //add taxes to the 2nd surcharge
                orderSurcharge.taxes = new List<ESDRecordOrderLineTax>();
                orderSurchargeTax = new ESDRecordOrderLineTax();
                orderSurchargeTax.keyTaxcodeID = "TAXCODE-1";
                orderSurchargeTax.taxcode = "GST";
                orderSurchargeTax.taxcodeLabel = "Goods And Services Tax";
                //pricing data only needs to be set if the order isn't being repriced
                orderSurchargeTax.priceTax = (decimal)0.50;
                orderSurchargeTax.taxRate = (decimal)10;
                orderSurchargeTax.quantity = (decimal)1;
                orderSurchargeTax.priceTotalTax = (decimal)5.00;
                orderSurcharge.taxes.Add(orderSurchargeTax);

                //add surcharges to the order
                salesOrderRecord.surcharges = orderSurcharges;

                //create an array of sales order payments
                List<ESDRecordOrderPayment> orderPayments = new List<ESDRecordOrderPayment>();

                //create sales order payment record
                ESDRecordOrderPayment orderPayment = new ESDRecordOrderPayment();
                orderPayment.paymentMethod = ESDocumentConstants.PAYMENT_METHOD_CREDIT;
                orderPayment.paymentProprietaryCode = "Freight Surcharge";
                orderPayment.paymentReceipt = "3422ads2342233";
                orderPayment.keyPaymentTypeID = "VISA-CREDIT-CARD";
                orderPayment.paymentAmount = (decimal)22.80;
                orderPayments.Add(orderPayment);

                //create 2nd sales order payment record
                orderPayment = new ESDRecordOrderPayment();
                orderPayment.paymentMethod = ESDocumentConstants.PAYMENT_METHOD_PROPRIETARY;
                orderPayment.paymentProprietaryCode = "PAYPAL";
                orderPayment.paymentReceipt = "2323432341231";
                orderPayment.keyPaymentTypeID = "PP";
                orderPayment.paymentAmount = (decimal)30.00;
                orderPayments.Add(orderPayment);

                //add payments to the order and set overall payment details
                salesOrderRecord.payments = orderPayments;
                salesOrderRecord.paymentAmount = (decimal)41.00;
                salesOrderRecord.paymentStatus = ESDocumentConstants.PAYMENT_STATUS_PAID;

                //set order totals, pricing data only needs to be set if the order isn't being repriced
                salesOrderRecord.totalPriceIncTax = (decimal)52.80;
                salesOrderRecord.totalPriceExTax = (decimal)48.00;
                salesOrderRecord.totalTax = (decimal)4.80;
                salesOrderRecord.totalSurchargeExTax = (decimal)8.00;
                salesOrderRecord.totalSurchargeIncTax = (decimal)8.80;
                salesOrderRecord.totalSurchargeTax = (decimal)8.00;

                //create sales order records list and add the sales order record to it
                List<ESDRecordOrderSale> salesOrderRecords = new List<ESDRecordOrderSale>();
                salesOrderRecords.Add(salesOrderRecord);

                //after 120 seconds give up on waiting for a response from the API when importing a sales order
                int timeoutMilliseconds = 120000;

                //create purchase order Ecommerce Standards document and add purchse order records to the document
                ESDocumentOrderSale orderSaleESD = new ESDocumentOrderSale(ESDocumentConstants.RESULT_SUCCESS, "successfully obtained data", salesOrderRecords.ToArray(), new Dictionary<string, string>());

                //send purchase order document to the API for procurement by the supplier organisation
                APIv1EndpointResponseESD<ESDocumentOrderSale> endpointResponseESD = APIv1EndpointOrgImportSalesOrder.call(apiOrgSession, timeoutMilliseconds, orderSaleESD, repriceOrder.ToUpper() == ESDocumentConstants.ESD_VALUE_YES);
                ESDocumentOrderSale esDocumentOrderSale = endpointResponseESD.esDocument;

                //check the result of procuring the purchase orders
                if (endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                {
                    Console.WriteLine("SUCCESS - organisation purchase orders have successfully been sent to supplier organisation.");

                    //iterate through each of the returned sales orders and output the details of the sales orders
                    if (esDocumentOrderSale.dataRecords != null)
                    {
                        foreach (ESDRecordOrderSale salesOrderRecordResponse in esDocumentOrderSale.dataRecords)
                        {
                            Console.WriteLine("\nSales Order Returned, Order Details: ");
                            Console.WriteLine("Sales Order Code: " + salesOrderRecordResponse.salesOrderCode);
                            Console.WriteLine("Sales Order Total Cost: " + salesOrderRecordResponse.totalPriceIncTax + " (" + salesOrderRecordResponse.currencyISOCode + ")");
                            Console.WriteLine("Sales Order Total Taxes: " + salesOrderRecordResponse.totalTax + " (" + salesOrderRecordResponse.currencyISOCode + ")");
                            Console.WriteLine("Sales Order Customer Account: " + salesOrderRecordResponse.customerAccountCode);
                            Console.WriteLine("Sales Order Total Lines: " + salesOrderRecordResponse.totalLines);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("FAIL - organisation purchase orders failed to be processed. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);


                    //check if the server response returned back a Ecommerce Standards Document
                    if (esDocumentOrderSale != null)
                    {
                        switch (endpointResponseESD.result_code)
                        {
                            //if one or more products in the sales order could not match the organisation's products then find out the order lines caused the problem
                            case APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_ORDER_PRODUCT_NOT_MATCHED:
                                //get a list of order lines that could not be mapped
                                List<KeyValuePair<int, int>> unmatchedLines = APIv1EndpointOrgImportSalesOrder.getUnmatchedOrderLines(esDocumentOrderSale);

                                //iterate through each unmatched order line
                                foreach (KeyValuePair<int, int> unmatchedLine in unmatchedLines)
                                {
                                    //get the index of the sales order and line that contained the unmatched product
                                    int orderIndex = unmatchedLine.Key;
                                    int lineIndex = unmatchedLine.Value;

                                    //check that the order can be found that contains the problematic line
                                    if (orderIndex < orderSaleESD.dataRecords.Length && lineIndex < orderSaleESD.dataRecords[orderIndex].lines.Count)
                                    {
                                        Console.WriteLine("For sales order: " + orderSaleESD.dataRecords[orderIndex].salesOrderCode + " a matching product for line number: " + (lineIndex + 1) + " could not be found.");
                                    }
                                }

                                break;

                            //if one or more products in the sales order could not be priced then find the order line that caused the problem
                            case APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_ORDER_LINE_PRICING_MISSING:
                                //get a list of order lines that could not be priced
                                List<KeyValuePair<int, int>> unpricedLines = APIv1EndpointOrgImportSalesOrder.getUnpricedOrderLines(esDocumentOrderSale);

                                //iterate through each unpriced order line
                                foreach (KeyValuePair<int, int> unmappedLine in unpricedLines)
                                {
                                    //get the index of the sales order and line that contained the unpriced product
                                    int orderIndex = unmappedLine.Key;
                                    int lineIndex = unmappedLine.Value;

                                    //check that the order can be found that contains the problematic line
                                    if (orderIndex < orderSaleESD.dataRecords.Length && lineIndex < orderSaleESD.dataRecords[orderIndex].lines.Count)
                                    {
                                        Console.WriteLine("For sales order: " + orderSaleESD.dataRecords[orderIndex].salesOrderCode + " has not set pricing for line number: " + (lineIndex + 1));
                                    }
                                }

                                break;

                            case APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_ORDER_SURCHARGE_NOT_FOUND:
                                //get a list of order surcharges that could not be matched
                                List<KeyValuePair<int, int>> unmappedSurcharges = APIv1EndpointOrgImportSalesOrder.getUnmatchedOrderSurcharges(esDocumentOrderSale);

                                //iterate through each unmatched order surcharge
                                foreach (KeyValuePair<int, int> unmappedSurcharge in unmappedSurcharges)
                                {
                                    //get the index of the sales order and surcharge that contained the unmapped surcharge
                                    int orderIndex = unmappedSurcharge.Key;
                                    int surchargeIndex = unmappedSurcharge.Value;

                                    //check that the order can be found that contains the problematic surcharge
                                    if (orderIndex < orderSaleESD.dataRecords.Length && surchargeIndex < orderSaleESD.dataRecords[orderIndex].surcharges.Count)
                                    {
                                        Console.WriteLine("For sales order: " + orderSaleESD.dataRecords[orderIndex].salesOrderCode + " a matching surcharge for surcharge number: " + (surchargeIndex + 1) + " could not be found.");
                                    }
                                }

                                break;

                            //if one or more surcharges in the sales order could not be priced then find the order surcharge that caused the problem
                            case APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_ORDER_SURCHARGE_PRICING_MISSING:

                                //get a list of order surcharges that could not be priced
                                List<KeyValuePair<int, int>> unpricedSurcharges = APIv1EndpointOrgImportSalesOrder.getUnpricedOrderSurcharges(esDocumentOrderSale);

                                //iterate through each unpriced order surcharge
                                foreach (KeyValuePair<int, int> unmappedSurcharge in unpricedSurcharges)
                                {
                                    //get the index of the purchase order and surcharge that contained the unpriced surcharge
                                    int orderIndex = unmappedSurcharge.Key;
                                    int surchargeIndex = unmappedSurcharge.Value;

                                    //check that the order can be found that contains the problematic surcharge
                                    if (orderIndex < orderSaleESD.dataRecords.Length && surchargeIndex < orderSaleESD.dataRecords[orderIndex].surcharges.Count)
                                    {
                                        Console.WriteLine("For sales order: " + orderSaleESD.dataRecords[orderIndex].salesOrderCode + " has not set pricing for surcharge number: " + (surchargeIndex + 1));
                                    }
                                }

                                break;

                            case APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_ORDER_PAYMENT_NOT_MATCHED:
                                //get a list of order payments that could not be mapped
                                List<KeyValuePair<int, int>> unmappedPayments = APIv1EndpointOrgImportSalesOrder.getUnmatchedOrderPayments(esDocumentOrderSale);

                                //iterate through each unmapped order payment
                                foreach (KeyValuePair<int, int> unmappedPayment in unmappedPayments)
                                {
                                    //get the index of the purchase order and payment that contained the unmapped payment
                                    int orderIndex = unmappedPayment.Key;
                                    int paymentIndex = unmappedPayment.Value;

                                    //check that the order can be found that contains the problematic payment
                                    if (orderIndex < orderSaleESD.dataRecords.Length && paymentIndex < orderSaleESD.dataRecords[orderIndex].payments.Count)
                                    {
                                        Console.WriteLine("For sales order: " + orderSaleESD.dataRecords[orderIndex].salesOrderCode + " a matching payment for payment number: " + (paymentIndex + 1) + " could not be found.");
                                    }
                                }

                                break;
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