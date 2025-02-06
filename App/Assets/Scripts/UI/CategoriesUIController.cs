using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class CategoriesUIController : SafeArea
    {
        [SerializeField] private GameUIController gameUIController;
        [SerializeField] private ErrorModalUIController errorModalUIController;
        [SerializeField] private LoadingUIController loadingUIController;
        
        private VisualElement categoriesUI;
        private VisualElement categoriesPart;
        private Button selectAllButton;
        private Button backButton;
        private Button getQuestionsButton;

        private List<Category> categories;
        
        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            categoriesUI = root.Q("categoriesUI");
            categoriesPart = root.Q<VisualElement>("categoriesPart");
            selectAllButton = root.Q<Button>("selectAllButton");
            backButton = root.Q<Button>("backButton");
            getQuestionsButton = root.Q<Button>("getQuestionsButton");
            
            Hide();

            if (selectAllButton != null) selectAllButton.clicked += SelectAllCategories;
            if (getQuestionsButton != null) getQuestionsButton.clicked += LoadQuestions;
            if (backButton != null) backButton.clicked += Hide;
        }

        private void OnDestroy()
        {
            if (selectAllButton != null) selectAllButton.clicked -= SelectAllCategories;
            if (getQuestionsButton != null) getQuestionsButton.clicked -= LoadQuestions;
            if (backButton != null) backButton.clicked -= Hide;

            for (int i = 0; i < categoriesPart.childCount; i++)
            {
                categoriesPart[i].UnregisterCallback<ChangeEvent<bool>>(OnCategoryChanged);
            }
        }

        public IEnumerator OpenCategories(bool isDirectMode)
        {
            gameUIController.SetIsDirectMode(isDirectMode);
            
            yield return StartCoroutine(LoadCategoriesData());

            if (categories != null) Show();
        }

        private IEnumerator LoadCategoriesData()
        {
            if (categories != null) yield break;
            
            yield return StartCoroutine(GetCategories());
        }

        private void OnCategoryChanged(ChangeEvent<bool> evt)
        {
            VisualElement toggleVisualElement = evt.currentTarget as VisualElement;
            
            Category category = toggleVisualElement.userData as Category;
            category.isUsed = !category.isUsed;
        }

        private IEnumerator GetCategories()
        {
            loadingUIController.ShowLoadingMessage("Loading Categories...");
            
            yield return StartCoroutine(GameManager.Instance.GetCategories((response, message) =>
            {
                loadingUIController.Hide();
                
                if (response != null)
                {
                    categories = response;

                    SetCategoryToggles();
                }
                else
                {
                    errorModalUIController.ShowMessage(message);
                }
            }));
        }

        private void SetCategoryToggles()
        {
            VisualTreeAsset categoryTemplateAsset = Resources.Load<VisualTreeAsset>("ToggleCategoryTemplate");
                
            foreach (Category category in categories)
            {
                VisualElement template = categoryTemplateAsset.CloneTree();
                VisualElement categoryTemplate = template.Children().First();
                categoriesPart.Add(categoryTemplate);
                    
                categoryTemplate.userData = category;
                categoryTemplate.Q<Label>().text = category.CategoryName;
                categoryTemplate.Q<Toggle>().value = true;
                    
                categoryTemplate.RegisterCallback<ChangeEvent<bool>>(OnCategoryChanged);
            }
        }

        private void SelectAllCategories()
        {
            for (int i = 0; i < categoriesPart.childCount; i++)
            {
                Toggle categoryToggle = categoriesPart[i] as Toggle;

                if (categoryToggle.value) continue;
                
                categoryToggle.value = true;
            }
        }

        private void LoadQuestions()
        {
            List<int> categoryIds = categories.Where(a => a.isUsed).Select(a => a.Id).ToList();

            if (categoryIds.Count == 0)
            {
                errorModalUIController.ShowMessage("You have to select at least one category!");
            }
            else
            {
                StartCoroutine(gameUIController.LoadQuestions(categoryIds));
                Hide();
            }
        }

        private void Show()
        {
            categoriesUI.style.display = DisplayStyle.Flex;
        }
        
        private void Hide()
        {
            categoriesUI.style.display = DisplayStyle.None;
        }
    }
}