using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultNamespace.Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace DefaultNamespace
{
    public class QuestionServiceHandler : MonoBehaviour
    {
        [SerializeField] private string baseUrl;
        
        public IEnumerator GetRandomUniqueQuestions(string userId, int numberOfQuestions, Action<List<Question>> onComplete)
        {
            string url = $"{baseUrl}{userId}/{numberOfQuestions}";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    onComplete?.Invoke(null);
                    yield break;
                }
        
                try 
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    List<Question> questions = JsonConvert.DeserializeObject<List<Question>>(jsonResponse);
                    onComplete?.Invoke(questions);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Parsing error: {ex.Message}");
                    onComplete?.Invoke(null);
                }
            }
        }
    }
}