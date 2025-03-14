using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharedStandard.Models;
using UI.CustomUIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class CategoriesUIController : SafeArea
    {
        [SerializeField] private GameUIController gameUIController;
        private Button backButton;

        private List<CategoryDto> categories;
        private VisualElement categoriesPart;
        private VisualElement categoriesUI;
        private Button deSelectAllButton;
        private Action deSelectAllHandler;
        private ListView list;
        private float savedScrollPosition;
        private ScrollView scrollView;
        private Button selectAllButton;

        private Action selectAllHandler;
        private Dictionary<SlideToggle, EventCallback<ChangeEvent<bool>>> valueChangedHandlers;

        private void Start()
        {
            valueChangedHandlers = new Dictionary<SlideToggle, EventCallback<ChangeEvent<bool>>>();

            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            categoriesUI = root.Q("categoriesUI");
            categoriesPart = root.Q<VisualElement>("categoriesPart");
            selectAllButton = root.Q<Button>("selectAllButton");
            deSelectAllButton = root.Q<Button>("deSelectAllButton");
            backButton = root.Q<Button>("backButton");
            list = categoriesUI.Q<ListView>("list");
            scrollView = categoriesUI.Q<ScrollView>();

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
                toggleVisualElement.userData is CategoryDto category)
            {
                category.isSelected = evt.newValue;
            }
        }

        public IEnumerator GetCategories()
        {
            if (categories != null) yield break;

            LoadingUIController.Instance.ShowLoadingMessage("Loading Categories...");

            yield return StartCoroutine(GameManager.Instance.GetCategories((response, message) =>
            {
                LoadingUIController.Instance.Hide();

                if (response != null)
                {
                    categories = response;

                    SetCategoryToggles();
                }
                else
                {
                    ErrorModalUIController.Instance.ShowMessage(message);
                }
            }));
        }

        private void SetCategoryToggles()
        {
            list.itemsSource = categories;
            list.makeItem = () => new SlideToggle();
            list.bindItem = (element, index) =>
            {
                if (element is not SlideToggle slideToggle) return;

                CategoryDto category = categories[index];
                CreateCategoryToggle(category, slideToggle);
            };

            list.Rebuild();
        }

        private void CreateCategoryToggle(CategoryDto category, SlideToggle element)
        {
            element.userData = category;
            element.Q<Label>("textLabel").text = category.CategoryName;
            element.value = category.isSelected;

            var handler = new EventCallback<ChangeEvent<bool>>(OnCategoryValueChanged);
            valueChangedHandlers[element] = handler;
            element.RegisterValueChangedCallback(handler);

            foreach (CategoryDto child in category.ChildCategories)
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
            return categories?
                .SelectMany(GetSelectedCategoriesRecursive)
                .Where(c => c.isSelected)
                .Select(c => c.Id)
                .ToList();
        }

        private IEnumerable<CategoryDto> GetSelectedCategoriesRecursive(CategoryDto category)
        {
            return new[] { category }
                .Concat(category.ChildCategories?
                    .SelectMany(GetSelectedCategoriesRecursive) ?? Enumerable.Empty<CategoryDto>());
        }

        private void RestoreScrollPosition()
        {
            scrollView.verticalScroller.slider.value = savedScrollPosition;
        }

        private void SaveScrollPosition()
        {
            savedScrollPosition = scrollView.verticalScroller.slider.value;
        }

        private void SubscribeToGeometryChange()
        {
            categoriesUI.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            categoriesUI.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            categoriesUI.schedule.Execute(RestoreScrollPosition);
            categoriesUI.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void Show()
        {
            categoriesUI.style.display = DisplayStyle.Flex;

            SubscribeToGeometryChange();
        }

        private void Hide()
        {
            SaveScrollPosition();
            categoriesUI.style.display = DisplayStyle.None;
        }
    }
}