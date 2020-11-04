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
using EcommerceStandardsDocuments;
using System.Web;
using Newtonsoft.Json;
namespace Squizz.Platform.API.v1.endpoint
{
    /// <summary>
    ///     Class handles calling the SQUIZZ.com API endpoint to send one more of an organisation's sales orders into the platform, where they are then validated, optionally re-priced and raised against the organisation for processing and dispatch.
    ///     This endpoint allows an organisation to import its own sales orders from its own selling system(s) into SQUIZZ.com, via an API session that the organisation has logged into
    /// </summary>
    public class APIv1EndpointOrgImportSalesOrder
    {
        /// <summary>Calls the platform's API endpoint and pushes up and import organisation data in a Ecommerce Standards Document of a specified type</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="esDocumentOrderSale">Sales Order Ecommerce Standards Document that contains one or more sales order records
        /// <param name="repriceOrder">if true then allow the order lines and surcharges to be repriced on import</param>
        /// <returns>response from calling the API endpoint</returns>
        public static APIv1EndpointResponseESD<ESDocumentOrderSale> call(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, ESDocumentOrderSale esDocumentOrderSale, bool repriceOrder)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            requestHeaders.Add(new KeyValuePair<string, string>(APIv1HTTPRequest.HTTP_HEADER_CONTENT_ENCODING, APIv1HTTPRequest.HTTP_HEADER_CONTENT_ENCODING_GZIP));
            requestHeaders.Add(new KeyValuePair<string, string>(APIv1HTTPRequest.HTTP_HEADER_CONTENT_TYPE, APIv1HTTPRequest.HTTP_HEADER_CONTENT_TYPE_JSON));
            APIv1EndpointResponseESD<ESDocumentOrderSale> endpointResponse = new APIv1EndpointResponseESD<ESDocumentOrderSale>();

            try
            {
                //set request parameters
                String endpointParams = "reprice_order=" + (repriceOrder ? ESDocumentConstants.ESD_VALUE_YES : ESDocumentConstants.ESD_VALUE_NO);

                //make a HTTP request to the platform's API endpoint to send the ESD containing the sales orders
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest<ESDocumentOrderSale>(APIv1Constants.HTTP_REQUEST_METHOD_POST, APIv1Constants.API_ORG_ENDPOINT_IMPORT_SALES_ORDER_ESD + APIv1Constants.API_PATH_SLASH + apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", esDocumentOrderSale, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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

        /// <summary>gets a list of order indexes that contain order lines that could not be mapped to a supplier organisation's products</summary>
        /// <param name="esDocument">Ecommerce standards document containing configuration that specifies unmapped order lines</param>
        /// <returns>array containing pairs. Each pair has the index of the order, and the index of the order line that could not be mapped</returns>
        public static List<KeyValuePair<int, int>> getUnmatchedOrderLines(ESDocument esDocument)
        {
            List<KeyValuePair<int, int>> unmatchedOrderLines = new List<KeyValuePair<int, int>>();

            //check that the ecommerce standards document's configs contains a key specifying the unmapped order lines
            if (esDocument.configs.ContainsKey(APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNMATCHED_LINES))
            {
                //get comma separated list of order record indicies and line indicies that indicate the unmapped order lines
                String unmatchedOrderLineCSV = esDocument.configs[APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNMATCHED_LINES];

                //get the index of the order record and line that contained the unmatched product
                if (!String.IsNullOrWhiteSpace(unmatchedOrderLineCSV.Trim()))
                {
                    String[] unmatchedOrderLineIndices = unmatchedOrderLineCSV.Trim().Split(',');

                    //iterate through each order-line index
                    for (int i = 0; i < unmatchedOrderLineIndices.Length; i++)
                    {
                        //get order index and line index
                        String[] orderLineIndex = unmatchedOrderLineIndices[i].Split(':');
                        if (orderLineIndex.Length == 2)
                        {
                            try
                            {
                                int orderIndex = Convert.ToInt32(orderLineIndex[0]);
                                int lineIndex = Convert.ToInt32(orderLineIndex[1]);
                                KeyValuePair<int, int> orderLinePair = new KeyValuePair<int, int>(orderIndex, lineIndex);
                                unmatchedOrderLines.Add(orderLinePair);

                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }

            return unmatchedOrderLines;
        }

        /// <summary>gets a list of order indexes that contain order lines that could not be priced for a supplier organisation's products</summary>
        /// <param name="esDocument">Ecommerce standards document containing configuration that specifies unpriced order lines</param>
        /// <returns>array containing pairs. Each pair has the index of the order, and the index of the order line that could not be priced</returns>
        public static List<KeyValuePair<int, int>> getUnpricedOrderLines(ESDocument esDocument)
        {
            List<KeyValuePair<int, int>> unpricedOrderLines = new List<KeyValuePair<int, int>>();

            //check that the ecommerce standards document's configs contains a key specifying the unpriced order lines
            if (esDocument.configs.ContainsKey(APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNPRICED_LINES))
            {
                //get comma separated list of order record indicies and line indicies that indicate the unpriced order lines
                String unpricedOrderLineCSV = esDocument.configs[APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNPRICED_LINES];

                //get the index of the order record and line that contained the unpriced product
                if (!String.IsNullOrWhiteSpace(unpricedOrderLineCSV.Trim()))
                {
                    String[] unmappedOrderLineIndices = unpricedOrderLineCSV.Trim().Split(',');

                    //iterate through each order-line index
                    for (int i = 0; i < unmappedOrderLineIndices.Length; i++)
                    {
                        //get order index and line index
                        String[] orderLineIndex = unmappedOrderLineIndices[i].Split(':');
                        if (orderLineIndex.Length == 2)
                        {
                            try
                            {
                                int orderIndex = Convert.ToInt32(orderLineIndex[0]);
                                int lineIndex = Convert.ToInt32(orderLineIndex[1]);
                                KeyValuePair<int, int> orderLinePair = new KeyValuePair<int, int>(orderIndex, lineIndex);
                                unpricedOrderLines.Add(orderLinePair);

                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }

            return unpricedOrderLines;
        }

        /// <summary>gets a list of order indexes that contain order surcharges that could not be matched to the organisation's own surcharges</summary>
        /// <param name="esDocument">Ecommerce standards document containing configuration that specifies unmatched order surcharges</param>
        /// <returns>an array containing pairs. Each pair has the index of the order, and the index of the order surcharge that could not be matched</returns>
        public static List<KeyValuePair<int, int>> getUnmatchedOrderSurcharges(ESDocument esDocument)
        {
            List<KeyValuePair<int, int>> unmatchedOrderSurcharges = new List<KeyValuePair<int, int>>();

            //check that the ecommerce standards document's configs contains a key specifying the unmapped order surcharges
            if (esDocument.configs.ContainsKey(APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNMATCHED_SURCHARGES))
            {
                //get comma separated list of order record indicies and surcharge record indicies that indicate the unmatched order surcharge
                String unmappedOrderSurchargeCSV = esDocument.configs[APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNMATCHED_SURCHARGES];

                //get the index of the order record and surcharge that contained the unmatched surcharge
                if (!String.IsNullOrWhiteSpace(unmappedOrderSurchargeCSV.Trim()))
                {
                    String[] unmappedOrderSurchargeIndices = unmappedOrderSurchargeCSV.Trim().Split(',');

                    //iterate through each order-surcharge index
                    for (int i = 0; i < unmappedOrderSurchargeIndices.Length; i++)
                    {
                        //get order index and surcharge index
                        String[] orderSurchargeIndex = unmappedOrderSurchargeIndices[i].Split(':');
                        if (orderSurchargeIndex.Length == 2)
                        {
                            try
                            {
                                int orderIndex = Convert.ToInt32(orderSurchargeIndex[0]);
                                int surcahrgeRecordIndex = Convert.ToInt32(orderSurchargeIndex[1]);
                                KeyValuePair<int, int> orderSurchargePair = new KeyValuePair<int, int>(orderIndex, surcahrgeRecordIndex);
                                unmatchedOrderSurcharges.Add(orderSurchargePair);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }

            return unmatchedOrderSurcharges;
        }

        /// <summary>gets a list of order indexes that contain order surcharges that could not be priced for the organisation's own surcharges</summary>
        /// <param name="esDocument">Ecommerce standards document containing configuration that specifies unpriced order surcharges</param>
        /// <returns>an array containing pairs. Each pair has the index of the order, and the index of the order surcharge that could not be priced</returns>
        public static List<KeyValuePair<int, int>> getUnpricedOrderSurcharges(ESDocument esDocument)
        {
            List<KeyValuePair<int, int>> unpricedOrderSurcharges = new List<KeyValuePair<int, int>>();

            //check that the ecommerce standards document's configs contains a key specifying the unpriced order surcharges
            if (esDocument.configs.ContainsKey(APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNPRICED_SURCHARGES))
            {
                //get comma separated list of order record indicies and surcharge indicies that indicate the unpriced order surcharges
                String unpricedOrderSurchargeCSV = esDocument.configs[APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNPRICED_SURCHARGES];

                //get the index of the order record and surcharge that contained the unpriced product
                if (!String.IsNullOrWhiteSpace(unpricedOrderSurchargeCSV.Trim()))
                {
                    String[] unmappedOrderSurchargeIndices = unpricedOrderSurchargeCSV.Trim().Split(',');

                    //iterate through each order-surcharge index
                    for (int i = 0; i < unmappedOrderSurchargeIndices.Length; i++)
                    {
                        //get order index and surcharge index
                        String[] orderSurchargeIndex = unmappedOrderSurchargeIndices[i].Split(':');
                        if (orderSurchargeIndex.Length == 2)
                        {
                            try
                            {
                                int orderIndex = Convert.ToInt32(orderSurchargeIndex[0]);
                                int surchargeIndex = Convert.ToInt32(orderSurchargeIndex[1]);
                                KeyValuePair<int, int> orderSurchargePair = new KeyValuePair<int, int>(orderIndex, surchargeIndex);
                                unpricedOrderSurcharges.Add(orderSurchargePair);

                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }

            return unpricedOrderSurcharges;
        }

        /// <summary>gets a list of order indexes that contain order payments that could not be matched to the organisation's own payment types</summary>
        /// <param name="esDocument">Ecommerce standards document containing configuration that specifies unmatched order payments</param>
        /// <returns>an array containing pairs. Each pair has the index of the order, and the index of the order payment that could not be matched</returns>
        public static List<KeyValuePair<int, int>> getUnmatchedOrderPayments(ESDocument esDocument)
        {
            List<KeyValuePair<int, int>> unmatchedOrderPayments = new List<KeyValuePair<int, int>>();

            //check that the ecommerce standards document's configs contains a key specifying the unmapped order paymenys
            if (esDocument.configs.ContainsKey(APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNMATCHED_PAYMENTS))
            {
                //get comma separated list of order record indicies and payment record indicies that indicate the unmatched order payment
                String unmappedOrderPaymentCSV = esDocument.configs[APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNMATCHED_PAYMENTS];

                //get the index of the order record and payment that contained the unmatched payment
                if (!String.IsNullOrWhiteSpace(unmappedOrderPaymentCSV.Trim()))
                {
                    String[] unmappedOrderPaymentIndices = unmappedOrderPaymentCSV.Trim().Split(',');

                    //iterate through each order-payment index
                    for (int i = 0; i < unmappedOrderPaymentIndices.Length; i++)
                    {
                        //get order index and payment index
                        String[] orderPaymentIndex = unmappedOrderPaymentIndices[i].Split(':');
                        if (orderPaymentIndex.Length == 2)
                        {
                            try
                            {
                                int orderIndex = Convert.ToInt32(orderPaymentIndex[0]);
                                int paymentRecordIndex = Convert.ToInt32(orderPaymentIndex[1]);
                                KeyValuePair<int, int> orderPaymentPair = new KeyValuePair<int, int>(orderIndex, paymentRecordIndex);
                                unmatchedOrderPayments.Add(orderPaymentPair);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }

            return unmatchedOrderPayments;
        }
    }
}