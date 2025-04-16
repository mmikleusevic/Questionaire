using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ServiceHandlers;
using SharedStandard.Models;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const int TargetNumberOfQuestions = 50;

    private CategoryServiceHandler categoryServiceHandler;
    [CanBeNull] private UniqueQuestionsRequestDto lastUniqueQuestionsRequestDto;
    private QuestionServiceHandler questionServiceHandler;

    private string deviceIdentifier;
    private UserQuestionHistoryHandler userQuestionHistoryHandler;
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
        userQuestionHistoryHandler = new UserQuestionHistoryHandler();
        categoryServiceHandler = new CategoryServiceHandler();

        deviceIdentifier = SystemInfo.deviceUniqueIdentifier;
    }

    public IEnumerator GetUniqueQuestions(bool isSingleAnswerMode,
        List<QuestionDto> questions,
        HashSet<int> categories,
        HashSet<Difficulty> difficulties,
        Action<List<QuestionDto>, string> onComplete)
    {
        UniqueQuestionsRequestDto uniqueQuestionsRequestDto =
            CreateUniqueQuestionRequest(questions, categories, difficulties, isSingleAnswerMode);

        yield return StartCoroutine(questionServiceHandler.GetRandomUniqueQuestions(uniqueQuestionsRequestDto,
            (questions, message) => onComplete?.Invoke(questions, message)));
    }

    public IEnumerator GetCategories(Action<List<CategoryDto>, string> onComplete)
    {
        yield return StartCoroutine(categoryServiceHandler.GetCategories((categories, message) =>
            onComplete?.Invoke(categories, message)));
    }

    public IEnumerator CreateUserQuestionHistory(List<int> questionIds, Action<string> onComplete)
    {
        UserQuestionHistoryDto userQuestionHistoryDto = new UserQuestionHistoryDto
        {
            UserId = deviceIdentifier,
            QuestionIds = questionIds
        };

        yield return StartCoroutine(userQuestionHistoryHandler.CreateUserQuestionHistory(userQuestionHistoryDto,
            message =>
                onComplete?.Invoke(message)));
    }

    private UniqueQuestionsRequestDto CreateUniqueQuestionRequest(List<QuestionDto> questions,
        HashSet<int> categories,
        HashSet<Difficulty> difficulties,
        bool isSingleAnswerMode)
    {
        bool criteriaChanged = true;
        if (lastUniqueQuestionsRequestDto != null)
        {
            HashSet<int> lastCategoryIds = lastUniqueQuestionsRequestDto.CategoryIds.ToHashSet();
            HashSet<Difficulty> lastDifficulties = lastUniqueQuestionsRequestDto.Difficulties.ToHashSet();

            bool categoriesSame = categories.SetEquals(lastCategoryIds);
            bool difficultiesSame = difficulties.SetEquals(lastDifficulties);
            criteriaChanged = !categoriesSame || !difficultiesSame;
        }

        int numberOfQuestionsToFetch = 0;

        if (criteriaChanged)
        {
            questions.Clear();
            numberOfQuestionsToFetch = TargetNumberOfQuestions;
        }
        else
        {
            questions.RemoveAll(q => q.isRead);

            int currentQuestionCount = questions.Count;
            numberOfQuestionsToFetch = TargetNumberOfQuestions - currentQuestionCount;
            numberOfQuestionsToFetch = Math.Max(0, numberOfQuestionsToFetch);
        }

        UniqueQuestionsRequestDto currentRequestDto = new UniqueQuestionsRequestDto
        {
            UserId = deviceIdentifier,
            NumberOfQuestions = numberOfQuestionsToFetch,
            Difficulties = difficulties.ToArray(),
            CategoryIds = categories.ToArray(),
            IsSingleAnswerMode = isSingleAnswerMode
        };

        lastUniqueQuestionsRequestDto = currentRequestDto;

        return currentRequestDto;
    }
}