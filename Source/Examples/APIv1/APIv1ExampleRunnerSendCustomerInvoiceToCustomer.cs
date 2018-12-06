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
     * Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then sends an organisation's customer invoice to a customer organisation
     */
    public class APIv1ExampleRunnerSendCustomerInvoiceToCustomer
    {
        public static void runAPIv1ExampleRunnerSendCustomerInvoiceToCustomer()
        {
            Console.WriteLine("Example - Send Customer Invoice To Customer API Session");
            Console.WriteLine("");

            //obtain or load in an organisation's API credentials, in this example from the user in the console
            Console.WriteLine("Enter Organisation ID:");
            string orgID = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Key:");
            string orgAPIKey = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Password:");
            string orgAPIPass = Console.ReadLine();
            Console.WriteLine("Enter Customer Organisation ID:");
            string customerOrgID = Console.ReadLine();
            Console.WriteLine("(optional) Enter Customer's Supplier Account Code:");
            string supplierAccountCode = Console.ReadLine();

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

            //create and send customer invoice if the API was successfully created
            if (apiOrgSession.doesSessionExist())
            {
                //create customer invoice record to import
                ESDRecordCustomerInvoice customerInvoiceRecord = new ESDRecordCustomerInvoice();

                //set data within the customer invoice
                customerInvoiceRecord.keyCustomerInvoiceID = "111";
                customerInvoiceRecord.customerInvoiceCode = "CINV-22";
                customerInvoiceRecord.customerInvoiceNumber = "22";
                customerInvoiceRecord.salesOrderCode = "SO-332";
                customerInvoiceRecord.purchaseOrderNumber = "PO-345";
                customerInvoiceRecord.instructions = "Please pay within 30 days";
                customerInvoiceRecord.keyCustomerAccountID = "2";
                customerInvoiceRecord.customerAccountCode = "ACM-002";

                //set dates within the invoice, in unix time, milliseconds since the 01/01/1970 12AM UTC epoch
                DateTime epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                customerInvoiceRecord.paymentDueDate = (long)(DateTime.UtcNow.AddDays(30) - epochDateTime).TotalMilliseconds;
                customerInvoiceRecord.createdDate = (long)(DateTime.UtcNow - epochDateTime).TotalMilliseconds;
                customerInvoiceRecord.dispatchedDate = (long)(DateTime.UtcNow.AddDays(-2) - epochDateTime).TotalMilliseconds;

                //set delivery address that invoice goods were delivered to
                customerInvoiceRecord.deliveryAddress1 = "32";
                customerInvoiceRecord.deliveryAddress2 = "Main Street";
                customerInvoiceRecord.deliveryAddress3 = "Melbourne";
                customerInvoiceRecord.deliveryRegionName = "Victoria";
                customerInvoiceRecord.deliveryCountryName = "Australia";
                customerInvoiceRecord.deliveryPostcode = "3000";
                customerInvoiceRecord.deliveryOrgName = "Acme Industries";
                customerInvoiceRecord.deliveryContact = "Jane Doe";

                //set billing address that the invoice is billed to for payment
                customerInvoiceRecord.billingAddress1 = "43";
                customerInvoiceRecord.billingAddress2 = " Smith Street";
                customerInvoiceRecord.billingAddress3 = "Melbourne";
                customerInvoiceRecord.billingRegionName = "Victoria";
                customerInvoiceRecord.billingCountryName = "Australia";
                customerInvoiceRecord.billingPostcode = "3000";
                customerInvoiceRecord.billingOrgName = "Supplier Industries International";
                customerInvoiceRecord.billingContact = "Lee Lang";

                //create an array of customer invoice lines
                List<ESDRecordCustomerInvoiceLine> invoiceLines = new List<ESDRecordCustomerInvoiceLine>();

                //create invoice line record 1
                ESDRecordCustomerInvoiceLine invoicedProduct = new ESDRecordCustomerInvoiceLine();
                invoicedProduct.lineType = ESDocumentConstants.INVOICE_LINE_TYPE_PRODUCT;
                invoicedProduct.productCode = "ACME-SUPPLIER-TTGREEN";
                invoicedProduct.productName = "Green tea towel - 30 x 6 centimetres";
                invoicedProduct.keySellUnitID = "2";
                invoicedProduct.unitName = "EACH";
                invoicedProduct.quantityInvoiced = 4;
                invoicedProduct.sellUnitBaseQuantity = 4;
                invoicedProduct.priceExTax = (decimal)5.00;
                invoicedProduct.priceIncTax = (decimal)5.50;
                invoicedProduct.priceTax = (decimal)0.50;
                invoicedProduct.priceTotalIncTax = (decimal)22.00;
                invoicedProduct.priceTotalExTax = (decimal)20.00;
                invoicedProduct.priceTotalTax = (decimal)2.00;
                //optionally specify customer's product code in purchaseOrderProductCode if it is different to the line's productCode field and the supplier org. knows the customer's codes
                invoicedProduct.purchaseOrderProductCode = "TEA-TOWEL-GREEN";

                //add tax details to the product invoice line
                ESDRecordInvoiceLineTax productTax = new ESDRecordInvoiceLineTax();
                productTax.priceTax = invoicedProduct.priceTax;
                productTax.priceTotalTax = invoicedProduct.priceTotalTax;
                productTax.quantity = invoicedProduct.quantityInvoiced;
                productTax.taxRate = (decimal)10.00;
                productTax.taxcode = "GST";
                productTax.taxcodeLabel = "Goods And Services Tax";
                invoicedProduct.taxes = new List<ESDRecordInvoiceLineTax>(){productTax};

                //add 1st invoice line to lines list
                invoiceLines.Add(invoicedProduct);

                //add a 2nd invoice line record that is a text line
                invoicedProduct = new ESDRecordCustomerInvoiceLine();
                invoicedProduct.lineType = ESDocumentConstants.INVOICE_LINE_TYPE_TEXT;
                invoicedProduct.textDescription = "Please bundle tea towels into a box";
                invoiceLines.Add(invoicedProduct);

                //add a 3rd invoice line product record to the invoice
                invoicedProduct = new ESDRecordCustomerInvoiceLine();
                invoicedProduct.lineType = ESDocumentConstants.INVOICE_LINE_TYPE_PRODUCT;
                invoicedProduct.productCode = "ACME-TTBLUE";
                invoicedProduct.quantityInvoiced = 10;
                invoicedProduct.priceExTax = (decimal)10.00;
                invoicedProduct.priceIncTax = (decimal)1.10;
                invoicedProduct.priceTax = (decimal)1.00;
                invoicedProduct.priceTotalIncTax = (decimal)110.00;
                invoicedProduct.priceTotalExTax = (decimal)100.00;
                invoicedProduct.priceTotalTax = (decimal)10.00;
                invoiceLines.Add(invoicedProduct);

                //add lines to the invoice
                customerInvoiceRecord.lines = invoiceLines;

                //set invoice totals
                customerInvoiceRecord.totalPriceIncTax = (decimal)132.00;
                customerInvoiceRecord.totalPriceExTax = (decimal)120.00;
                customerInvoiceRecord.totalTax = (decimal)12.00;
                customerInvoiceRecord.totalLines = invoiceLines.Count;
                customerInvoiceRecord.totalProducts = 2;

                //create customer invoices records list and add customer invoice to it
                List<ESDRecordCustomerInvoice> customerInvoiceRecords = new List<ESDRecordCustomerInvoice>();
                customerInvoiceRecords.Add(customerInvoiceRecord);

                //after 60 seconds give up on waiting for a response from the API when creating the notification
                int timeoutMilliseconds = 60000;

                //create customer invoice Ecommerce Standards document and add customer invoice records to the document
                ESDocumentCustomerInvoice customerInvoiceESD = new ESDocumentCustomerInvoice(ESDocumentConstants.RESULT_SUCCESS, "successfully obtained data", customerInvoiceRecords.ToArray(), new Dictionary<string, string>());

                //send customer invoice document to the API and onto the customer organisation
                APIv1EndpointResponseESD<ESDocumentSupplierInvoice> endpointResponseESD = APIv1EndpointOrgSendCustomerInvoiceToCustomer.call(apiOrgSession, timeoutMilliseconds, customerOrgID, supplierAccountCode, customerInvoiceESD);
                ESDocumentSupplierInvoice esDocumentSupplierInvoice = endpointResponseESD.esDocument;

                //check the result of sending the supplier invoice
                if (endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                {
                    Console.WriteLine("SUCCESS - organisation customer invoice(s) have successfully been sent to customer organisation.");

                    //iterate through each of the returned supplier invoice(s) that the customer invoice(s) were converted from and output the details of the supplier invoices
                    if (esDocumentSupplierInvoice.dataRecords != null)
                    {
                        foreach (ESDRecordSupplierInvoice supplierInvoiceRecord in esDocumentSupplierInvoice.dataRecords)
                        {
                            Console.WriteLine("\nCustomer Invoice Returned, Invoice Details: ");
                            Console.WriteLine("Supplier Invoice Code: " + supplierInvoiceRecord.supplierInvoiceCode);
                            Console.WriteLine("Supplier Invoice Total Cost: " + supplierInvoiceRecord.totalPriceIncTax + " (" + supplierInvoiceRecord.currencyISOCode + ")");
                            Console.WriteLine("Supplier Invoice Total Taxes: " + supplierInvoiceRecord.totalTax + " (" + supplierInvoiceRecord.currencyISOCode + ")");
                            Console.WriteLine("Supplier Invoice Supplier Account: " + supplierInvoiceRecord.supplierAccountCode);
                            Console.WriteLine("Supplier Invoice Total Lines: " + supplierInvoiceRecord.totalLines);
                        }
                    }
                }
                else {
                    Console.WriteLine("FAIL - organisation customer invoice(s) failed to be processed. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);

                    //check that a Ecommerce standards document was returned
                    if (esDocumentSupplierInvoice != null && esDocumentSupplierInvoice.configs != null)
                    {
                        //if one or more products in the customer invoice could not match a product to the customer organisation then find out the invoice lines that caused the problem
                        if (esDocumentSupplierInvoice.configs.ContainsKey(APIv1EndpointResponseESD<ESDocumentSupplierInvoice>.ESD_CONFIG_INVOICES_WITH_UNMAPPED_LINES))
                        {
                            //get a list of invoice lines that could not be mapped
                            List<KeyValuePair<int, int>> unmappedLines = APIv1EndpointOrgSendCustomerInvoiceToCustomer.getUnmappedInvoiceLines(esDocumentSupplierInvoice);

                            //iterate through each unmapped invoice line
                            foreach (KeyValuePair<int, int> unmappedLine in unmappedLines)
                            {
                                //get the index of the customer invoice and line that contained the unmapped product
                                int invoiceIndex = unmappedLine.Key;
                                int lineIndex = unmappedLine.Value;

                                //check that the invoice can be found that contains the problematic line
                                if (invoiceIndex < customerInvoiceESD.dataRecords.Length && lineIndex < customerInvoiceESD.dataRecords[invoiceIndex].lines.Count)
                                {
                                    Console.WriteLine("For customer invoice: " + customerInvoiceESD.dataRecords[invoiceIndex].customerInvoiceCode + " a matching customer product for line number: " + (lineIndex + 1) + " could not be matched up or handled.");
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
