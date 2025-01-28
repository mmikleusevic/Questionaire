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
        [SerializeField] private LoadingUIController loadingUIController;
        
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
            loadingUIController.Show();
            
            StartCoroutine( GameManager.Instance.GetUniqueQuestions((questions, message) =>
            {
                loadingUIController.Hide();
                
                if (questions != null)
                {
                    gameUIController.ShowOptions(questions);
                }
                else
                {
                    errorModalUIController.Show(message);
                }
            }));
        }
        
        private void PlayDirectClicked()
        {
            loadingUIController.Show();
            
            StartCoroutine(GameManager.Instance.GetUniqueQuestions((questions, message) =>
            {
                loadingUIController.Hide();
                
                if (questions != null)
                {
                    gameUIController.ShowDirect(questions);
                }
                else
                {
                    errorModalUIController.Show(message);
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