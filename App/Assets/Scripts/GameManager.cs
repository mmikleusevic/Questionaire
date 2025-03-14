using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using ServiceHandlers;
using SharedStandard.Models;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private CategoryServiceHandler categoryServiceHandler;

    private QuestionServiceHandler questionServiceHandler;

    private string uniqueID;
    public static GameManager Instance { get; private set; }

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

    public IEnumerator GetUniqueQuestions(int numberOfQuestions, List<int> categoryIds, bool isSingleAnswerMode,
        Action<List<Question>, string> onComplete)
    {
        UniqueQuestionRequestDto request = new UniqueQuestionRequestDto
        {
            UserId = uniqueID,
            NumberOfQuestions = numberOfQuestions,
            CategoryIds = categoryIds.ToArray(),
            IsSingleAnswerMode = isSingleAnswerMode
        };

        yield return StartCoroutine(questionServiceHandler.GetRandomUniqueQuestions(request,
            (questions, message) => onComplete?.Invoke(questions, message)));
    }

    public IEnumerator GetCategories(Action<List<CategoryDto>, string> onComplete)
    {
        yield return StartCoroutine(categoryServiceHandler.GetCategories((categories, message) =>
            onComplete?.Invoke(categories, message)));
    }
}