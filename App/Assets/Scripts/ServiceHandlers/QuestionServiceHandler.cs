using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SharedStandard.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace ServiceHandlers
{
    public class QuestionServiceHandler
    {
        public IEnumerator GetRandomUniqueQuestions(UniqueQuestionRequestDto request,
            Action<List<QuestionDto>, string> onComplete)
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
                    string errorMessage = webRequest.downloadHandler.text;
                    errorMessage = Helper.StripQuotationMarks(errorMessage);

                    if (string.IsNullOrEmpty(errorMessage))
                        errorMessage = "An error has occurred fetching questions. Try again later.";

                    Debug.LogError($"Error: {errorMessage}");
                    onComplete?.Invoke(null, errorMessage);
                    yield break;
                }

                try
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    List<QuestionDto> questions = JsonConvert.DeserializeObject<List<QuestionDto>>(jsonResponse);
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