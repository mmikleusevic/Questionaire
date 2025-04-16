using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharedStandard.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class GameUIController : SafeArea
    {
        [SerializeField] private GameSettingsUIController gameSettingsUIController;
        
        private Label answer1Text;
        private Label answer2Text;
        private Label answer3Text;

        private Label[] answerTexts;
        private int currentQuestionIndex;
        private Button exitButton;
        private VisualElement gameUI;
        private bool isSingleAnswerMode;
        private Button nextButton;
        private Button previousButton;
        private Button showHideButton;
        private List<QuestionDto> questions;
        private Label questionText;
        private bool showAnswerOrStyling;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            gameUI = root.Q<VisualElement>("gameUI");
            questionText = root.Q<Label>("questionText");
            answer1Text = root.Q<Label>("answer1Text");
            answer2Text = root.Q<Label>("answer2Text");
            answer3Text = root.Q<Label>("answer3Text");
            showHideButton = root.Q<Button>("showHideButton");
            exitButton = root.Q<Button>("exitButton");
            previousButton = root.Q<Button>("previousButton");
            nextButton = root.Q<Button>("nextButton");

            questions = new List<QuestionDto>();
            answerTexts = new[] { answer1Text, answer2Text, answer3Text };

            Hide();

            if (showHideButton != null) showHideButton.clicked += ToggleAnswerStyle;
            if (exitButton != null) exitButton.clicked += ExitPressed;
            if (previousButton != null) previousButton.clicked += PreviousPressed;
            if (nextButton != null) nextButton.clicked += NextPressed;
        }

        private void OnDestroy()
        {
            if (showHideButton != null) showHideButton.clicked -= ToggleAnswerStyle;
            if (exitButton != null) exitButton.clicked -= ExitPressed;
            if (previousButton != null) previousButton.clicked -= PreviousPressed;
            if (nextButton != null) nextButton.clicked -= NextPressed;
        }

        private void ExitPressed() => StartCoroutine(CreateUserQuestionHistory());
        private void PreviousPressed() => PreviousQuestion();
        private void NextPressed() => NextQuestion();

        public IEnumerator LoadQuestions(HashSet<int> categories, HashSet<Difficulty> difficulties,
            bool isSingleAnswerMode)
        {
            this.isSingleAnswerMode = isSingleAnswerMode;
            LoadingUIController.Instance.ShowLoadingMessage("Loading Questions...");

            yield return StartCoroutine(GameManager.Instance.GetUniqueQuestions(this.isSingleAnswerMode, questions,
                categories,
                difficulties, (retrievedQuestions, message) =>
                {
                    LoadingUIController.Instance.Hide();

                    if (retrievedQuestions != null)
                    {
                        questions.AddRange(retrievedQuestions);
                        currentQuestionIndex = 0;

                        Show();

                        SetNavigationButtons();
                        ShowQuestion();
                    }
                    else
                    {
                        ErrorModalUIController.Instance.ShowMessage(message);
                    }
                }));
        }

        private void SetNavigationButtons()
        {
            previousButton.SetEnabled(currentQuestionIndex > 0);
            nextButton.SetEnabled(currentQuestionIndex < questions.Count - 1);
        }

        private void ShowQuestion()
        {
            if (questions == null || questions.Count == 0) return;

            QuestionDto question = questions[currentQuestionIndex];
            question.isRead = true;
            questionText.text = question.QuestionText;

            bool hasOneAnswer = question.Answers.Count == 1;

            if (isSingleAnswerMode || hasOneAnswer)
            {
                AnswerDto correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);

                if (correctAnswer == null) return;

                answer2Text.text = correctAnswer.AnswerText;
                answer2Text.AddToClassList("correctAnswerBackground");
                ToggleAnswerStyle(answer2Text);
            }
            else
            {
                for (int i = 0; i < question.Answers.Count; i++)
                {
                    Label answerLabel = answerTexts[i];
                    answerLabel.userData = question.Answers[i];
                    answerLabel.text = question.Answers[i].AnswerText;
                    answerLabel.style.opacity = 1;
                    ToggleAnswerStyle(answerLabel);
                }
            }
        }

        private void PreviousQuestion()
        {
            currentQuestionIndex--;
            ResetAnswers();
            SetNavigationButtons();
            ShowQuestion();
        }

        private void NextQuestion()
        {
            currentQuestionIndex++;
            ResetAnswers();
            SetNavigationButtons();
            ShowQuestion();
        }

        private IEnumerator CreateUserQuestionHistory()
        {
            List<int> questionIds = questions.Where(q => q.isRead).Select(q => q.Id).ToList();

            yield return StartCoroutine(GameManager.Instance.CreateUserQuestionHistory(questionIds, message =>
            {
                if (string.IsNullOrEmpty(message))
                {
                    Hide();
                }
                else
                {
                    ErrorModalUIController.Instance.ShowMessage(message);
                }
            }));
        }

        private void ResetAnswers()
        {
            foreach (Label answerLabel in answerTexts)
            {
                answerLabel.text = string.Empty;

                ResetAnswerClass(answerLabel);
            }
        }

        private void ResetAnswerClass(Label answerLabel)
        {
            answerLabel.RemoveFromClassList("correctAnswerBackground");
            answerLabel.RemoveFromClassList("incorrectAnswerBackground");
        }

        private void ToggleAnswerStyle()
        {
            showAnswerOrStyling = !showAnswerOrStyling;
            UpdateShowHideState(showAnswerOrStyling);

            if (isSingleAnswerMode)
            {
                ToggleAnswerStyle(answer2Text);
            }
            else
            {
                foreach (Label answerLabel in answerTexts)
                {
                    ToggleAnswerStyle(answerLabel);
                }
            }
        }

        private void ToggleAnswerStyle(Label answerLabel)
        {
            if (isSingleAnswerMode)
            {
                answerLabel.style.opacity = showAnswerOrStyling ? 1 : 0;
            }
            else
            {
                AnswerDto answer = answerLabel.userData as AnswerDto;

                if (answer == null) return;

                if (!showAnswerOrStyling)
                {
                    ResetAnswerClass(answerLabel);
                }
                else
                {
                    answerLabel.AddToClassList(answer.IsCorrect
                        ? "correctAnswerBackground"
                        : "incorrectAnswerBackground");
                }
            }
        }

        private void HideAnswers()
        {
            foreach (Label answerLabel in answerTexts)
            {
                answerLabel.style.opacity = 0;
            }
        }

        private void UpdateShowHideState(bool value)
        {
            showAnswerOrStyling = value;
            showHideButton.text = value ? "Hide" : "Show";
        }

        private void Show()
        {
            UpdateShowHideState(false);
            gameUI.style.display = DisplayStyle.Flex;
        }

        private void Hide()
        {
            UpdateShowHideState(true);
            HideAnswers();
            ResetAnswers();
            gameUI.style.display = DisplayStyle.None;
        }
    }
}