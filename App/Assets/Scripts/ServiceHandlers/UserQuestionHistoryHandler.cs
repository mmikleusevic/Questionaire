using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using SharedStandard.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace ServiceHandlers
{
    public class UserQuestionHistoryHandler
    {
        public IEnumerator CreateUserQuestionHistory(UserQuestionHistoryDto userQuestionHistoryDto,
            Action<string> onComplete)
        {
            string endpoint = "api/UserQuestionHistory";
            string url = EnvironmentConfig.ApiBaseUrl + endpoint;

            string jsonRequestData = JsonConvert.SerializeObject(userQuestionHistoryDto);

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
                    onComplete?.Invoke(errorMessage);
                    yield break;
                }

                try
                {
                    onComplete?.Invoke(string.Empty);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Parsing error: {ex.Message}");
                    onComplete?.Invoke(ex.Message);
                }
            }
        }
    }
}