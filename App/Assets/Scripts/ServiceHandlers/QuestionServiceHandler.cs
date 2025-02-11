using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace ServiceHandlers
{
    public class QuestionServiceHandler
    {
        public IEnumerator GetRandomUniqueQuestions(QuestionRequest request, Action<List<Question>, string> onComplete)
        {
            string endpoint = "api/Question/random";
            string url = EnvironmentConfig.ApiBaseUrl + endpoint;

            string jsonRequestData = JsonConvert.SerializeObject(request);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                //bypassing validation
                webRequest.certificateHandler = new BypassCertificate();
                
                // webRequest.certificateHandler = new CustomCertificateHandler(EnvironmentConfig.CertificateThumbprint);
                
                byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonRequestData);
                webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

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
                    List<Question> questions = JsonConvert.DeserializeObject<List<Question>>(jsonResponse);
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
