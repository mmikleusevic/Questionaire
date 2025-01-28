using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUIController : MonoBehaviour
{
    private VisualElement gameUI;
    private Label questionText;
    private Label answer1Text;
    private Label answer2Text;
    private Label answer3Text;
    private Button exitButton;
    private Button previousButton;
    private Button nextButton;
    
    private Label[] answerTexts;
    private List<Question> questions;
    private int currentQuestionIndex;
    private bool isDirect;
    
    private void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        gameUI = root.Q<VisualElement>("gameUI");
        questionText = root.Q<Label>("questionText");
        answer1Text = root.Q<Label>("answer1Text");
        answer2Text = root.Q<Label>("answer2Text");
        answer3Text = root.Q<Label>("answer3Text");
        exitButton = root.Q<Button>("exitButton");
        previousButton = root.Q<Button>("previousButton");
        nextButton = root.Q<Button>("nextButton");

        answerTexts = new[] { answer1Text, answer2Text, answer3Text };
        
        if (exitButton != null) exitButton.clicked += ExitPressed;
        if (previousButton != null) previousButton.clicked += PreviousPressed;
        if (nextButton != null) nextButton.clicked += NextPressed;
    }

    private void OnDestroy()
    {
        if (exitButton != null) exitButton.clicked -= ExitPressed;
        if (previousButton != null) previousButton.clicked -= PreviousPressed;
        if (nextButton != null) nextButton.clicked -= NextPressed;
    }

    private void ExitPressed()
    {
        Hide();
    }
    
    private void PreviousPressed()
    {
        PreviousQuestion();
    }
    
    private void NextPressed()
    {
        NextQuestion();
    }

    public void ShowDirect(List<Question> questions)
    {
        this.questions = questions;
        currentQuestionIndex = 0;
        isDirect = true;
        
        Show();
        
        ShowAnswers(false, true, false);
        SetNavigationButtons();
        ShowQuestion();
    }

    public void ShowOptions(List<Question> questions)
    {
        this.questions = questions;
        currentQuestionIndex = 0;
        isDirect = false;
        
        Show();

        ShowAnswers(true, true, true);
        SetNavigationButtons();
        ShowQuestion();
    }

    private void ShowAnswers(bool showFirst, bool showSecond, bool showThird)
    {
        answer1Text.visible = showFirst;
        answer2Text.visible = showSecond;
        answer3Text.visible = showThird;
    }

    private void SetNavigationButtons()
    {
        if (currentQuestionIndex == 0)
        {
            previousButton.visible = false;
            nextButton.visible = true;
        }
        else if (currentQuestionIndex == questions.Count - 1)
        {
            previousButton.visible = true;
            nextButton.visible = false;
        }
        else
        {
            previousButton.visible = true;
            nextButton.visible = true;
        }
    }
    
    private void ShowQuestion()
    {
        Question question = questions[currentQuestionIndex];
        
        questionText.text = question.QuestionText;

        if (isDirect)
        {
            Answer answer = question.Answers.FirstOrDefault(a => a.isCorrect);
            answer2Text.text = answer.AnswerText;
            answer2Text.AddToClassList("correctAnswerBackground");
        }
        else
        {
            for (int i = 0; i < answerTexts.Length; i++)
            {
                answerTexts[i].text = question.Answers[i].AnswerText;
                if (question.Answers[i].isCorrect) answerTexts[i].AddToClassList("correctAnswerBackground");
            }
        }
    }

    private void PreviousQuestion()
    {
        currentQuestionIndex = currentQuestionIndex - 1;

        RemoveCorrectAnswerBackground();
        SetNavigationButtons();
        ShowQuestion();
    }

    private void NextQuestion()
    {
        currentQuestionIndex = currentQuestionIndex + 1;

        RemoveCorrectAnswerBackground();
        SetNavigationButtons();
        ShowQuestion();
    }

    private void RemoveCorrectAnswerBackground()
    {
        foreach (var answer in answerTexts)
        {
            answer.RemoveFromClassList("correctAnswerBackground");
        }
    }
    
    private void Show()
    {
        gameUI.style.display = DisplayStyle.Flex;
    }
    
    private void Hide()
    {
        RemoveCorrectAnswerBackground();
        
        gameUI.style.display = DisplayStyle.None;
    }
}
