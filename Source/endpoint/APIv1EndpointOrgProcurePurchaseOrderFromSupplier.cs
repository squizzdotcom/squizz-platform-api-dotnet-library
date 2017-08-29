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
using EcommerceStandardsDocuments;
using System.Web;
using Newtonsoft.Json;
namespace Squizz.Platform.API.v1.endpoint
{
    /// <summary>
    ///     Class handles calling the SQUIZZ.com API endpoint to send one more of an organisation's purchase orders into the platform, where they are then converted into sales orders and sent to a supplier organisation for processing and dispatch.
    ///     This endpoint allows goods and services to be purchased by the "customer" organisation logged into the API session from their chosen supplier organisation
    /// </summary>
    public class APIv1EndpointOrgProcurePurchaseOrderFromSupplier 
    {
        /// <summary>Calls the platform's API endpoint and pushes up and import organisation data in a Ecommerce Standards Document of a specified type</summary>
        /// <param name="apiOrgSession">existing organisation API session</param>
        /// <param name="endpointTimeoutMilliseconds">amount of milliseconds to wait after calling the the API before giving up, set a positive number</param>
        /// <param name="supplierOrgID">unique ID of the supplier organisation in the SQUIZZ.com platform</param>
        /// <param name="customerAccountCode">code of the supplier organisation's customer account. Customer account only needs to be set if the supplier organisation has assigned multiple accounts to the organisation logged into the API session (customer org)</param>
        /// <param name="esDocumentOrderPurchase">Purchase Order Ecommerce Standards Document that contains one or more purchase order records</param>
        /// <returns>response from calling the API endpoint</returns>
        public static APIv1EndpointResponseESD<ESDocumentOrderSale> call(APIv1OrgSession apiOrgSession, int endpointTimeoutMilliseconds, String supplierOrgID, String customerAccountCode, ESDocumentOrderPurchase esDocumentOrderPurchase)
        {
            List<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
            requestHeaders.Add(new KeyValuePair<string, string>(APIv1HTTPRequest.HTTP_HEADER_CONTENT_ENCODING, APIv1HTTPRequest.HTTP_HEADER_CONTENT_ENCODING_GZIP));
            requestHeaders.Add(new KeyValuePair<string, string>(APIv1HTTPRequest.HTTP_HEADER_CONTENT_TYPE, APIv1HTTPRequest.HTTP_HEADER_CONTENT_TYPE_JSON));
            APIv1EndpointResponseESD<ESDocumentOrderSale> endpointResponse = new APIv1EndpointResponseESD<ESDocumentOrderSale>();
            
            try
            {
                //set notification parameters
                String endpointParams = "supplier_org_id="+ HttpUtility.UrlEncode(supplierOrgID) + "&customer_account_code="+ HttpUtility.UrlEncode(customerAccountCode);
                
                //make a HTTP request to the platform's API endpoint to send the ESD containing the purchase orders
                endpointResponse = APIv1HTTPRequest.sendESDocumentHTTPRequest<ESDocumentOrderSale>(APIv1Constants.HTTP_REQUEST_METHOD_POST, APIv1Constants.API_ORG_ENDPOINT_PROCURE_PURCHASE_ORDER_FROM_SUPPLIER+APIv1Constants.API_PATH_SLASH+apiOrgSession.getSessionID(), endpointParams, requestHeaders, "", esDocumentOrderPurchase, endpointTimeoutMilliseconds, apiOrgSession.getLangBundle(), apiOrgSession.languageLocale, endpointResponse);

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
                    if(endpointResponse.result_code.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_SESSION_INVALID){
                        //mark that the session has expired
                        apiOrgSession.markSessionExpired();
                    }
                }
            }
            catch(Exception ex)
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
        public static List<KeyValuePair<int, int>> getUnmappedOrderLines(ESDocument esDocument)
        {
            List<KeyValuePair<int, int>> upmappedOrderLines = new List<KeyValuePair<int, int>>();
        
            //check that the ecommerce standards document's configs contains a key specifying the unmapped order lines
            if(esDocument.configs.ContainsKey(APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNMAPPED_LINES))
            {
                //get comma separated list of order record indicies and line indicies that indicate the unmapped order lines
                String unmappedOrderLineCSV = esDocument.configs[APIv1EndpointResponseESD<ESDocumentOrderSale>.ESD_CONFIG_ORDERS_WITH_UNMAPPED_LINES];

                //get the index of the order record and line that contained the unmapped product
                if(!String.IsNullOrWhiteSpace(unmappedOrderLineCSV.Trim())){
                    String[] unmappedOrderLineIndices = unmappedOrderLineCSV.Trim().Split(',');

                    //iterate through each order-line index
                    for(int i=0; i < unmappedOrderLineIndices.Length; i++){
                        //get order index and line index
                        String[] orderLineIndex = unmappedOrderLineIndices[i].Split(':');
                        if(orderLineIndex.Length == 2){
                            try{
                                int orderIndex = Convert.ToInt32(orderLineIndex[0]);
                                int lineIndex = Convert.ToInt32(orderLineIndex[1]);
                                KeyValuePair<int, int> orderLinePair = new KeyValuePair<int, int>(orderIndex, lineIndex);
                                upmappedOrderLines.Add(orderLinePair);

                            }catch(Exception ex){
                            }
                        }
                    }
                }
            }
        
            return upmappedOrderLines;
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
                if(!String.IsNullOrWhiteSpace(unpricedOrderLineCSV.Trim())){
                    String[] unmappedOrderLineIndices = unpricedOrderLineCSV.Trim().Split(',');

                    //iterate through each order-line index
                    for(int i=0; i < unmappedOrderLineIndices.Length; i++){
                        //get order index and line index
                        String[] orderLineIndex = unmappedOrderLineIndices[i].Split(':');
                        if(orderLineIndex.Length == 2){
                            try{
                                int orderIndex = Convert.ToInt32(orderLineIndex[0]);
                                int lineIndex = Convert.ToInt32(orderLineIndex[1]);
                                KeyValuePair<int, int> orderLinePair = new KeyValuePair<int, int>(orderIndex, lineIndex);
                                unpricedOrderLines.Add(orderLinePair);

                            }catch(Exception ex){
                            }
                        }
                    }
                }
            }
        
            return unpricedOrderLines;
        }
    }
}