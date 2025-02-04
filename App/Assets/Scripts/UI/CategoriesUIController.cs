using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class CategoriesUIController : SafeArea
    {
        [SerializeField] private GameUIController gameUIController;
        [SerializeField] private ErrorModalUIController errorModalUIController;
        
        private VisualElement categoriesUI;
        private VisualElement categoriesPart;
        private Button getQuestionsButton;
        private Button backButton;

        private List<Category> categories;
        
        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            categoriesUI = root.Q("categoriesUI");
            categoriesPart = root.Q<VisualElement>("categoriesPart");
            getQuestionsButton = root.Q<Button>("getQuestionsButton");
            backButton = root.Q<Button>("backButton");
            
            Hide();

            StartCoroutine(SetCategories());

            if (getQuestionsButton != null) getQuestionsButton.clicked += LoadQuestions;
            if (backButton != null) backButton.clicked += Hide;
        }

        private void OnDestroy()
        {
            if (getQuestionsButton != null) getQuestionsButton.clicked -= LoadQuestions;
            if (backButton != null) backButton.clicked -= Hide;

            for (int i = 0; i < categoriesPart.childCount; i++)
            {
                categoriesPart[i].UnregisterCallback<ClickEvent>(OnCategorySelected);
            }
        }

        public void OpenCategories(bool isDirectMode)
        {
            gameUIController.SetIsDirectMode(isDirectMode);
            
            Show();
        }

        private IEnumerator SetCategories()
        {
            yield return StartCoroutine(GetCategories());

            if (categories != null)
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
                    
                    categoryTemplate.RegisterCallback<ClickEvent>(OnCategorySelected);
                }
            }
        }

        private void OnCategorySelected(ClickEvent evt)
        {
            VisualElement toggleVisualElement = evt.currentTarget as VisualElement;
            
            Category category = toggleVisualElement.userData as Category;
            category.isUsed = !category.isUsed;
        }

        private IEnumerator GetCategories()
        {
            yield return StartCoroutine(GameManager.Instance.GetCategories((response, message) =>
            {
                if (response != null)
                {
                    categories = response;
                }
                else
                {
                    errorModalUIController.Show(message);
                }
            }));
        }

        private void LoadQuestions()
        {
            int[] categoryIds = categories.Where(a => a.isUsed).Select(a => a.Id).ToArray();
            
            gameUIController.LoadQuestions(categoryIds);
            
            Hide();
        }

        private void Show()
        {
            if (categories == null)
            {
                errorModalUIController.Show("Error: Couldn't fetch categories!");
            }
            else
            {
                categoriesUI.style.display = DisplayStyle.Flex;
            }
        }
        
        private void Hide()
        {
            categoriesUI.style.display = DisplayStyle.None;
        }
    }
}