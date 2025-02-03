using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class QuestionServiceHandler : MonoBehaviour
{
    public IEnumerator GetRandomUniqueQuestions(string userId, int numberOfQuestions, Action<List<Question>, string> onComplete)
    {
        string endpoint = "api/Question/";
        
        string url = $"{EnvironmentConfig.ApiBaseUrl + endpoint}{userId}/{numberOfQuestions}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                webRequest.result == UnityWebRequest.Result.ProtocolError)
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
