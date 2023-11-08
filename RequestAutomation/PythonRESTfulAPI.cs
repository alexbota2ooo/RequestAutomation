using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;
using RestSharp;
using Common.Interfaces;

namespace RequestAutomation
{
    public class PythonRESTfulAPI : IPythonRESTfulAPI
    {
        /// <summary>
        /// RestSharp call Python RESTful API for image classification with Base64 encode
        /// </summary>
        /// <param name="urlWebAPI">UIR web api link</param>
        /// <param name="imagePathName">Image path and name</param>
        /// <param name="exceptionMessage">Returned exception message</param>
        /// <returns>Response content string</returns>
        public string PythonDigitRecognizerAPI(string urlWebAPI, string imagePathName, out string exceptionMessage)
        {
            string base64String = string.Empty;
            string imageJsonString = string.Empty;
            exceptionMessage = string.Empty;
            string responseContent = string.Empty;
            try
            {
                base64String = ImageFileToBase64String(imagePathName);
                imageJsonString = BuildImageJsonString(base64String);
                RestRequest restRequest = new RestRequest(Method.POST);
                restRequest.AddHeader("content-type", "application/json");
                restRequest.AddParameter("application/json", imageJsonString, ParameterType.RequestBody);
                RestClient restClient = new RestClient(urlWebAPI);
                IRestResponse iRestResponse = restClient.Execute(restRequest);
                string errorMessage = iRestResponse.ErrorMessage;
                if (string.IsNullOrEmpty(errorMessage))
                {
                    responseContent = iRestResponse.Content;
                }
                else
                {
                    responseContent = errorMessage;
                }
            }
            catch (Exception ex)
            {
                exceptionMessage = $"An error occurred. {ex.Message}";
            }
            return responseContent;
        }

        /// <summary>
        /// Convert image file to base64 encoded string
        /// </summary>
        /// <param name="imagePathName">Image path and name</param>     
        /// <returns>Base64 encoded string</returns>
        private string ImageFileToBase64String(string imagePathName)
        {
            string base64String = string.Empty;
            try
            {
                using (Image image = Image.FromFile(imagePathName))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        image.Save(memoryStream, image.RawFormat);
                        byte[] imageBytes = memoryStream.ToArray();
                        base64String = Convert.ToBase64String(imageBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                string exceptionMessage = $"An error occurred. {ex.Message}";
            }
            return base64String;
        }

        /// <summary>
        /// Build image json string object
        /// </summary>
        /// <param name="base64String">Base64 encoded string</param>
        /// <returns>Image json string</returns>
        private string BuildImageJsonString(string base64String)
        {
            string imageJsonString = string.Empty;
            try
            {
                dynamic imageJson = new JObject();
                imageJson.content = base64String;
                imageJsonString = imageJson.ToString();
            }
            catch (Exception ex)
            {
                string exceptionMessage = $"An error occurred. {ex.Message}";
            }
            return imageJsonString;
        }
    }
}
