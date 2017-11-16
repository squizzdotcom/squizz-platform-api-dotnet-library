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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then search for records from a conencted organisation's customer account in the platform</summary>
    public class APIv1ExampleRunnerSearchCustomerAccountRecords
    {
        public static DateTime EPOCH_DATE_TIME = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static string RECORD_DATE_FORMAT = "dd-MM-yyyy";

        public static void runAPIv1ExampleRunnerSearchCustomerAccountRecords()
        {
            Console.WriteLine("Example - Search Supplier Organisation Customer Account Records");
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
            Console.WriteLine("Enter number for type of records to search for:");
            Console.WriteLine("1 - Invoices");
            Console.WriteLine("2 - Sales Orders");
            Console.WriteLine("3 - Back Orders");
            Console.WriteLine("4 - Transactions");
            Console.WriteLine("5 - Credits");
            Console.WriteLine("6 - Payments");

            //validate the type of record to obtain
            int recordTypeNumber = 1;
            try
            {
                recordTypeNumber = Convert.ToInt32(Console.ReadLine());
            }catch{}

            string recordType = ESDocumentConstants.RECORD_TYPE_INVOICE;
            switch(recordTypeNumber)
            {
                case 2:
                    recordType = ESDocumentConstants.RECORD_TYPE_ORDER_SALE;
                    break;
                case 3:
                    recordType = ESDocumentConstants.RECORD_TYPE_BACKORDER;
                    break;
                case 4:
                    recordType = ESDocumentConstants.RECORD_TYPE_TRANSACTION;
                    break;
                case 5:
                    recordType = ESDocumentConstants.RECORD_TYPE_CREDIT;
                    break;
                case 6:
                    recordType = ESDocumentConstants.RECORD_TYPE_PAYMENT;
                    break;
            }

            //determine the amount of months to search for records from
            Console.WriteLine("Number Of Months To Search For Records Back From:");
            int searchBeginMonths = 3;
            try
            {
                searchBeginMonths = Convert.ToInt32(Console.ReadLine());
            }
            catch { }

            Console.WriteLine("(Optioanal) Set Search Term:");
            string searchString = Console.ReadLine();
            Console.WriteLine("(Optioanal) Set Search Field/Type:");
            string searchType = Console.ReadLine();
            Console.WriteLine("(Optioanal) Set Key Record IDs To Search For:");
            string keyRecordIDs = Console.ReadLine();

            long beginDateTime = (long)(DateTime.Now.AddMonths(-searchBeginMonths) - EPOCH_DATE_TIME).TotalMilliseconds;
            long endDateTime = (long)(DateTime.Now - EPOCH_DATE_TIME).TotalMilliseconds;
            int pageNumber = 1;
            int recordsMaxAmount = 100;
            bool outstandingRecordsOnly = false;

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

            //search for records if the API was successfully created
            if (apiOrgSession.doesSessionExist())
            {
                //after 60 seconds give up on waiting for a response from the API when creating the notification
                int timeoutMilliseconds = 60000;

                //call the platform's API to search for customer account records from the supplier organiation's connected business system
                APIv1EndpointResponseESD < ESDocumentCustomerAccountEnquiry > endpointResponseESD = APIv1EndpointOrgSearchCustomerAccountRecords.call(
                    apiOrgSession,
                    timeoutMilliseconds,
                    recordType,
                    supplierOrgID,
                    customerAccountCode,
                    beginDateTime,
                    endDateTime,
                    pageNumber,
                    recordsMaxAmount,
                    outstandingRecordsOnly,
                    searchString,
                    keyRecordIDs,
                    searchType
                );
                ESDocumentCustomerAccountEnquiry esDocumentCustomerAccountEnquiry = endpointResponseESD.esDocument;

                //check that the search successfully occurred
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
                            Console.WriteLine("Invoice Records:");

                            //iterate through each invoice record stored within the standards document
                            int i = 0;
                            foreach (ESDRecordCustomerAccountEnquiryInvoice invoiceRecord in esDocumentCustomerAccountEnquiry.invoiceRecords)
                            {
                                //output details of the invoice record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("     Invoice Record #: " + i);
                                Console.WriteLine("       Key Invoice ID: " + invoiceRecord.keyInvoiceID);
                                Console.WriteLine("           Invoice ID: " + invoiceRecord.invoiceID);
                                Console.WriteLine("       Invoice Number: " + invoiceRecord.invoiceNumber);
                                Console.WriteLine("         Invoice Date: " + EPOCH_DATE_TIME.AddMilliseconds(invoiceRecord.invoiceDate).ToString(RECORD_DATE_FORMAT));
                                Console.WriteLine("Total Price (Inc Tax): " + invoiceRecord.totalIncTax + " " + invoiceRecord.currencyCode);
                                Console.WriteLine("           Total Paid: " + invoiceRecord.totalPaid + " " + invoiceRecord.currencyCode);
                                Console.WriteLine("           Total Owed: " + invoiceRecord.balance + " " + invoiceRecord.currencyCode);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                i++;
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

                            //iterate through each sales order record stored within the standards document
                            int i = 0;
                            foreach (ESDRecordCustomerAccountEnquiryOrderSale orderSaleRecord in esDocumentCustomerAccountEnquiry.orderSaleRecords)
                            {
                                //output details of the sales order record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine(" Sales Order Record #: " + i);
                                Console.WriteLine("    Key Order Sale ID: " + orderSaleRecord.keyOrderSaleID);
                                Console.WriteLine("             Order ID: " + orderSaleRecord.orderID);
                                Console.WriteLine("         Order Number: " + orderSaleRecord.orderNumber);
                                Console.WriteLine("           Order Date: " + EPOCH_DATE_TIME.AddMilliseconds(orderSaleRecord.orderDate).ToString(RECORD_DATE_FORMAT));
                                Console.WriteLine("Total Price (Inc Tax): " + orderSaleRecord.totalIncTax + " " + orderSaleRecord.currencyCode);
                                Console.WriteLine("           Total Paid: " + orderSaleRecord.totalPaid + " " + orderSaleRecord.currencyCode);
                                Console.WriteLine("           Total Owed: " + orderSaleRecord.balance + " " + orderSaleRecord.currencyCode);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                i++;
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

                            //iterate through each back order record stored within the standards document
                            int i = 0;
                            foreach (ESDRecordCustomerAccountEnquiryBackOrder backOrderRecord in esDocumentCustomerAccountEnquiry.backOrderRecords)
                            {
                                //output details of the back order record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("  Back Order Record #: " + i);
                                Console.WriteLine("    Key Back Order ID: " + backOrderRecord.keyBackOrderID);
                                Console.WriteLine("             Order ID: " + backOrderRecord.backOrderID);
                                Console.WriteLine("    Back Order Number: " + backOrderRecord.backOrderNumber);
                                Console.WriteLine("           Order Date: " + EPOCH_DATE_TIME.AddMilliseconds(backOrderRecord.backOrderDate).ToString(RECORD_DATE_FORMAT));
                                Console.WriteLine("Total Price (Inc Tax): " + backOrderRecord.totalIncTax + " " + backOrderRecord.currencyCode);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                i++;
                            }
                        }
                    }
                    else if (recordType == ESDocumentConstants.RECORD_TYPE_TRANSACTION)
                    {
                        Console.WriteLine("SUCCESS - account transaction record data successfully obtained from the platform");
                        Console.WriteLine("Transaction Records Returned: " + esDocumentCustomerAccountEnquiry.totalDataRecords);

                        //check that transaction records have been placed into the standards document
                        if (esDocumentCustomerAccountEnquiry.transactionRecords != null)
                        {
                            Console.WriteLine("Transaction Records:");

                            //iterate through each transaction record stored within the standards document
                            int i = 0;
                            foreach (ESDRecordCustomerAccountEnquiryTransaction transactionRecord in esDocumentCustomerAccountEnquiry.transactionRecords)
                            {
                                //output details of the transaction record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine(" Transaction Record #: " + i);
                                Console.WriteLine("   Key Transaction ID: " + transactionRecord.keyTransactionID);
                                Console.WriteLine("       Transaction ID: " + transactionRecord.transactionID);
                                Console.WriteLine("   Transaction Number: " + transactionRecord.transactionNumber);
                                Console.WriteLine("     Transaction Date: " + EPOCH_DATE_TIME.AddMilliseconds(transactionRecord.transactionDate).ToString(RECORD_DATE_FORMAT));
                                if (transactionRecord.debitAmount > 0)
                                {
                                    Console.WriteLine("       Amount Debited: " + transactionRecord.debitAmount + " " + transactionRecord.currencyCode);
                                }
                                else if (transactionRecord.creditAmount > 0)
                                {
                                    Console.WriteLine("      Amount Credited: " + transactionRecord.creditAmount + " " + transactionRecord.currencyCode);
                                }
                                Console.WriteLine("              Balance: " + transactionRecord.balance + " " + transactionRecord.currencyCode);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                i++;
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

                            //iterate through each credit record stored within the standards document
                            int i = 0;
                            foreach (ESDRecordCustomerAccountEnquiryCredit creditRecord in esDocumentCustomerAccountEnquiry.creditRecords)
                            {
                                //output details of the credit record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("      Credit Record #: " + i);
                                Console.WriteLine("        Key Credit ID: " + creditRecord.keyCreditID);
                                Console.WriteLine("            Credit ID: " + creditRecord.creditID);
                                Console.WriteLine("        Credit Number: " + creditRecord.creditNumber);
                                Console.WriteLine("          Credit Date: " + EPOCH_DATE_TIME.AddMilliseconds(creditRecord.creditDate).ToString(RECORD_DATE_FORMAT));
                                Console.WriteLine("      Amount Credited: " + creditRecord.appliedAmount + " " + creditRecord.currencyCode);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                i++;
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

                            //iterate through each payment record stored within the standards document
                            int i = 0;
                            foreach (ESDRecordCustomerAccountEnquiryPayment paymentRecord in esDocumentCustomerAccountEnquiry.paymentRecords)
                            {
                                //output details of the payment record
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                Console.WriteLine("     Payment Record #: " + i);
                                Console.WriteLine("       Key Payment ID: " + paymentRecord.keyPaymentID);
                                Console.WriteLine("           Payment ID: " + paymentRecord.paymentID);
                                Console.WriteLine("       Payment Number: " + paymentRecord.paymentNumber);
                                Console.WriteLine("         Payment Date: " + EPOCH_DATE_TIME.AddMilliseconds(paymentRecord.paymentDate).ToString(RECORD_DATE_FORMAT));
                                Console.WriteLine("    Total Amount Paid: " + paymentRecord.totalAmount + " " + paymentRecord.currencyCode);
                                Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                                i++;
                            }
                        }
                    }
                }
                else {
                    Console.WriteLine("FAIL - organisation data failed to be obtained from the platform. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);
                }

                //next steps
                //call other API endpoints...
                //destroy API session when done...
                apiOrgSession.destroyOrgSession();
            }
        }
    }
}
