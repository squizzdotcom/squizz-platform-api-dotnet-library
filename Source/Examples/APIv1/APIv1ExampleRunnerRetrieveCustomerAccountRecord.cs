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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then retrieve the details of a record from a conencted organisation's customer account in the platform</summary>
    public class APIv1ExampleRunnerRetrieveCustomerAccountRecord
    {
        public static DateTime EPOCH_DATE_TIME = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static string RECORD_DATE_FORMAT = "dd-MM-yyyy";
        public static string RECORD_CURRENCY_FORMAT = "F2";

        public static void runAPIv1ExampleRunnerRetrieveCustomerAccountRecord()
        {
            Console.WriteLine("Example - Retrieve Supplier Organisation Customer Account Record");
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
            Console.WriteLine("(Optioanal) Enter Supplier's Customer Account Code:");
            string customerAccountCode = Console.ReadLine();
            Console.WriteLine("Enter number for type of record to retrieve:");
            Console.WriteLine("1 - Invoice");
            Console.WriteLine("2 - Sales Order");
            Console.WriteLine("3 - Back Order");
            Console.WriteLine("4 - Credit");
            Console.WriteLine("5 - Payment");

            //validate the type of record to obtain
            int recordTypeNumber = 1;
            try
            {
                recordTypeNumber = Convert.ToInt32(Console.ReadLine());
            }
            catch { }

            string recordType = ESDocumentConstants.RECORD_TYPE_INVOICE;
            switch (recordTypeNumber)
            {
                case 2:
                    recordType = ESDocumentConstants.RECORD_TYPE_ORDER_SALE;
                    break;
                case 3:
                    recordType = ESDocumentConstants.RECORD_TYPE_BACKORDER;
                    break;
                case 4:
                    recordType = ESDocumentConstants.RECORD_TYPE_CREDIT;
                    break;
                case 5:
                    recordType = ESDocumentConstants.RECORD_TYPE_PAYMENT;
                    break;
            }

            Console.WriteLine("Enter the Key Record ID (Unique ID) of the record:");
            string keyRecordID = Console.ReadLine();

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

            //retrieve record data if the API was successfully created
            if (apiOrgSession.doesSessionExist())
            {
                //after 60 seconds give up on waiting for a response from the API when creating the notification
                int timeoutMilliseconds = 60000;

                //call the platform's API to search for customer account records from the supplier organiation's connected business system
                APIv1EndpointResponseESD<ESDocumentCustomerAccountEnquiry> endpointResponseESD = APIv1EndpointOrgRetrieveCustomerAccountRecord.call(
                    apiOrgSession,
                    timeoutMilliseconds,
                    recordType,
                    supplierOrgID,
                    customerAccountCode,
                    keyRecordID
                );

                ESDocumentCustomerAccountEnquiry esDocumentCustomerAccountEnquiry = endpointResponseESD.esDocument;

                //check that the record detail was successfully obtained
                if (endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                {
                    //output records based on the record type
                    if (recordType == ESDocumentConstants.RECORD_TYPE_INVOICE)
                    {
                        Console.WriteLine("SUCCESS - account invoice record data successfully obtained from the platform");
                        Console.WriteLine("\nInvoice Records Returned: " + esDocumentCustomerAccountEnquiry.totalDataRecords);

                        //check that invoice records have been placed into the standards document
                        if (esDocumentCustomerAccountEnquiry.invoiceRecords != null)
                        {
                            Console.WriteLine("SUCCESS - account invoice record data successfully obtained from the platform");
                            Console.WriteLine("\nInvoice Records Returned: " + esDocumentCustomerAccountEnquiry.totalDataRecords);

                            //check that invoice record has been placed into the standards document
                            if (esDocumentCustomerAccountEnquiry.invoiceRecords != null)
                            {
                                Console.WriteLine("Invoice Records:");

                                //display the details of the record stored within the standards document
                                foreach (ESDRecordCustomerAccountEnquiryInvoice invoiceRecord in esDocumentCustomerAccountEnquiry.invoiceRecords)
                                {
                                    //output details of the invoice record
                                    Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                    Console.WriteLine("       Key Invoice ID: " + invoiceRecord.keyInvoiceID);
                                    Console.WriteLine("           Invoice ID: " + invoiceRecord.invoiceID);
                                    Console.WriteLine("       Invoice Number: " + invoiceRecord.invoiceNumber);
                                    Console.WriteLine("         Invoice Date: " + EPOCH_DATE_TIME.AddMilliseconds(invoiceRecord.invoiceDate).ToString(RECORD_DATE_FORMAT));
                                    Console.WriteLine("Total Price (Inc Tax): " + invoiceRecord.totalIncTax.ToString(RECORD_CURRENCY_FORMAT) + " " + invoiceRecord.currencyCode);
                                    Console.WriteLine("           Total Paid: " + invoiceRecord.totalPaid.ToString(RECORD_CURRENCY_FORMAT) + " " + invoiceRecord.currencyCode);
                                    Console.WriteLine("           Total Owed: " + invoiceRecord.balance.ToString(RECORD_CURRENCY_FORMAT) + " " + invoiceRecord.currencyCode);
                                    Console.WriteLine("          Description: " + invoiceRecord.description);
                                    Console.WriteLine("              Comment: " + invoiceRecord.comment);
                                    Console.WriteLine("     Reference Number: " + invoiceRecord.referenceNumber);
                                    Console.WriteLine("       Reference Type: " + invoiceRecord.referenceType);
                                    Console.WriteLine("");
                                    Console.WriteLine("     Delivery Address: ");
                                    Console.WriteLine("    Organisation Name: " + invoiceRecord.deliveryOrgName);
                                    Console.WriteLine("              Contact: " + invoiceRecord.deliveryContact);
                                    Console.WriteLine("            Address 1: " + invoiceRecord.deliveryAddress1);
                                    Console.WriteLine("            Address 2: " + invoiceRecord.deliveryAddress2);
                                    Console.WriteLine("            Address 3: " + invoiceRecord.deliveryAddress3);
                                    Console.WriteLine("State/Province/Region: " + invoiceRecord.deliveryStateProvince);
                                    Console.WriteLine("              Country: " + invoiceRecord.deliveryCountry);
                                    Console.WriteLine("     Postcode/Zipcode: " + invoiceRecord.deliveryPostcode);
                                    Console.WriteLine("");
                                    Console.WriteLine("      Billing Address: ");
                                    Console.WriteLine("    Organisation Name: " + invoiceRecord.billingOrgName);
                                    Console.WriteLine("              Contact: " + invoiceRecord.billingContact);
                                    Console.WriteLine("            Address 1: " + invoiceRecord.billingAddress1);
                                    Console.WriteLine("            Address 2: " + invoiceRecord.billingAddress2);
                                    Console.WriteLine("            Address 3: " + invoiceRecord.billingAddress3);
                                    Console.WriteLine("State/Province/Region: " + invoiceRecord.billingStateProvince);
                                    Console.WriteLine("              Country: " + invoiceRecord.billingCountry);
                                    Console.WriteLine("     Postcode/Zipcode: " + invoiceRecord.billingPostcode);
                                    Console.WriteLine("");
                                    Console.WriteLine("      Freight Details: ");
                                    Console.WriteLine("     Consignment Code: " + invoiceRecord.freightCarrierConsignCode);
                                    Console.WriteLine("        Tracking Code: " + invoiceRecord.freightCarrierTrackingCode);
                                    Console.WriteLine("         Carrier Name: " + invoiceRecord.freightCarrierName);
                                    Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);

                                    //output the details of each line
                                    if (invoiceRecord.lines != null)
                                    {
                                        Console.WriteLine("                Lines: ");
                                        int i = 0;
                                        foreach (ESDRecordCustomerAccountEnquiryInvoiceLine invoiceLineRecord in invoiceRecord.lines)
                                        {
                                            i++;
                                            Console.WriteLine("          Line Number: " + i);

                                            if (invoiceLineRecord.lineType.ToUpper() == ESDocumentConstants.RECORD_LINE_TYPE_ITEM)
                                            {
                                                Console.WriteLine("             Line Type: ITEM");
                                                Console.WriteLine("          Line Item ID: " + invoiceLineRecord.lineItemID);
                                                Console.WriteLine("        Line Item Code: " + invoiceLineRecord.lineItemCode);
                                                Console.WriteLine("           Description: " + invoiceLineRecord.description);
                                                Console.WriteLine("      Quantity Ordered: " + invoiceLineRecord.quantityOrdered + " " + invoiceLineRecord.unit);
                                                Console.WriteLine("    Quantity Delivered: " + invoiceLineRecord.quantityDelivered + " " + invoiceLineRecord.unit);
                                                Console.WriteLine(" Quantity Back Ordered: " + invoiceLineRecord.quantityBackordered + " " + invoiceLineRecord.unit);
                                                Console.WriteLine("   Unit Price (Ex Tax): " + invoiceLineRecord.priceExTax.ToString(RECORD_CURRENCY_FORMAT) + " " + invoiceLineRecord.currencyCode);
                                                Console.WriteLine("  Total Price (Ex Tax): " + invoiceLineRecord.totalPriceExTax.ToString(RECORD_CURRENCY_FORMAT) + " " + invoiceLineRecord.currencyCode);
                                                Console.WriteLine("             Total Tax: " + invoiceLineRecord.totalPriceTax.ToString(RECORD_CURRENCY_FORMAT) + " Inclusive of " + invoiceLineRecord.taxCode + " " + invoiceLineRecord.taxCodeRatePercent + "%");
                                                Console.WriteLine(" Total Price (Inc Tax): " + invoiceLineRecord.totalPriceIncTax.ToString(RECORD_CURRENCY_FORMAT) + " " + invoiceLineRecord.currencyCode);
                                            }
                                            else if (invoiceLineRecord.lineType.ToUpper() == ESDocumentConstants.RECORD_LINE_TYPE_TEXT)
                                            {
                                                Console.WriteLine("            Line Type: TEXT");
                                                Console.WriteLine("          Description: " + invoiceLineRecord.description);
                                            }

                                            Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    else if (recordType == ESDocumentConstants.RECORD_TYPE_ORDER_SALE)
                    {
                        Console.WriteLine("SUCCESS - account sales order record data successfully obtained from the platform");
                        Console.WriteLine("Sales Order Records Returned: " + esDocumentCustomerAccountEnquiry.totalDataRecords);

                        //check that sales order records have been placed into the standards document
                        if (esDocumentCustomerAccountEnquiry.orderSaleRecords != null)
                        {
                            Console.WriteLine("Sales Order Records:");

                            //display the details of the record stored within the standards document
                            foreach (ESDRecordCustomerAccountEnquiryOrderSale salesOrderRecord in esDocumentCustomerAccountEnquiry.orderSaleRecords)
                            {
                                //output details of the sales order record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("    Key Order Sale ID: " + salesOrderRecord.keyOrderSaleID);
                                Console.WriteLine("             Order ID: " + salesOrderRecord.orderID);
                                Console.WriteLine("         Order Number: " + salesOrderRecord.orderNumber);
                                Console.WriteLine("           Order Date: " + EPOCH_DATE_TIME.AddMilliseconds(salesOrderRecord.orderDate).ToString(RECORD_DATE_FORMAT));
                                Console.WriteLine("Total Price (Inc Tax): " + salesOrderRecord.totalIncTax.ToString(RECORD_CURRENCY_FORMAT) + " " + salesOrderRecord.currencyCode);
                                Console.WriteLine("           Total Paid: " + salesOrderRecord.totalPaid.ToString(RECORD_CURRENCY_FORMAT) + " " + salesOrderRecord.currencyCode);
                                Console.WriteLine("           Total Owed: " + salesOrderRecord.balance.ToString(RECORD_CURRENCY_FORMAT) + " " + salesOrderRecord.currencyCode);
                                Console.WriteLine("          Description: " + salesOrderRecord.description);
                                Console.WriteLine("              Comment: " + salesOrderRecord.comment);
                                Console.WriteLine("     Reference Number: " + salesOrderRecord.referenceNumber);
                                Console.WriteLine("       Reference Type: " + salesOrderRecord.referenceType);
                                Console.WriteLine("");
                                Console.WriteLine("     Delivery Address: ");
                                Console.WriteLine("    Organisation Name: " + salesOrderRecord.deliveryOrgName);
                                Console.WriteLine("              Contact: " + salesOrderRecord.deliveryContact);
                                Console.WriteLine("            Address 1: " + salesOrderRecord.deliveryAddress1);
                                Console.WriteLine("            Address 2: " + salesOrderRecord.deliveryAddress2);
                                Console.WriteLine("            Address 3: " + salesOrderRecord.deliveryAddress3);
                                Console.WriteLine("State/Province/Region: " + salesOrderRecord.deliveryStateProvince);
                                Console.WriteLine("              Country: " + salesOrderRecord.deliveryCountry);
                                Console.WriteLine("     Postcode/Zipcode: " + salesOrderRecord.deliveryPostcode);
                                Console.WriteLine("");
                                Console.WriteLine("      Billing Address: ");
                                Console.WriteLine("    Organisation Name: " + salesOrderRecord.billingOrgName);
                                Console.WriteLine("              Contact: " + salesOrderRecord.billingContact);
                                Console.WriteLine("            Address 1: " + salesOrderRecord.billingAddress1);
                                Console.WriteLine("            Address 2: " + salesOrderRecord.billingAddress2);
                                Console.WriteLine("            Address 3: " + salesOrderRecord.billingAddress3);
                                Console.WriteLine("State/Province/Region: " + salesOrderRecord.billingStateProvince);
                                Console.WriteLine("              Country: " + salesOrderRecord.billingCountry);
                                Console.WriteLine("     Postcode/Zipcode: " + salesOrderRecord.billingPostcode);
                                Console.WriteLine("");
                                Console.WriteLine("      Freight Details: ");
                                Console.WriteLine("     Consignment Code: " + salesOrderRecord.freightCarrierConsignCode);
                                Console.WriteLine("        Tracking Code: " + salesOrderRecord.freightCarrierTrackingCode);
                                Console.WriteLine("         Carrier Name: " + salesOrderRecord.freightCarrierName);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);

                                //output the details of each line
                                if (salesOrderRecord.lines != null)
                                {
                                    Console.WriteLine("                Lines: ");
                                    int i = 0;
                                    foreach (ESDRecordCustomerAccountEnquiryOrderSaleLine orderLineRecord in salesOrderRecord.lines)
                                    {
                                        i++;
                                        Console.WriteLine("          Line Number: " + i);

                                        if (orderLineRecord.lineType.ToUpper() == ESDocumentConstants.RECORD_LINE_TYPE_ITEM)
                                        {
                                            Console.WriteLine("             Line Type: ITEM");
                                            Console.WriteLine("          Line Item ID: " + orderLineRecord.lineItemID);
                                            Console.WriteLine("        Line Item Code: " + orderLineRecord.lineItemCode);
                                            Console.WriteLine("           Description: " + orderLineRecord.description);
                                            Console.WriteLine("      Quantity Ordered: " + orderLineRecord.quantityOrdered + " " + orderLineRecord.unit);
                                            Console.WriteLine("    Quantity Delivered: " + orderLineRecord.quantityDelivered + " " + orderLineRecord.unit);
                                            Console.WriteLine(" Quantity Back Ordered: " + orderLineRecord.quantityBackordered + " " + orderLineRecord.unit);
                                            Console.WriteLine("   Unit Price (Ex Tax): " + orderLineRecord.priceExTax.ToString(RECORD_CURRENCY_FORMAT) + " " + orderLineRecord.currencyCode);
                                            Console.WriteLine("  Total Price (Ex Tax): " + orderLineRecord.totalPriceExTax.ToString(RECORD_CURRENCY_FORMAT) + " " + orderLineRecord.currencyCode);
                                            Console.WriteLine("             Total Tax: " + orderLineRecord.totalPriceTax.ToString(RECORD_CURRENCY_FORMAT) + " Inclusive of " + orderLineRecord.taxCode + " " + orderLineRecord.taxCodeRatePercent + "%");
                                            Console.WriteLine(" Total Price (Inc Tax): " + orderLineRecord.totalPriceIncTax.ToString(RECORD_CURRENCY_FORMAT) + " " + orderLineRecord.currencyCode);
                                        }
                                        else if (orderLineRecord.lineType.ToUpper() == ESDocumentConstants.RECORD_LINE_TYPE_TEXT)
                                        {
                                            Console.WriteLine("            Line Type: TEXT");
                                            Console.WriteLine("          Description: " + orderLineRecord.description);
                                        }

                                        Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (recordType == ESDocumentConstants.RECORD_TYPE_BACKORDER)
                    {
                        Console.WriteLine("SUCCESS - account back order record data successfully obtained from the platform");
                        Console.WriteLine("Back Order Records Returned: " + esDocumentCustomerAccountEnquiry.totalDataRecords);

                        //check that back order records have been placed into the standards document
                        if (esDocumentCustomerAccountEnquiry.backOrderRecords != null)
                        {
                            Console.WriteLine("Back Order Records:");

                            //display the details of the record stored within the standards document
                            foreach (ESDRecordCustomerAccountEnquiryBackOrder backOrderRecord in esDocumentCustomerAccountEnquiry.backOrderRecords)
                            {
                                //output details of the back order record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("    Key Back Order ID: " + backOrderRecord.keyBackOrderID);
                                Console.WriteLine("        Back Order ID: " + backOrderRecord.backOrderID);
                                Console.WriteLine("    Back Order Number: " + backOrderRecord.backOrderNumber);
                                Console.WriteLine("           Order Date: " + EPOCH_DATE_TIME.AddMilliseconds(backOrderRecord.backOrderDate).ToString(RECORD_DATE_FORMAT));
                                Console.WriteLine("Total Price (Inc Tax): " + backOrderRecord.totalIncTax.ToString(RECORD_CURRENCY_FORMAT) + " " + backOrderRecord.currencyCode);
                                Console.WriteLine("           Total Paid: " + backOrderRecord.totalPaid.ToString(RECORD_CURRENCY_FORMAT) + " " + backOrderRecord.currencyCode);
                                Console.WriteLine("           Total Owed: " + backOrderRecord.balance.ToString(RECORD_CURRENCY_FORMAT) + " " + backOrderRecord.currencyCode);
                                Console.WriteLine("          Description: " + backOrderRecord.description);
                                Console.WriteLine("              Comment: " + backOrderRecord.comment);
                                Console.WriteLine("     Reference Number: " + backOrderRecord.referenceNumber);
                                Console.WriteLine("       Reference Type: " + backOrderRecord.referenceType);
                                Console.WriteLine("");
                                Console.WriteLine("     Delivery Address: ");
                                Console.WriteLine("    Organisation Name: " + backOrderRecord.deliveryOrgName);
                                Console.WriteLine("              Contact: " + backOrderRecord.deliveryContact);
                                Console.WriteLine("            Address 1: " + backOrderRecord.deliveryAddress1);
                                Console.WriteLine("            Address 2: " + backOrderRecord.deliveryAddress2);
                                Console.WriteLine("            Address 3: " + backOrderRecord.deliveryAddress3);
                                Console.WriteLine("State/Province/Region: " + backOrderRecord.deliveryStateProvince);
                                Console.WriteLine("              Country: " + backOrderRecord.deliveryCountry);
                                Console.WriteLine("     Postcode/Zipcode: " + backOrderRecord.deliveryPostcode);
                                Console.WriteLine("");
                                Console.WriteLine("      Billing Address: ");
                                Console.WriteLine("    Organisation Name: " + backOrderRecord.billingOrgName);
                                Console.WriteLine("              Contact: " + backOrderRecord.billingContact);
                                Console.WriteLine("            Address 1: " + backOrderRecord.billingAddress1);
                                Console.WriteLine("            Address 2: " + backOrderRecord.billingAddress2);
                                Console.WriteLine("            Address 3: " + backOrderRecord.billingAddress3);
                                Console.WriteLine("State/Province/Region: " + backOrderRecord.billingStateProvince);
                                Console.WriteLine("              Country: " + backOrderRecord.billingCountry);
                                Console.WriteLine("     Postcode/Zipcode: " + backOrderRecord.billingPostcode);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);

                                //output the details of each line
                                if (backOrderRecord.lines != null)
                                {
                                    Console.WriteLine("                Lines: ");
                                    int i = 0;
                                    foreach (ESDRecordCustomerAccountEnquiryBackOrderLine orderLineRecord in backOrderRecord.lines)
                                    {
                                        i++;
                                        Console.WriteLine("          Line Number: " + i);

                                        if (orderLineRecord.lineType.ToUpper() == ESDocumentConstants.RECORD_LINE_TYPE_ITEM)
                                        {
                                            Console.WriteLine("             Line Type: ITEM");
                                            Console.WriteLine("          Line Item ID: " + orderLineRecord.lineItemID);
                                            Console.WriteLine("        Line Item Code: " + orderLineRecord.lineItemCode);
                                            Console.WriteLine("           Description: " + orderLineRecord.description);
                                            Console.WriteLine("      Quantity Ordered: " + orderLineRecord.quantityOrdered + " " + orderLineRecord.unit);
                                            Console.WriteLine("    Quantity Delivered: " + orderLineRecord.quantityDelivered + " " + orderLineRecord.unit);
                                            Console.WriteLine(" Quantity Back Ordered: " + orderLineRecord.quantityBackordered + " " + orderLineRecord.unit);
                                            Console.WriteLine("   Unit Price (Ex Tax): " + orderLineRecord.priceExTax.ToString(RECORD_CURRENCY_FORMAT) + " " + orderLineRecord.currencyCode);
                                            Console.WriteLine("  Total Price (Ex Tax): " + orderLineRecord.totalPriceExTax.ToString(RECORD_CURRENCY_FORMAT) + " " + orderLineRecord.currencyCode);
                                            Console.WriteLine("             Total Tax: " + orderLineRecord.totalPriceTax.ToString(RECORD_CURRENCY_FORMAT) + " Inclusive of " + orderLineRecord.taxCode + " " + orderLineRecord.taxCodeRatePercent + "%");
                                            Console.WriteLine(" Total Price (Inc Tax): " + orderLineRecord.totalPriceIncTax.ToString(RECORD_CURRENCY_FORMAT) + " " + orderLineRecord.currencyCode);
                                        }
                                        else if (orderLineRecord.lineType.ToUpper() == ESDocumentConstants.RECORD_LINE_TYPE_TEXT)
                                        {
                                            Console.WriteLine("            Line Type: TEXT");
                                            Console.WriteLine("          Description: " + orderLineRecord.description);
                                        }

                                        Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (recordType == ESDocumentConstants.RECORD_TYPE_CREDIT)
                    {
                        Console.WriteLine("SUCCESS - account credit record data successfully obtained from the platform");
                        Console.WriteLine("Credit Records Returned: " + esDocumentCustomerAccountEnquiry.totalDataRecords);

                        //check that credit records have been placed into the standards document
                        if (esDocumentCustomerAccountEnquiry.creditRecords != null)
                        {
                            Console.WriteLine("Credit Records:");

                            //display the details of the record stored within the standards document
                            foreach (ESDRecordCustomerAccountEnquiryCredit creditRecord in esDocumentCustomerAccountEnquiry.creditRecords)
                            {
                                //output details of the credit record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("        Key Credit ID: " + creditRecord.keyCreditID);
                                Console.WriteLine("            Credit ID: " + creditRecord.creditID);
                                Console.WriteLine("        Credit Number: " + creditRecord.creditNumber);
                                Console.WriteLine("          Credit Date: " + EPOCH_DATE_TIME.AddMilliseconds(creditRecord.creditDate).ToString(RECORD_DATE_FORMAT));
                                Console.WriteLine("      Amount Credited: " + creditRecord.appliedAmount.ToString(RECORD_CURRENCY_FORMAT) + " " + creditRecord.currencyCode);
                                Console.WriteLine("          Description: " + creditRecord.description);
                                Console.WriteLine("              Comment: " + creditRecord.comment);
                                Console.WriteLine("     Reference Number: " + creditRecord.referenceNumber);
                                Console.WriteLine("       Reference Type: " + creditRecord.referenceType);
                                Console.WriteLine("");
                                Console.WriteLine("     Delivery Address: ");
                                Console.WriteLine("    Organisation Name: " + creditRecord.deliveryOrgName);
                                Console.WriteLine("              Contact: " + creditRecord.deliveryContact);
                                Console.WriteLine("            Address 1: " + creditRecord.deliveryAddress1);
                                Console.WriteLine("            Address 2: " + creditRecord.deliveryAddress2);
                                Console.WriteLine("            Address 3: " + creditRecord.deliveryAddress3);
                                Console.WriteLine("State/Province/Region: " + creditRecord.deliveryStateProvince);
                                Console.WriteLine("              Country: " + creditRecord.deliveryCountry);
                                Console.WriteLine("     Postcode/Zipcode: " + creditRecord.deliveryPostcode);
                                Console.WriteLine("");
                                Console.WriteLine("      Billing Address: ");
                                Console.WriteLine("    Organisation Name: " + creditRecord.billingOrgName);
                                Console.WriteLine("              Contact: " + creditRecord.billingContact);
                                Console.WriteLine("            Address 1: " + creditRecord.billingAddress1);
                                Console.WriteLine("            Address 2: " + creditRecord.billingAddress2);
                                Console.WriteLine("            Address 3: " + creditRecord.billingAddress3);
                                Console.WriteLine("State/Province/Region: " + creditRecord.billingStateProvince);
                                Console.WriteLine("              Country: " + creditRecord.billingCountry);
                                Console.WriteLine("     Postcode/Zipcode: " + creditRecord.billingPostcode);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);

                                //output the details of each line
                                if (creditRecord.lines != null)
                                {
                                    Console.WriteLine("                Lines: ");
                                    int i = 0;
                                    foreach (ESDRecordCustomerAccountEnquiryCreditLine creditLineRecord in creditRecord.lines)
                                    {
                                        i++;
                                        Console.WriteLine("          Line Number: " + i);

                                        if (creditLineRecord.lineType.ToUpper() == ESDocumentConstants.RECORD_LINE_TYPE_ITEM)
                                        {
                                            Console.WriteLine("             Line Type: ITEM");
                                            Console.WriteLine("          Line Item ID: " + creditLineRecord.lineItemID);
                                            Console.WriteLine("        Line Item Code: " + creditLineRecord.lineItemCode);
                                            Console.WriteLine("           Description: " + creditLineRecord.description);
                                            Console.WriteLine("      Reference Number: " + creditLineRecord.referenceNumber);
                                            Console.WriteLine("        Reference Type: " + creditLineRecord.referenceType);
                                            Console.WriteLine("      Reference Key ID: " + creditLineRecord.referenceKeyID);
                                            Console.WriteLine(" Total Price (Inc Tax): " + creditLineRecord.totalPriceIncTax.ToString(RECORD_CURRENCY_FORMAT) + " " + creditLineRecord.currencyCode);
                                        }
                                        else if (creditLineRecord.lineType.ToUpper() == ESDocumentConstants.RECORD_LINE_TYPE_TEXT)
                                        {
                                            Console.WriteLine("            Line Type: TEXT");
                                            Console.WriteLine("          Description: " + creditLineRecord.description);
                                        }

                                        Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (recordType == ESDocumentConstants.RECORD_TYPE_PAYMENT)
                    {
                        Console.WriteLine("SUCCESS - account payment record data successfully obtained from the platform");
                        Console.WriteLine("Payment Records Returned: " + esDocumentCustomerAccountEnquiry.totalDataRecords);

                        //check that payment records have been placed into the standards document
                        if (esDocumentCustomerAccountEnquiry.paymentRecords != null)
                        {
                            Console.WriteLine("Payment Records:");

                            //display the details of the record stored within the standards document
                            foreach (ESDRecordCustomerAccountEnquiryPayment paymentRecord in esDocumentCustomerAccountEnquiry.paymentRecords)
                            {
                                //output details of the payment record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("       Key Payment ID: " + paymentRecord.keyPaymentID);
                                Console.WriteLine("           Payment ID: " + paymentRecord.paymentID);
                                Console.WriteLine("       Payment Number: " + paymentRecord.paymentNumber);
                                Console.WriteLine("         Payment Date: " + EPOCH_DATE_TIME.AddMilliseconds(paymentRecord.paymentDate).ToString(RECORD_DATE_FORMAT));
                                Console.WriteLine("    Total Amount Paid: " + paymentRecord.totalAmount.ToString(RECORD_CURRENCY_FORMAT) + " " + paymentRecord.currencyCode);
                                Console.WriteLine("          Description: " + paymentRecord.description);
                                Console.WriteLine("              Comment: " + paymentRecord.comment);
                                Console.WriteLine("     Reference Number: " + paymentRecord.referenceNumber);
                                Console.WriteLine("       Reference Type: " + paymentRecord.referenceType);
                                Console.WriteLine("     Reference Key ID: " + paymentRecord.referenceKeyID);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);

                                //output the details of each line
                                if (paymentRecord.lines != null)
                                {
                                    Console.WriteLine("                Lines: ");
                                    int i = 0;
                                    foreach (ESDRecordCustomerAccountEnquiryPaymentLine paymentLineRecord in paymentRecord.lines)
                                    {
                                        i++;
                                        Console.WriteLine("          Line Number: " + i);

                                        if (paymentLineRecord.lineType.ToUpper() == ESDocumentConstants.RECORD_LINE_TYPE_ITEM)
                                        {
                                            Console.WriteLine("             Line Type: ITEM");
                                            Console.WriteLine("          Line Item ID: " + paymentLineRecord.lineItemID);
                                            Console.WriteLine("        Line Item Code: " + paymentLineRecord.lineItemCode);
                                            Console.WriteLine("           Description: " + paymentLineRecord.description);
                                            Console.WriteLine("      Reference Number: " + paymentLineRecord.referenceNumber);
                                            Console.WriteLine("        Reference Type: " + paymentLineRecord.referenceType);
                                            Console.WriteLine("      Reference Key ID: " + paymentLineRecord.referenceKeyID);
                                            Console.WriteLine("        Payment Amount: " + paymentLineRecord.amount.ToString(RECORD_CURRENCY_FORMAT) + " " + paymentLineRecord.currencyCode);
                                        }
                                        else if (paymentLineRecord.lineType.ToUpper() == ESDocumentConstants.RECORD_LINE_TYPE_TEXT)
                                        {
                                            Console.WriteLine("            Line Type: TEXT");
                                            Console.WriteLine("          Description: " + paymentLineRecord.description);
                                        }

                                        Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
                else {
                    Console.WriteLine("FAIL - record data failed to be obtained from the platform. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);
                }

                //next steps
                //call other API endpoints...
                //destroy API session when done...
                apiOrgSession.destroyOrgSession();
            }
        }
    }
}
