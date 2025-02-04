using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class PlayUIController : SafeArea
    {
        [SerializeField] private CategoriesUIController categoriesUIController;
        
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
            
            Hide();

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
            
            categoriesUIController.OpenCategories(false);
        }
        
        private void PlayDirectClicked()
        {
            categoriesUIController.OpenCategories(true);
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