using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace ServiceHandlers
{
    public class CategoryServiceHandler
    {
        public IEnumerator GetCategories(Action<List<Category>, string> onComplete)
        {
            string endpoint = "api/Category";
            string url = EnvironmentConfig.ApiBaseUrl + endpoint;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                //bypassing validation
                webRequest.certificateHandler = new BypassCertificate();
                
                //webRequest.certificateHandler = new CustomCertificateHandler(EnvironmentConfig.CertificateThumbprint);
                
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    onComplete?.Invoke(null, webRequest.error);
                    yield break;
                }
    
                try 
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    List<Category> questions = JsonConvert.DeserializeObject<List<Category>>(jsonResponse);
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