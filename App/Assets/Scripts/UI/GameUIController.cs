using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class GameUIController : SafeArea
    {
        [SerializeField] private ErrorModalUIController errorModalUIController;
        [SerializeField] private LoadingUIController loadingUIController;
    
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
        private bool isDirectMode;
    
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
        
            questions = new List<Question>();
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

        private void ExitPressed() => Hide();

        private void PreviousPressed() => PreviousQuestion();
    
        private void NextPressed() => NextQuestion();
    
        private void LoadQuestions(bool directMode)
        {
            loadingUIController.Show();
            int numberOfQuestions = 40 - questions.Count(q => !q.isRead);
        
            StartCoroutine(GameManager.Instance.GetUniqueQuestions(numberOfQuestions, (retrievedQuestions, message) =>
            {
                loadingUIController.Hide();
            
                if (retrievedQuestions != null)
                {
                    questions.RemoveAll(q => q.isRead);
                    questions.AddRange(retrievedQuestions);
                
                    currentQuestionIndex = 0;
                    isDirectMode = directMode;
        
                    Show();

                    ShowAnswers(directMode ? new[] { false, true, false } : new[] { true, true, true });
                    SetNavigationButtons();
                    ShowQuestion();  
                }
                else
                {
                    errorModalUIController.Show(message);
                }
            }));
        }

        public void ShowDirect() => LoadQuestions(true);

        public void ShowOptions() => LoadQuestions(false);

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
        
            Question question = questions[currentQuestionIndex];
            question.isRead = true;
            questionText.text = question.QuestionText;

            if (isDirectMode)
            {
                Answer correctAnswer = question.Answers.FirstOrDefault(a => a.isCorrect);

                if (correctAnswer == null) return;
            
                answer2Text.text = correctAnswer.AnswerText;
                answer2Text.AddToClassList("correctAnswerBackground");
            }
            else
            {
                for (int i = 0; i < answerTexts.Length; i++)
                {
                    answerTexts[i].text = question.Answers[i].AnswerText;

                    answerTexts[i].AddToClassList(question.Answers[i].isCorrect
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
