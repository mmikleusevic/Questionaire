using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class PlayUIController : MonoBehaviour
    {
        private VisualElement playUI;
        private Button playDirectButton;
        private Button playChooseButton;
        private Button backButton;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            playUI = root.Q("playUI");
            playDirectButton = root.Q<Button>("playDirectButton");
            playChooseButton = root.Q<Button>("playChooseButton");
            backButton = root.Q<Button>("backButton");

            if (playChooseButton != null) playChooseButton.clicked += PlayChooseClicked;
            if (playDirectButton != null) playDirectButton.clicked += PlayDirectClicked;
            if (backButton != null) backButton.clicked += BackClicked;
        }

        private void OnDestroy()
        {
            if (playChooseButton != null) playChooseButton.clicked -= PlayChooseClicked;
            if (playDirectButton != null) playDirectButton.clicked -= PlayDirectClicked;
            if (backButton != null) backButton.clicked -= BackClicked;
        }

        private void PlayChooseClicked()
        {
            
        }
        
        private void PlayDirectClicked()
        {
            
        }
        
        private void BackClicked()
        {
            Hide();
        }

        public void Show()
        {
            playUI.visible = true;
        }

        private void Hide()
        {
            playUI.visible = false;
        }
    }
}