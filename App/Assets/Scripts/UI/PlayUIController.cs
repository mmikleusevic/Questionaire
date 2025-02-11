using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class PlayUIController : SafeArea
    {
        [SerializeField] private CategoriesUIController categoriesUIController;
        [SerializeField] private GameUIController gameUIController;
        [SerializeField] private ErrorModalUIController errorModalUIController;
        
        private VisualElement playUI;
        private Button playSingleAnswerButton;
        private Button playMultipleChoiceButton;
        private Button categoriesButton;
        private Button backButton;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            playUI = root.Q("playUI");
            playSingleAnswerButton = root.Q<Button>("playSingleAnswerButton");
            playMultipleChoiceButton = root.Q<Button>("playMultipleChoiceButton");
            categoriesButton = root.Q<Button>("categoriesButton");
            backButton = root.Q<Button>("backButton");
            
            Hide();

            if (playMultipleChoiceButton != null) playMultipleChoiceButton.clicked += PlayMultipleChoiceClicked;
            if (playSingleAnswerButton != null) playSingleAnswerButton.clicked += PlaySingleAnswerClicked;
            if (categoriesButton != null) categoriesButton.clicked += OpenCategories;
            if (backButton != null) backButton.clicked += Hide;
        }

        private void OnDestroy()
        {
            if (playMultipleChoiceButton != null) playMultipleChoiceButton.clicked -= PlayMultipleChoiceClicked;
            if (playSingleAnswerButton != null) playSingleAnswerButton.clicked -= PlaySingleAnswerClicked;
            if (categoriesButton != null) categoriesButton.clicked -= OpenCategories;
            if (backButton != null) backButton.clicked -= Hide;
        }

        private void PlayMultipleChoiceClicked()
        {
            PlayQuestionaire(false);
        }
        
        private void PlaySingleAnswerClicked()
        {
            PlayQuestionaire(true);
        }
        
        private void OpenCategories()
        {
            StartCoroutine(categoriesUIController.OpenCategories());
        }

        private void PlayQuestionaire(bool isSingleAnswerMode)
        {
            List<int> selectedCategoryIds = categoriesUIController.GetSelectedCategoryIds();

            if (selectedCategoryIds == null || selectedCategoryIds.Count == 0)
            {
                errorModalUIController.ShowMessage("You have to select at least one category!");

                return;
            }
            
            StartCoroutine(gameUIController.LoadQuestions(selectedCategoryIds, isSingleAnswerMode));
            Hide();
        }

        public void LoadCategories()
        {
            StartCoroutine(categoriesUIController.GetCategories());
        }
        
        public void Show()
        {
            playUI.style.display = DisplayStyle.Flex;
        }

        private void Hide()
        {
            playUI.style.display = DisplayStyle.None;
        }
    }
}