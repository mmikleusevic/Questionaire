using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using NUnit.Framework;
using ServiceHandlers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private QuestionServiceHandler questionServiceHandler;
    private CategoryServiceHandler categoryServiceHandler;

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
        
        questionServiceHandler = new QuestionServiceHandler();
        categoryServiceHandler = new CategoryServiceHandler();
        uniqueID = SystemInfo.deviceUniqueIdentifier;
    }

    public IEnumerator GetUniqueQuestions(int numberOfQuestions, List<int> categoryIds, bool isSingleAnswerMode , Action<List<Question>, string> onComplete)
    {
        QuestionRequest request = new QuestionRequest
        {
            UserId = uniqueID,
            NumberOfQuestions = numberOfQuestions,
            CategoryIds = categoryIds.ToArray(),
            IsSingleAnswerMode = isSingleAnswerMode
        };
        
        yield return StartCoroutine(questionServiceHandler.GetRandomUniqueQuestions(request,
            (questions, message) => onComplete?.Invoke(questions, message)  ));
    }

    public IEnumerator GetCategories(Action<List<Category>, string> onComplete)
    {
        yield return StartCoroutine((categoryServiceHandler.GetCategories((categories, message) =>
            onComplete?.Invoke(categories, message))));
    }
}
