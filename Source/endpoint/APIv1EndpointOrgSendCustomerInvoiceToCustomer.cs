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
using EcommerceStandardsDocuments;
using System.Web;

namespace Squizz.Platform.API.v1.endpoint
{
    /// <summary>
    ///     Class handles calling the SQUIZZ.com API endpoint to send one more of an organisation's customer invoices to the platform, where they are then converted into supplier invoices and sent to a customer organisation for importing and processing.
    ///     This endpoint allows the invoicing of goods and services supplied by a supplying organisation logged into the API session. to be sent their chosen customer organisation
    /// </summary>
    public class APIv1EndpointOrgSendCustomerInvoiceToCustomer
    {
        /// <summary>Calls the platform's API endpoint to push up a customer invoice and have it be sent to a connected customer organisation</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="customerOrgID">unique ID of the customer organisation in the SQUIZZ.com platform</param>
        /// <param name="supplierAccountCode">code of the customer organisation's supplier account. Supplier account only needs to be set if the customer organisation has assigned multiple accounts to the supplier organisation logged into the API session (supplier org)</param>
        /// <param name="esDocumentCustomerInvoice">Customer Invoice Ecommerce Standards Document that contains one or more customer invoice records</param>
        /// <returns>response from calling the API endpoint</returns>
        public static APIv1EndpointResponseESD<ESDocumentSupplierInvoice> call(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, String customerOrgID, String supplierAccountCode, ESDocumentCustomerInvoice esDocumentCustomerInvoice)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            requestHeaders.Add(new KeyValuePair<string, string>(APIv1HTTPRequest.HTTP_HEADER_CONTENT_ENCODING, APIv1HTTPRequest.HTTP_HEADER_CONTENT_ENCODING_GZIP));
            requestHeaders.Add(new KeyValuePair<string, string>(APIv1HTTPRequest.HTTP_HEADER_CONTENT_TYPE, APIv1HTTPRequest.HTTP_HEADER_CONTENT_TYPE_JSON));
            APIv1EndpointResponseESD<ESDocumentSupplierInvoice> endpointResponse = new APIv1EndpointResponseESD<ESDocumentSupplierInvoice>();

            try
            {
                //set notification parameters
                String endpointParams = "customer_org_id=" + HttpUtility.UrlEncode(customerOrgID) + "&supplier_account_code=" + HttpUtility.UrlEncode(supplierAccountCode);

                //make a HTTP request to the platform's API endpoint to send the ESD containing the customer invoices
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest<ESDocumentSupplierInvoice>(APIv1Constants.HTTP_REQUEST_METHOD_POST, APIv1Constants.API_ORG_ENDPOINT_SEND_CUSTOMER_INVOICE_TO_CUSTOMER + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", esDocumentCustomerInvoice, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

                //get result status and result code from document
                if (endpointResponse.esDocument != null)
                {
                    //get the result status from the esDocument
                    if (endpointResponse.esDocument.resultStatus == ESDocumentConstants.RESULT_SUCCESS)
                    {
                        endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS;
                    }

                    //get the result code from the ESdocument's configs if possible
                    if (endpointResponse.esDocument.configs != null)
                    {
                        string resultCode = "";
                        if (!endpointResponse.esDocument.configs.TryGetValue(APIv1Constants.API_ORG_ENDPOINT_ATTRIBUTE_RESULT_CODE, out resultCode))
                        {
                            resultCode = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_UNKNOWN;
                        }
                        endpointResponse.result_code = resultCode;
                    }
                }

                //get the message that corresponds with the result code
                if (!String.IsNullOrWhiteSpace(apiOrgSession.getLangBundle().GetString(endpointResponse.result_code, apiOrgSession.languageLocale)))
                {
                    endpointResponse.result_message = apiOrgSession.getLangBundle().GetString(endpointResponse.result_code);
                }

                //check that the data was successfully pushed up
                if (endpointResponse.result.ToUpper() != APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
                {
                    //check if the session still exists
                    if (endpointResponse.result_code.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_SESSION_INVALID)
                    {
                        //mark that the session has expired
                        apiOrgSession.markSessionExpired();
                    }
                }
            }
            catch (Exception ex)
            {
                endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_UNKNOWN;
                endpointResponse.result_message = apiOrgSession.getLangBundle().GetString(endpointResponse.result_code, apiOrgSession.languageLocale) + "\n" + ex.Message;
            }

            return endpointResponse;
        }

        /// <summary>gets a list of invoice indexes that contain invoice lines that could not be mapped to a customer organisation's products</summary>
        /// <param name="esDocument">Ecommerce standards document containing configuration that specifies unmapped invoice lines</param>
        /// <returns>array containing pairs. Each pair has the index of the invoice, and the index of the invoice line that could not be mapped</returns>
        public static List<KeyValuePair<int, int>> getUnmappedInvoiceLines(ESDocument esDocument)
        {
            List<KeyValuePair<int, int>> upmappedInvoiceLines = new List<KeyValuePair<int, int>>();

            //check that the ecommerce standards document's configs contains a key specifying the unmapped invoice lines
            if (esDocument.configs.ContainsKey(APIv1EndpointResponseESD<ESDocumentSupplierInvoice>.ESD_CONFIG_INVOICES_WITH_UNMAPPED_LINES))
            {
                //get comma separated list of invoice record indicies and line indicies that indicate the unmapped invoice lines
                String unmappedInvoiceLineCSV = esDocument.configs[APIv1EndpointResponseESD<ESDocumentSupplierInvoice>.ESD_CONFIG_INVOICES_WITH_UNMAPPED_LINES];

                //get the index of the invoice record and line that contained the unmapped product
                if (!String.IsNullOrWhiteSpace(unmappedInvoiceLineCSV.Trim()))
                {
                    String[] unmappedInvoiceLineIndices = unmappedInvoiceLineCSV.Trim().Split(',');

                    //iterate through each invoice-line index
                    for (int i = 0; i < unmappedInvoiceLineIndices.Length; i++)
                    {
                        //get invoice index and line index
                        string[] invoiceLineIndex = unmappedInvoiceLineIndices[i].Split(':');
                        if (invoiceLineIndex.Length == 2)
                        {
                            try
                            {
                                int invoiceIndex = Convert.ToInt32(invoiceLineIndex[0]);
                                int lineIndex = Convert.ToInt32(invoiceLineIndex[1]);
                                KeyValuePair<int, int> invoiceLinePair = new KeyValuePair<int, int>(invoiceIndex, lineIndex);
                                upmappedInvoiceLines.Add(invoiceLinePair);

                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }

            return upmappedInvoiceLines;
        }
    }
}
