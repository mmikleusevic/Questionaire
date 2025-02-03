using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using NUnit.Framework;
using UnityEngine;

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

    public IEnumerator GetUniqueQuestions(int numberOfQuestions, Action<List<Question>, string> onComplete)
    {
        yield return StartCoroutine(questionServiceHandler.GetRandomUniqueQuestions(uniqueID, numberOfQuestions, 
            (questions, message) => onComplete?.Invoke(questions, message)  ));
    }
}
