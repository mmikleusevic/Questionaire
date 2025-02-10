using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Models;
using UI.CustomUIElements;
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
        
        private Action selectAllHandler;
        private Action deSelectAllHandler;
        private Dictionary<SlideToggle, EventCallback<ChangeEvent<bool>>> valueChangedHandlers;
        private VisualElement categoriesUI;
        private VisualElement categoriesPart;
        private ListView list;
        private Button selectAllButton;
        private Button deSelectAllButton;
        private Button backButton;

        private List<Category> categories;
        
        private void Start()
        {
            valueChangedHandlers = new Dictionary<SlideToggle, EventCallback<ChangeEvent<bool>>>();
            
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            categoriesUI = root.Q("categoriesUI");
            categoriesPart = root.Q<VisualElement>("categoriesPart");
            selectAllButton = root.Q<Button>("selectAllButton");
            deSelectAllButton = root.Q<Button>("deSelectAllButton");
            backButton = root.Q<Button>("backButton");
            
            Hide();

            if (selectAllButton != null)
            {
                selectAllHandler += () => SetValueAllCategories(true);
                selectAllButton.clicked += selectAllHandler;
            }

            if (deSelectAllButton != null)
            {
                deSelectAllHandler += () => SetValueAllCategories(false);
                deSelectAllButton.clicked += deSelectAllHandler;
            }
            if (backButton != null) backButton.clicked += Hide;
        }

        private void OnDestroy()
        {
            if (selectAllButton != null) selectAllButton.clicked -= selectAllHandler;
            if (deSelectAllButton != null) deSelectAllButton.clicked -= deSelectAllHandler;
            if (backButton != null) backButton.clicked -= Hide;

            CleanupToggle();
        }
        
        private void CleanupToggle()
        {
            foreach (var (toggle, handler) in valueChangedHandlers)
            {
                toggle?.UnregisterValueChangedCallback(handler);
            }
    
            valueChangedHandlers.Clear();
        }

        public IEnumerator OpenCategories()
        {
            yield return StartCoroutine(LoadCategoriesData());

            if (categories == null) yield break;

            Show();
        }

        private IEnumerator LoadCategoriesData()
        {
            if (categories != null) yield break;
            
            yield return StartCoroutine(GetCategories());
        }

        private void OnCategoryValueChanged(ChangeEvent<bool> evt)
        {
            if (evt.currentTarget is VisualElement toggleVisualElement && 
                toggleVisualElement.userData is Category category)
            {
                category.isSelected = evt.newValue;
                Debug.Log(category.isSelected);
            }
        }

        public IEnumerator GetCategories()
        {
            if (categories != null) yield break;
            
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
            list = categoriesUI.Q<ListView>("list");
            list.itemsSource = categories;
            
            list.makeItem = () => new SlideToggle();
            list.bindItem = (element, index) =>
            {
                if (element is not SlideToggle slideToggle) return;
                
                Category category = categories[index];
                CreateCategoryToggle(category,slideToggle);
            };
            
            list.Rebuild();
        }

        private void CreateCategoryToggle(Category category, SlideToggle element)
        {
            element.userData = category;
            element.Q<Label>("textLabel").text = category.CategoryName;
            element.value = category.isSelected;

            var handler = new EventCallback<ChangeEvent<bool>>(OnCategoryValueChanged);
            valueChangedHandlers[element] = handler;
            element.RegisterValueChangedCallback(handler);

            foreach (Category child in category.ChildCategories)
            {
                SlideToggle childToggle = list.itemTemplate.CloneTree().Children().First() as SlideToggle;
                CreateCategoryToggle(child, childToggle);
                childToggle.AddToClassList("child");
                element.Add(childToggle);
            }
        }
        
        private void SetValueAllCategories(bool value)
        {
            for (int i = 0; i < list.itemsSource.Count; i++)
            {
                var element = list.GetRootElementForIndex(i);
                if (element is SlideToggle categoryToggle)
                {
                    SetCategorySelected(categoryToggle, value);
                }
            }
        }
        
        private void SetCategorySelected(SlideToggle toggle, bool value)
        {
            toggle.value = value;
            
            foreach (VisualElement child in toggle.Children())
            {
                if (child is SlideToggle childToggle)
                {
                    SetCategorySelected(childToggle, value);
                }
            }
        }
        
        public List<int> GetSelectedCategoryIds()
        {
            List<int> selectedCategoryIds = categories?.Where(a => a.isSelected).Select(a => a.Id).ToList();
            
            return selectedCategoryIds;
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