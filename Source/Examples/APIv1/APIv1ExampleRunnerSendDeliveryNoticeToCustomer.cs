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
     * Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then sends an organisation's delivery notice to a customer to advise of the delivery of goods
     */
    public class APIv1ExampleRunnerSendDeliveryNoticeToCustomer
    {
        public static void runAPIv1ExampleRunnerSendDeliveryNoticeToCustomer()
        {
            Console.WriteLine("Example - Send Delivery Notice To Customer API Session");
            Console.WriteLine("");

            //obtain or load in an organisation's API credentials, in this example from the user in the console
            Console.WriteLine("Enter Organisation ID:");
            string orgID = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Key:");
            string orgAPIKey = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Password:");
            string orgAPIPass = Console.ReadLine();
            Console.WriteLine("(optional) Enter Customer Organisation ID:");
            string customerOrgID = Console.ReadLine();
            Console.WriteLine("(optional) Enter Customer's Supplier Account Code:");
            string supplierAccountCode = Console.ReadLine();
            Console.WriteLine("(optional) Should Notice Be Exported Using Data Adaptor ("+ ESDocumentConstants.ESD_VALUE_YES + " or "+ ESDocumentConstants.ESD_VALUE_NO + "):");
            bool useDeliveryNoticeExport = Console.ReadLine().Trim().ToUpper() == ESDocumentConstants.ESD_VALUE_YES;

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

            //create and send delivery notice if the API was successfully created
            if (apiOrgSession.doesSessionExist())
            {
                //create delivery notice record to import
                ESDRecordDeliveryNotice deliveryNoticeRecord = new ESDRecordDeliveryNotice();

                //set data within the delivery notice
                deliveryNoticeRecord.keyDeliveryNoticeID = "DN123";
                deliveryNoticeRecord.deliveryNoticeCode = "CUSDELNUM-123-A";
                deliveryNoticeRecord.deliveryStatus = ESDocumentConstants.DELIVERY_STATUS_IN_TRANSIT;
                deliveryNoticeRecord.deliveryStatusMessage = "Currently en-route to receiver.";

                //set information about the freight carrier currently performing the delivery
                deliveryNoticeRecord.freightCarrierName = "ACME Freight Logistics Inc.";
                deliveryNoticeRecord.freightCarrierCode = "ACFLI";
                deliveryNoticeRecord.freightCarrierTrackingCode = "34320-ACFLI-34324-234";
                deliveryNoticeRecord.freightCarrierAccountCode = "VIP00012";
                deliveryNoticeRecord.freightCarrierConsignCode = "42343-242344";
                deliveryNoticeRecord.freightCarrierServiceCode = "SUPER-SMART-FREIGHT-FACILITATOR";
                deliveryNoticeRecord.freightSystemRefCode = "SSFF-3421";

                // add references to other records (sales order, customer invoice, purchase order, customer account) that this delivery is associated to
                deliveryNoticeRecord.keyCustomerInvoiceID = "111";
                deliveryNoticeRecord.customerInvoiceCode = "CINV-22";
                deliveryNoticeRecord.customerInvoiceNumber = "22";
                deliveryNoticeRecord.keySalesOrderID = "332";
                deliveryNoticeRecord.salesOrderCode = "SO-332";
                deliveryNoticeRecord.salesOrderNumber = "SO-332";
                deliveryNoticeRecord.purchaseOrderCode = "PO-345";
                deliveryNoticeRecord.purchaseOrderNumber = "345";
                deliveryNoticeRecord.instructions = "Please leave goods via the back driveway";
                deliveryNoticeRecord.keyCustomerAccountID = "2";

                // set where the delivery is currently located geographically
                deliveryNoticeRecord.atGeographicLocation = ESDocumentConstants.ESD_VALUE_YES;
                deliveryNoticeRecord.locationLatitude = (decimal)-37.8277324706811;
                deliveryNoticeRecord.locationLongitude = (decimal)144.92382897158126;

                //set dates within the invoice, in unix time, milliseconds since the 01/01/1970 12AM UTC epoch
                DateTime epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                deliveryNoticeRecord.deliveryDate = (long)(DateTime.UtcNow - epochDateTime).TotalMilliseconds;
                deliveryNoticeRecord.dispatchedDate = (long)(DateTime.UtcNow.AddDays(-2) - epochDateTime).TotalMilliseconds;

                //create delivery notice records list and add the delivery notice to it
                List<ESDRecordDeliveryNotice> deliveryNoticeRecords = new List<ESDRecordDeliveryNotice>();
                deliveryNoticeRecords.Add(deliveryNoticeRecord);

                //after 60 seconds give up on waiting for a response from the API when creating the delivery notice
                int timeoutMilliseconds = 60000;

                //create delivery notice Ecommerce Standards document and add delivery notice records to the document
                ESDocumentDeliveryNotice deliveryNoticeESD = new ESDocumentDeliveryNotice(ESDocumentConstants.RESULT_SUCCESS, "successfully obtained data", deliveryNoticeRecords.ToArray(), new Dictionary<string, string>());

                //send delivery notice document to the API and onto the customer
                APIv1EndpointResponseESD<ESDocument> endpointResponseESD = APIv1EndpointOrgSendDeliveryNoticeToCustomer.call(apiOrgSession, timeoutMilliseconds, customerOrgID, supplierAccountCode, useDeliveryNoticeExport, deliveryNoticeESD);
                ESDocument esDocumentResult = endpointResponseESD.esDocument;

                //check the result of sending the supplier invoice
                if (endpointResponseESD.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                {
                    Console.WriteLine("SUCCESS - organisation delivery notice(s) have successfully been sent to customer.");

                    
                }
                else
                {
                    Console.WriteLine("FAIL - organisation delivery notice(s) failed to be processed. Reason: " + endpointResponseESD.result_message + " Error Code: " + endpointResponseESD.result_code);
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
