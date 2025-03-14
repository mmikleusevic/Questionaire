using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SharedStandard.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace ServiceHandlers
{
    public class CategoryServiceHandler
    {
        public IEnumerator GetCategories(Action<List<CategoryDto>, string> onComplete)
        {
            string endpoint = "api/Category/nested";
            string url = EnvironmentConfig.ApiBaseUrl + endpoint;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                //bypassing validation
                webRequest.certificateHandler = new BypassCertificate();

                //webRequest.certificateHandler = new CustomCertificateHandler(EnvironmentConfig.CertificateThumbprint);

                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    string errorMessage = webRequest.downloadHandler.text;
                    errorMessage = Helper.StripQuotationMarks(errorMessage);

                    if (string.IsNullOrEmpty(errorMessage))
                        errorMessage = "An error has occurred fetching categories. Try again later.";

                    Debug.LogError($"Error: {errorMessage}");
                    onComplete?.Invoke(null, errorMessage);
                    yield break;
                }

                try
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    List<CategoryDto> questions = JsonConvert.DeserializeObject<List<CategoryDto>>(jsonResponse,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
                    onComplete?.Invoke(questions, string.Empty);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Parsing error: {ex.Message}");
                    onComplete?.Invoke(null, ex.Message);
                }
            }
        }
    }
}