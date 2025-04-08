using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedStandard.Models;
using UI.CustomUIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class GameSettingsUIController : SafeArea
    {
        [SerializeField] private GameUIController gameUIController;
        private Button backButton;
        private ListView categorieslist;
        private Button deselectAllButton;
        private Action deselectAllHandler;
        private ListView difficultieslist;

        private VisualElement gameSettingsUI;
        private float savedScrollPosition;
        private ScrollView scrollView;
        private Button selectAllButton;

        private Action selectAllHandler;

        private List<CategoryDto> selectedCategories;
        private List<Difficulty> selectedDifficulties;
        private Dictionary<SlideToggle, EventCallback<ChangeEvent<bool>>> valueChangedHandlers;

        private void Start()
        {
            valueChangedHandlers = new Dictionary<SlideToggle, EventCallback<ChangeEvent<bool>>>();

            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            gameSettingsUI = root.Q("gameSettingsUI");
            selectAllButton = root.Q<Button>("selectAllButton");
            deselectAllButton = root.Q<Button>("deSelectAllButton");
            backButton = root.Q<Button>("backButton");
            categorieslist = gameSettingsUI.Q<ListView>("categoriesList");
            difficultieslist = gameSettingsUI.Q<ListView>("difficultiesList");
            scrollView = gameSettingsUI.Q<ScrollView>();

            SetDifficultyToggles();

            Hide();

            if (selectAllButton != null)
            {
                selectAllHandler += () => SetAllValues(true);
                selectAllButton.clicked += selectAllHandler;
            }

            if (deselectAllButton != null)
            {
                deselectAllHandler += () => SetAllValues(false);
                deselectAllButton.clicked += deselectAllHandler;
            }

            if (backButton != null) backButton.clicked += Hide;
        }

        private void OnDestroy()
        {
            if (selectAllButton != null) selectAllButton.clicked -= selectAllHandler;
            if (deselectAllButton != null) deselectAllButton.clicked -= deselectAllHandler;
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

        public IEnumerator OpenGameSettings()
        {
            yield return StartCoroutine(LoadCategoriesData());

            if (selectedCategories == null) yield break;

            Show();
        }

        private IEnumerator LoadCategoriesData()
        {
            if (selectedCategories != null) yield break;

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

        private void OnDifficultyValueChanged(ChangeEvent<bool> evt)
        {
            if (evt.currentTarget is not VisualElement toggleVisualElement ||
                toggleVisualElement.userData is not Difficulty difficulty) return;

            if (evt.newValue)
            {
                selectedDifficulties.Add(difficulty);
            }
            else
            {
                selectedDifficulties.Remove(difficulty);
            }
        }

        private IEnumerator GetCategories()
        {
            if (selectedCategories != null) yield break;

            LoadingUIController.Instance.ShowLoadingMessage("Loading Categories...");

            yield return StartCoroutine(GameManager.Instance.GetCategories((response, message) =>
            {
                LoadingUIController.Instance.Hide();

                if (response != null)
                {
                    selectedCategories = response;

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
            categorieslist.itemsSource = selectedCategories;
            categorieslist.makeItem = () => new SlideToggle();
            categorieslist.bindItem = (element, index) =>
            {
                if (element is not SlideToggle slideToggle) return;

                CategoryDto category = selectedCategories[index];
                CreateCategoryToggle(category, slideToggle);
            };

            categorieslist.Rebuild();
        }

        private void SetDifficultyToggles()
        {
            List<Difficulty> allDifficulties =
                new List<Difficulty>(Enum.GetValues(typeof(Difficulty)).Cast<Difficulty>());
            selectedDifficulties = allDifficulties.ToList();

            difficultieslist.itemsSource = allDifficulties;
            difficultieslist.makeItem = () => new SlideToggle();
            difficultieslist.bindItem = (visualElement, index) =>
            {
                if (visualElement is not SlideToggle slideToggle) return;

                SlideToggle element = visualElement as SlideToggle;

                Difficulty difficulty = allDifficulties[index];

                element.userData = difficulty;
                element.Q<Label>("textLabel").text = difficulty.ToString();
                element.value = true;

                var handler = new EventCallback<ChangeEvent<bool>>(OnDifficultyValueChanged);
                valueChangedHandlers[element] = handler;
                element.RegisterValueChangedCallback(handler);
            };

            difficultieslist.Rebuild();
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
                SlideToggle childToggle = categorieslist.itemTemplate.CloneTree().Children().First() as SlideToggle;
                CreateCategoryToggle(child, childToggle);
                childToggle.AddToClassList("child");
                element.Add(childToggle);
            }
        }

        private void SetAllValues(bool value)
        {
            for (int i = 0; i < categorieslist.itemsSource.Count; i++)
            {
                var element = categorieslist.GetRootElementForIndex(i);
                if (element is SlideToggle categoryToggle)
                {
                    SetValueSelected(categoryToggle, value);
                }
            }

            for (int i = 0; i < difficultieslist.itemsSource.Count; i++)
            {
                var element = difficultieslist.GetRootElementForIndex(i);
                if (element is SlideToggle categoryToggle)
                {
                    SetValueSelected(categoryToggle, value);
                }
            }
        }

        private void SetValueSelected(SlideToggle toggle, bool value)
        {
            toggle.value = value;

            foreach (VisualElement child in toggle.Children())
            {
                if (child is SlideToggle childToggle)
                {
                    SetValueSelected(childToggle, value);
                }
            }
        }

        public IEnumerator GetSelectedCategoryIdsCoroutine(Action<List<int>> callback)
        {
            if (selectedCategories == null)
            {
                yield return StartCoroutine(GetCategories());
            }

            List<int> selectedIds = selectedCategories?
                .SelectMany(GetSelectedCategoriesRecursive)
                .Where(c => c.isSelected)
                .Select(c => c.Id)
                .ToList();

            callback?.Invoke(selectedIds);
        }

        public List<Difficulty> GetSelectedDifficulties()
        {
            return selectedDifficulties;
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
            gameSettingsUI.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            gameSettingsUI.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            gameSettingsUI.schedule.Execute(RestoreScrollPosition);
            gameSettingsUI.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void Show()
        {
            gameSettingsUI.style.display = DisplayStyle.Flex;

            SubscribeToGeometryChange();
        }

        private void Hide()
        {
            SaveScrollPosition();
            gameSettingsUI.style.display = DisplayStyle.None;
        }
    }
}