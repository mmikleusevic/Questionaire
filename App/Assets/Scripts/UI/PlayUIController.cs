using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharedStandard.Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace UI
{
    public class PlayUIController : SafeArea
    {
        [FormerlySerializedAs("categoriesUIController")] [SerializeField]
        private GameSettingsUIController gameSettingsUIController;

        [SerializeField] private GameUIController gameUIController;
        private Button backButton;
        private Button gameSettingsButton;
        private Button playMultipleChoiceButton;
        private Button playSingleAnswerButton;

        private VisualElement playUI;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            playUI = root.Q("playUI");
            playSingleAnswerButton = root.Q<Button>("playSingleAnswerButton");
            playMultipleChoiceButton = root.Q<Button>("playMultipleChoiceButton");
            gameSettingsButton = root.Q<Button>("gameSettingsButton");
            backButton = root.Q<Button>("backButton");

            Hide();

            if (playMultipleChoiceButton != null) playMultipleChoiceButton.clicked += PlayMultipleChoiceClicked;
            if (playSingleAnswerButton != null) playSingleAnswerButton.clicked += PlaySingleAnswerClicked;
            if (gameSettingsButton != null) gameSettingsButton.clicked += OpenGameSettings;
            if (backButton != null) backButton.clicked += Hide;
        }

        private void OnDestroy()
        {
            if (playMultipleChoiceButton != null) playMultipleChoiceButton.clicked -= PlayMultipleChoiceClicked;
            if (playSingleAnswerButton != null) playSingleAnswerButton.clicked -= PlaySingleAnswerClicked;
            if (gameSettingsButton != null) gameSettingsButton.clicked -= OpenGameSettings;
            if (backButton != null) backButton.clicked -= Hide;
        }

        private void PlayMultipleChoiceClicked()
        {
            StartCoroutine(PlayQuestionaire(false));
        }

        private void PlaySingleAnswerClicked()
        {
            StartCoroutine(PlayQuestionaire(true));
        }

        private void OpenGameSettings()
        {
            StartCoroutine(gameSettingsUIController.OpenGameSettings());
        }

        private IEnumerator PlayQuestionaire(bool isSingleAnswerMode)
        {
            List<string> errorMessages = new List<string>();
            HashSet<int> selectedCategoryIds = new HashSet<int>();
            yield return StartCoroutine(
                gameSettingsUIController.GetSelectedCategoryIdsCoroutine(value =>
                    selectedCategoryIds = value.ToHashSet()));
            HashSet<Difficulty> selectedDifficulties = gameSettingsUIController.GetSelectedDifficulties();

            if (selectedCategoryIds == null || selectedCategoryIds.Count == 0)
            {
                errorMessages.Add("You have to select at least one category!");
            }

            if (selectedDifficulties == null || selectedDifficulties.Count == 0)
            {
                errorMessages.Add("You have to select at least one difficulty!");
            }

            if (errorMessages.Any())
            {
                string combinedErrorMessage = string.Join(Environment.NewLine, errorMessages);

                ErrorModalUIController.Instance.ShowMessage(combinedErrorMessage);
                yield break;
            }

            yield return StartCoroutine(
                gameUIController.LoadQuestions(selectedCategoryIds, selectedDifficulties, isSingleAnswerMode));
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