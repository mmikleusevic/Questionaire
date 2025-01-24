using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultNamespace.Models;
using NUnit.Framework;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private QuestionServiceHandler questionServiceHandler;

        private string uniqueID;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            uniqueID = SystemInfo.deviceUniqueIdentifier;
        }

        public IEnumerator GetUniqueQuestions(Action<List<Question>> onComplete)
        {
            yield return StartCoroutine(questionServiceHandler.GetRandomUniqueQuestions(uniqueID, 40, 
                (response) => onComplete?.Invoke(response)  ));
        }
    }
}