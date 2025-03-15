using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharedStandard.Models;
using UnityEngine.UIElements;

namespace UI
{
    public class GameUIController : SafeArea
    {
        private Label answer1Text;
        private Label answer2Text;
        private Label answer3Text;

        private Label[] answerTexts;
        private List<int> currentCategoryIds;
        private int currentQuestionIndex;
        private Button exitButton;
        private VisualElement gameUI;
        private bool isSingleAnswerMode;
        private Button nextButton;
        private Button previousButton;
        private List<QuestionDto> questions;
        private Label questionText;

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

            questions = new List<QuestionDto>();
            currentCategoryIds = new List<int>();
            answerTexts = new[] { answer1Text, answer2Text, answer3Text };

            Hide();

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

        public IEnumerator LoadQuestions(List<int> categories, bool isSingleAnswerMode)
        {
            this.isSingleAnswerMode = isSingleAnswerMode;
            LoadingUIController.Instance.ShowLoadingMessage("Loading Questions...");

            int numberOfQuestionsToFetch = 40;
            if (currentCategoryIds.SequenceEqual(categories))
            {
                int numberOfUnreadQuestions = questions.Count(q => !q.isRead);
                numberOfQuestionsToFetch -= numberOfUnreadQuestions;

                questions.RemoveAll(q => q.isRead);
            }
            else
            {
                questions.Clear();
            }

            yield return StartCoroutine(GameManager.Instance.GetUniqueQuestions(numberOfQuestionsToFetch, categories,
                this.isSingleAnswerMode, (retrievedQuestions, message) =>
                {
                    LoadingUIController.Instance.Hide();

                    if (retrievedQuestions != null)
                    {
                        currentCategoryIds.Clear();
                        currentCategoryIds.AddRange(categories);
                        questions.AddRange(retrievedQuestions);
                        currentQuestionIndex = 0;

                        Show();

                        ShowAnswers(isSingleAnswerMode ? new[] { false, true, false } : new[] { true, true, true });
                        SetNavigationButtons();
                        ShowQuestion();
                    }
                    else
                    {
                        ErrorModalUIController.Instance.ShowMessage(message);
                    }
                }));
        }

        private void ShowAnswers(bool[] showAnswers)
        {
            for (int i = 0; i < answerTexts.Length; i++)
            {
                answerTexts[i].visible = showAnswers[i];
            }
        }

        private void SetNavigationButtons()
        {
            previousButton.visible = currentQuestionIndex > 0;
            nextButton.visible = currentQuestionIndex < questions.Count - 1;
        }

        private void ShowQuestion()
        {
            if (questions == null || questions.Count == 0) return;

            QuestionDto question = questions[currentQuestionIndex];
            question.isRead = true;
            questionText.text = question.QuestionText;

            if (isSingleAnswerMode)
            {
                AnswerDto correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);

                if (correctAnswer == null) return;

                answer2Text.text = correctAnswer.AnswerText;
                answer2Text.AddToClassList("correctAnswerBackground");
            }
            else
            {
                for (int i = 0; i < question.Answers.Count; i++)
                {
                    answerTexts[i].text = question.Answers[i].AnswerText;

                    answerTexts[i].AddToClassList(question.Answers[i].IsCorrect
                        ? "correctAnswerBackground"
                        : "incorrectAnswerBackground");
                }
            }
        }

        private void PreviousQuestion()
        {
            currentQuestionIndex--;
            RemoveCorrectAnswerBackground();
            SetNavigationButtons();
            ShowQuestion();
        }

        private void NextQuestion()
        {
            currentQuestionIndex++;
            RemoveCorrectAnswerBackground();
            SetNavigationButtons();
            ShowQuestion();
        }

        private void RemoveCorrectAnswerBackground()
        {
            foreach (var answer in answerTexts)
            {
                answer.RemoveFromClassList("correctAnswerBackground");
                answer.RemoveFromClassList("incorrectAnswerBackground");
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
}