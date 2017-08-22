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
using System.Resources;
using System.Reflection;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Web;
using Squizz.Platform.API.v1.endpoint;

//libraries to handle JSON data
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Globalization;

namespace Squizz.Platform.API.v1
{
    /**
     * A generic class that can be used to send HTTP requests to the platform's API and return HTTP a response as an endpoint object
     */
    public class APIv1HTTPRequest
    {
        public const string HTTP_HEADER_CONTENT_TYPE = "Content-Type";
        public const string HTTP_HEADER_CONTENT_TYPE_FORM_URL_ENCODED = "application/x-www-form-urlencoded";
        public const string HTTP_HEADER_CONTENT_TYPE_JSON = "application/json";
        public const string HTTP_HEADER_CONTENT_ENCODING = "Content-Encoding";
        public const string HTTP_HEADER_CONTENT_ENCODING_GZIP = "gzip";
        
        /// <summary>Sends a HTTP request with a specified URL, headers and optionally post data to the SQUIZZ.com platform's API. Parses JSON data returned into a HTTP response</summary>
        /// <param name="requestMethod">method to send the HTTP request with. Set to POST to push up data</param>
        /// <param name="endpointName">name of the endpoint in the platform's API to send the request to</param>
        /// <param name="endpointParams">list of parameters to append to the end of the request's URL</param>
        /// <param name="requestHeaders">list of key value pairs to add to the request's headers</param>
        /// <param name="endpointPostData">data to place in the body of the HTTP request and post up</param>
        /// <param name="timeoutMilliseconds">amount of milliseconds to wait before giving up waiting on receiving a response from the API. For larger amounts of data posted increase the timeout time</param>
        /// <param name="langResourceManager">language resource manager to use to return result messages in</param>
        /// <param name="languageLocale">language locale that specifies the language messages from the endpoint should be displayed in</param>
        /// <param name="endpointResponse">the response object that may be used to report the response from the server</param>
        /// <returns>type of endpoint response based on the type of endpoint being called.</returns>
        public static APIv1EndpointResponse sendHTTPRequest(string requestMethod, string endpointName, string endpointParams, List<KeyValuePair<string, string>> requestHeaders, string endpointPostData, int timeoutMilliseconds, ResourceManager langResourceManager, CultureInfo languageLocale, APIv1EndpointResponse endpointResponse)
        {
            //make a request to login to the API with the credentials
            HttpWebRequest webConnection = null;
            HttpWebResponse webResponse = null;
            StreamReader streamResponseReader = null;
            HttpStatusCode responseCode = 0;
            bool compressDataToGZIP = false;

            try
            {
                //create a new HTTP connection
                string serverAddress = APIv1Constants.API_PROTOCOL + APIv1Constants.API_DOMAIN + APIv1Constants.API_ORG_PATH + endpointName + "?" + endpointParams;
                webConnection = (HttpWebRequest)WebRequest.Create(serverAddress);
                webConnection.Method = requestMethod.ToUpper();
                webConnection.Timeout = timeoutMilliseconds;
                webConnection.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                //add the header properties to the request
                WebHeaderCollection webConnectionHeaders = webConnection.Headers;
                foreach (KeyValuePair<string, string> requestHeaderPair in requestHeaders) {

                    if(requestHeaderPair.Key.ToUpper() == HTTP_HEADER_CONTENT_TYPE.ToUpper()) {
                        webConnection.ContentType = requestHeaderPair.Value;
                    }
                    else{
                        webConnectionHeaders.Add(requestHeaderPair.Key, requestHeaderPair.Value);
                    }

                    //check if the request should gzip compress the data
                    if (requestHeaderPair.Key.Trim().ToUpper() == HTTP_HEADER_CONTENT_ENCODING.ToUpper() && requestHeaderPair.Value.ToUpper().Contains(HTTP_HEADER_CONTENT_ENCODING_GZIP.ToUpper())){
                        compressDataToGZIP = true;
                    }
                }

                //set the body of the request
                if (requestMethod.ToUpper() == APIv1Constants.HTTP_REQUEST_METHOD_POST) {
                    //write the post data to the request's data stream
                    using (StreamWriter requestStreamWriter = new StreamWriter(webConnection.GetRequestStream()))
                    {
                        // compress the post data into GZIP if allowed
                        if (compressDataToGZIP)
                        {
                            using (GZipStream gzipStream = new GZipStream(webConnection.GetRequestStream(), CompressionMode.Compress, false))
                            {
                                byte[] jsonDataBytes = Encoding.ASCII.GetBytes(endpointPostData);
                                gzipStream.Write(jsonDataBytes, 0, jsonDataBytes.Length);
                            }
                        }
                        else
                        {
                            //write the post data to the http request stream
                            requestStreamWriter.Write(endpointPostData);
                        }

                        //cloase the http request stream
                        requestStreamWriter.Flush();
                        requestStreamWriter.Close();
                    }

                }

                //send HTTP request
                webResponse = (HttpWebResponse)webConnection.GetResponse();

                //get the output of the HTTP response
                responseCode = webResponse.StatusCode;
                if (responseCode == HttpStatusCode.OK)
                {
                    //obtain the encoding returned by the server
                    String encoding = webResponse.ContentEncoding;

                    //The Content-Type can be used later to determine the nature of the content regardless of compression
                    String contentType = webConnection.ContentType;

                    //get the response from the server
                    streamResponseReader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8);
                    string responseBodyRaw = streamResponseReader.ReadToEnd();

                    //deserialize HTTP response from JSON into the endpoint response object
                    endpointResponse = JsonConvert.DeserializeObject<APIv1EndpointResponse>(responseBodyRaw);

                    //get the message that corresponds with the result code
                    if (!String.IsNullOrWhiteSpace(langResourceManager.GetString(endpointResponse.result_code, languageLocale))) {
                        endpointResponse.result_message = langResourceManager.GetString(endpointResponse.result_code);
                    }
                } else {
                    endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                    endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_RESPONSE;
                    endpointResponse.result_message = langResourceManager.GetString(endpointResponse.result_code, languageLocale) + "\n" + responseCode;
                }

            } catch (WebException e) {
                if(e.Status == WebExceptionStatus.ServerProtocolViolation)
                {
                    endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                    endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_REQUEST_PROTOCOL;
                    endpointResponse.result_message = langResourceManager.GetString(endpointResponse.result_code, languageLocale) + "\n" + e.Message;
                }
                else
                {
                    endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                    endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_CONNECTION;
                    endpointResponse.result_message = langResourceManager.GetString(endpointResponse.result_code, languageLocale) + "\n" + e.Message;
                }
            }catch (IOException e) {
                endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_IO;
                endpointResponse.result_message = langResourceManager.GetString(endpointResponse.result_code, languageLocale) + "\n" + e.Message;
            }
            catch (Exception e) {
                endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_UNKNOWN;
                endpointResponse.result_message = langResourceManager.GetString(endpointResponse.result_code, languageLocale) + "\n" + e.Message;
            }

            finally
            {
                //close the connection, set all objects to null
                if (webConnection != null) {
                    webConnection.Abort();
                    webConnection = null;
                }

                if (webResponse != null)
                {
                    webResponse.Close();
                    webResponse = null;
                }
                if (streamResponseReader != null)
                {
                    streamResponseReader.Close();
                    streamResponseReader = null;
                }
            }

            return endpointResponse;
        }

        /// <summary>
        ///     Sends a HTTP request with a specified URL, headers and data of a Ecommerce Standards Document to the SQUIZZ.com platform's API. 
        ///     Parses JSON data returned from a HTTP response into an Ecommerce Standards Document of a specified type
        ///     Note that data uploaded is compressed using GZIP
        /// </summary>
        /// <typeparam name="T">The type of Ecommerce Standards Document that is expected to be returned when calling a HTTP webservice</typeparam>
        /// <param name="requestMethod">method to send the HTTP request as, either GET or POST</param>
        /// <param name="endpointName">name of the endpoint in the platform's API to send the request to</param>
        /// <param name="endpointParams">list of parameters to append to the end of the request's URL</param>
        /// <param name="requestHeaders">list of key value pairs to add to the request's headers</param>
        /// <param name="postData">content to send in the body of the request, this text is ignored if esDocument is not null</param>
        /// <param name="esDocument">Ecommerce Standards Document containing the records and data to push up to the platform's API</param>
        /// <param name="timeoutMilliseconds">amount of milliseconds to wait before giving up waiting on receiving a response from the API. For larger amounts of data posted increase the timeout time</param>
        /// <param name="langResourceManager">language resource manager to use to return result messages in</param>
        /// <param name="languageLocale">language locale that specifies the language messages from the endpoint should be displayed in</param>
        /// <param name="endpointResponse">the response object that may be used to report the response from the server</param>
        /// <returns>type of endpoint response based on the type of endpoint being called, with the response containing the ESDocument</returns>
        public static APIv1EndpointResponseESD<T> sendESDocumentHTTPRequest<T>(String requestMethod, String endpointName, String endpointParams, List<KeyValuePair<String, String>> requestHeaders, String postData, ESDocument esDocument, int timeoutMilliseconds, ResourceManager langResourceManager, CultureInfo languageLocale, APIv1EndpointResponseESD<T> endpointResponse)
        {
            //make a request to login to the API with the credentials
            HttpWebRequest webConnection = null;
            HttpWebResponse webResponse = null;
            StreamReader streamResponseReader = null;
            HttpStatusCode responseCode = 0;
            bool compressDataToGZIP = false;

            try
            {
                //create a new HTTP connection
                string serverAddress = APIv1Constants.API_PROTOCOL + APIv1Constants.API_DOMAIN + APIv1Constants.API_ORG_PATH + endpointName + "?" + endpointParams;
                webConnection = (HttpWebRequest)WebRequest.Create(serverAddress);
                webConnection.Method = requestMethod.ToUpper();
                webConnection.Timeout = timeoutMilliseconds;
                webConnection.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                //add the header properties to the request
                WebHeaderCollection webConnectionHeaders = webConnection.Headers;
                foreach (KeyValuePair<string, string> requestHeaderPair in requestHeaders)
                {

                    if (requestHeaderPair.Key.ToUpper() == HTTP_HEADER_CONTENT_TYPE.ToUpper())
                    {
                        webConnection.ContentType = requestHeaderPair.Value;
                    }
                    else {
                        webConnectionHeaders.Add(requestHeaderPair.Key, requestHeaderPair.Value);
                    }

                    //check if the request should gzip compress the data
                    if (requestHeaderPair.Key.Trim().ToUpper() == HTTP_HEADER_CONTENT_ENCODING.ToUpper() && requestHeaderPair.Value.ToUpper().Contains(HTTP_HEADER_CONTENT_ENCODING_GZIP.ToUpper()))
                    {
                        compressDataToGZIP = true;
                    }
                }

                //set the body of the request
                if (requestMethod.ToUpper() == APIv1Constants.HTTP_REQUEST_METHOD_POST)
                {
                    //write the JSON to the request's data stream
                    using (StreamWriter requestStreamWriter = new StreamWriter(webConnection.GetRequestStream()))
                    {
                        // write uncompressed JSON data to memory
                        using (MemoryStream jsonStream = new MemoryStream())
                        using (StreamWriter streamWriter = new StreamWriter(jsonStream))
                        using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.NullValueHandling = NullValueHandling.Ignore;
                            serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
                            serializer.Serialize(jsonWriter, esDocument);
                            jsonWriter.Flush();

                            // compress the streamed JSON data into GZIP if allowed
                            if (compressDataToGZIP)
                            {
                                using (GZipStream gzipJSONStream = new GZipStream(webConnection.GetRequestStream(), CompressionMode.Compress, false))
                                {
                                    byte[] jsonDataBytes = jsonStream.ToArray();
                                    gzipJSONStream.Write(jsonDataBytes, 0, jsonDataBytes.Length);
                                }
                            }
                            else
                            {
                                //process all the JSON stream data into a string
                                StreamReader jsonStreamReader = new StreamReader(jsonStream, Encoding.UTF8);
                                string jsonString = Encoding.Default.GetString(jsonStream.ToArray());
                                jsonStreamReader.Close();
                                //string jsonString = jsonStreamReader.ReadToEnd();

                                //write the JSON data to the http request stream
                                requestStreamWriter.Write(jsonString);
                            }
                        }

                        //cloase the http request stream
                        requestStreamWriter.Flush();
                        requestStreamWriter.Close();
                    }
                }

                //send HTTP request
                webResponse = (HttpWebResponse)webConnection.GetResponse();

                //get the output of the HTTP response
                responseCode = webResponse.StatusCode;
                if (responseCode == HttpStatusCode.OK)
                {
                    //obtain the encoding returned by the server
                    String encoding = webResponse.ContentEncoding;

                    //The Content-Type can be used later to determine the nature of the content regardless of compression
                    String contentType = webConnection.ContentType;

                    //get the response from the server
                    streamResponseReader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8);
                    string responseBodyRaw = streamResponseReader.ReadToEnd();

                    //deserialize HTTP response from JSON into the endpoint response object
                    endpointResponse.esDocument = JsonConvert.DeserializeObject<T>(responseBodyRaw);
                    endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                }
                else {
                    endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                    endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_RESPONSE;
                    endpointResponse.result_message = langResourceManager.GetString(endpointResponse.result_code, languageLocale) + "\n" + responseCode;
                }

            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ServerProtocolViolation)
                {
                    endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                    endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_REQUEST_PROTOCOL;
                    endpointResponse.result_message = langResourceManager.GetString(endpointResponse.result_code, languageLocale) + "\n" + e.Message;
                }
                else
                {
                    endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                    endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_CONNECTION;
                    endpointResponse.result_message = langResourceManager.GetString(endpointResponse.result_code, languageLocale) + "\n" + e.Message;
                }
            }
            catch (IOException e)
            {
                endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_IO;
                endpointResponse.result_message = langResourceManager.GetString(endpointResponse.result_code, languageLocale) + "\n" + e.Message;
            }
            catch (Exception e)
            {
                endpointResponse.result = APIv1EndpointResponse.ENDPOINT_RESULT_FAILURE;
                endpointResponse.result_code = APIv1EndpointResponse.ENDPOINT_RESULT_CODE_ERROR_UNKNOWN;
                endpointResponse.result_message = langResourceManager.GetString(endpointResponse.result_code, languageLocale) + "\n" + e.Message;
            }

            finally
            {
                //close the connection, set all objects to null
                if (webConnection != null)
                {
                    webConnection.Abort();
                    webConnection = null;
                }

                if (webResponse != null)
                {
                    webResponse.Close();
                    webResponse = null;
                }
                if (streamResponseReader != null)
                {
                    streamResponseReader.Close();
                    streamResponseReader = null;
                }
            }

            return endpointResponse;
        }
    }
}
