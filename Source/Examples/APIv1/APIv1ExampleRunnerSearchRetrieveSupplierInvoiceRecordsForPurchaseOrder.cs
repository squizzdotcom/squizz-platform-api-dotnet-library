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
    /// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then calling the endpoint to search for invoices matching a purchase order number in the conencted supplier organisation's business system, and retrieving the details of each invoice record found</summary>
    public class APIv1ExampleRunnerSearchRetrieveSupplierInvoiceRecordsForPurchaseOrder
    {
        public static DateTime EPOCH_DATE_TIME = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static string RECORD_DATE_FORMAT = "dd-MM-yyyy";
        public static string RECORD_CURRENCY_FORMAT = "F2";

        public static void runAPIv1ExampleRunnerSearchCustomerAccountRecords()
        {
            Console.WriteLine("Example - Search and Retrieve Supplier Organisation Invoice Records For Purchase Order");
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

            //determine the amount of months to search for records from
            Console.WriteLine("Number Of Months To Search For Invoice Records Back From:");
            int searchBeginMonths = 3;
            try
            {
                searchBeginMonths = Convert.ToInt32(Console.ReadLine());
            }
            catch { }

            Console.WriteLine("Enter Purchase Order Number:");
            string searchString = Console.ReadLine();

            //set variables used to search for invoice records
            long beginDateTime = (long)(DateTime.Now.AddMonths(-searchBeginMonths) - EPOCH_DATE_TIME).TotalMilliseconds;
            long endDateTime = (long)(DateTime.Now - EPOCH_DATE_TIME).TotalMilliseconds;
            string recordType = ESDocumentConstants.RECORD_TYPE_INVOICE;
            string searchType = "purchaseOrderNumber";
            string keyRecordIDs = "";
            int pageNumber = 1;
            int recordsMaxAmount = 10;
            bool outstandingRecordsOnly = false;
            bool getInvoiceDetails = true;
            bool displayInvoiceDetails = true;
            List<ESDRecordCustomerAccountEnquiryInvoice> invoiceRecords = new List<ESDRecordCustomerAccountEnquiryInvoice>();

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
                APIv1EndpointResponseESD<ESDocumentCustomerAccountEnquiry> searchEndpointResponseESD = APIv1EndpointOrgSearchCustomerAccountRecords.call(
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
                ESDocumentCustomerAccountEnquiry esDocumentCustomerAccountEnquiry = (ESDocumentCustomerAccountEnquiry)searchEndpointResponseESD.esDocument;

                //check that the search successfully occurred
                if (searchEndpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                {
                    Console.WriteLine("SUCCESS - account invoice record data successfully obtained from the platform");
                    Console.WriteLine("\nInvoice Records Returned: " + esDocumentCustomerAccountEnquiry.totalDataRecords);

                    //check that invoice records have been placed into the standards document
                    if (esDocumentCustomerAccountEnquiry.invoiceRecords != null)
                    {
                        Console.WriteLine(esDocumentCustomerAccountEnquiry.invoiceRecords.Length + " invoice records have been found from the search.");
                        getInvoiceDetails = true;
                    }
                    else
                    {
                        Console.WriteLine("No matching invoice records were found in the supplier's connected business system.");
                    }
                }
                else {
                    Console.WriteLine("FAIL - organisation data failed to be obtained from the platform. Reason: " + searchEndpointResponseESD.result_message + " Error Code: " + searchEndpointResponseESD.result_code);
                }

                //obtain the details and lines of the invoice record found by calling the retrieve customer account record endpoint
                if(getInvoiceDetails)
                {
                    //iterate through each invoice previously found
                    foreach (ESDRecordCustomerAccountEnquiryInvoice invoiceRecord in esDocumentCustomerAccountEnquiry.invoiceRecords)
                    {
                        //call the platform's API to retrieve the details of the invoice record from the supplier organisation's connected business system
                        APIv1EndpointResponseESD<ESDocumentCustomerAccountEnquiry> endpointResponseESD = APIv1EndpointOrgRetrieveCustomerAccountRecord.call(
                            apiOrgSession,
                            timeoutMilliseconds,
                            recordType,
                            supplierOrgID,
                            customerAccountCode,
                            invoiceRecord.keyInvoiceID
                        );
                        ESDocumentCustomerAccountEnquiry esDocumentCustomerAccountEnquiryDetail = endpointResponseESD.esDocument;

                        //check that the data successfully retrieved
                        if (endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                        {
                            //check that a single invoice record was returned from the endpoint
                            if (esDocumentCustomerAccountEnquiryDetail.invoiceRecords != null && esDocumentCustomerAccountEnquiryDetail.invoiceRecords.Length > 0)
                            {
                                invoiceRecords.Add(esDocumentCustomerAccountEnquiryDetail.invoiceRecords[0]);
                            }
                            else
                            {
                                Console.WriteLine("The details of the invoice were unable to be found for invoice with the keyInvoiceID: " + invoiceRecord.keyInvoiceID + " Invoice Number: " + invoiceRecord.invoiceNumber);
                            }
                        }
                        else {
                            Console.WriteLine("FAIL - Unable to retrieve invoice record data from the platform's API endpoint. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);
                            displayInvoiceDetails = false;
                            break;
                        }
                    }
                }

                //display the details of all the found invoice records
                if (getInvoiceDetails && displayInvoiceDetails)
                {
                    Console.WriteLine(APIv1ExampleRunner.CONSOLE_LINE);
                    Console.WriteLine("Displaying details of the "+ invoiceRecords.Count + " invoice records found:");

                    //display the details of the record stored within the standards document
                    foreach (ESDRecordCustomerAccountEnquiryInvoice invoiceRecord in invoiceRecords)
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

                //next steps
                //call other API endpoints...
                //destroy API session when done...
                apiOrgSession.destroyOrgSession();
            }
        }
    }
}
