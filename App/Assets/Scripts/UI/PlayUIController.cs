using System;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class PlayUIController : MonoBehaviour
    {
        [SerializeField] private GameUIController gameUIController;
        [SerializeField] private ErrorModalUIController errorModalUIController;
        
        private VisualElement playUI;
        private Button playDirectButton;
        private Button playOptionsButton;
        private Button backButton;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            playUI = root.Q("playUI");
            playDirectButton = root.Q<Button>("playDirectButton");
            playOptionsButton = root.Q<Button>("playOptionsButton");
            backButton = root.Q<Button>("backButton");

            if (playOptionsButton != null) playOptionsButton.clicked += PlayOptionsClicked;
            if (playDirectButton != null) playDirectButton.clicked += PlayDirectClicked;
            if (backButton != null) backButton.clicked += BackClicked;
        }

        private void OnDestroy()
        {
            if (playOptionsButton != null) playOptionsButton.clicked -= PlayOptionsClicked;
            if (playDirectButton != null) playDirectButton.clicked -= PlayDirectClicked;
            if (backButton != null) backButton.clicked -= BackClicked;
        }

        private void PlayOptionsClicked()
        {
            StartCoroutine(GameManager.Instance.GetUniqueQuestions(questions =>
            {
                //TODO: make it return error and show it
                if (questions != null)
                {
                    gameUIController.ShowOptions(questions);
                }
            }));
        }
        
        private void PlayDirectClicked()
        {
            StartCoroutine(GameManager.Instance.GetUniqueQuestions(questions =>
            {
                //TODO: make it return error and show it
                if (questions != null)
                {
                    gameUIController.ShowDirect(questions);
                }
            }));
        }
        
        private void BackClicked()
        {
            Hide();
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