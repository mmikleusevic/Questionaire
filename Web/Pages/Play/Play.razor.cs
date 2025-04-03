using BlazorBootstrap;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Models;
using SharedStandard.Models;
using Web.Handlers;
using Web.Interfaces;

namespace Web.Pages.Play;

public partial class Play : ComponentBase
{
    [Inject] private ICategoryService CategoryService { get; set; }
    [Inject] private ILocalStorageService LocalStorage { get; set; }
    [Inject] private IQuestionService QuestionService { get; set; }

    private Modal? modal;
    private List<CategoryExtendedDto>? nestedCategories;
    private List<CategoryExtendedDto>? selectedCategories;
    private readonly HashSet<Difficulty> selectedDifficulties = new HashSet<Difficulty> { Difficulty.Easy };

    private List<QuestionExtendedDto> fetchedQuestions = new List<QuestionExtendedDto>();
    private bool isLoading;
    private string? deviceIdentifier;
    private bool isInitializing = true;

    private List<Difficulty> AvailableDifficultiesToAdd =>
        Enum.GetValues<Difficulty>().Except(selectedDifficulties).ToList();

    private bool CanPlay => !isInitializing &&
                            selectedCategories != null &&
                            GetSelectedCategoryIds().Length > 0 &&
                            selectedDifficulties.Any() &&
                            !isLoading &&
                            !string.IsNullOrEmpty(deviceIdentifier);

    protected override async Task OnInitializedAsync()
    {
        isInitializing = true;
        await LoadCategories();
        await InitializeDeviceIdentifier();
        isInitializing = false;
    }

    private async Task LoadCategories()
    {
        nestedCategories = await CategoryService.GetNestedCategories();

        selectedCategories = nestedCategories.ToList();
    }

    private async Task InitializeDeviceIdentifier()
    {
        deviceIdentifier = await LocalStorage.GetItemAsStringAsync("deviceUniqueIdentifier");

        if (string.IsNullOrEmpty(deviceIdentifier))
        {
            deviceIdentifier = Guid.NewGuid().ToString();
            await LocalStorage.SetItemAsStringAsync("deviceUniqueIdentifier", deviceIdentifier);
        }
    }

    private void ToggleAllCategories(bool isSelected)
    {
        if (selectedCategories == null) return;

        foreach (var category in selectedCategories)
        {
            SetAllCategoriesSelected(category, isSelected);
        }
    }

    private int[] GetSelectedCategoryIds()
    {
        List<int> selectedIds = new();
        CollectSelectedCategoryIdsRecursive(nestedCategories, selectedIds);
        return selectedIds.ToArray();
    }

    private void CollectSelectedCategoryIdsRecursive(List<CategoryExtendedDto>? categories, List<int> selectedIds)
    {
        if (categories == null) return;

        foreach (var category in categories)
        {
            if (category.isSelected)
            {
                selectedIds.Add(category.Id);
            }

            CollectSelectedCategoryIdsRecursive(category.ChildCategories, selectedIds);
        }
    }

    private IEnumerable<CategoryExtendedDto> GetSelectedCategoriesRecursive(CategoryExtendedDto category)
    {
        return new[] { category }
            .Concat(category.ChildCategories?
                .SelectMany(GetSelectedCategoriesRecursive) ?? []);
    }

    private void SetAllCategoriesSelected(CategoryExtendedDto category, bool isSelected)
    {
        SetCategorySelected(category, isSelected);

        foreach (CategoryExtendedDto childCategory in category.ChildCategories)
        {
            SetAllCategoriesSelected(childCategory, isSelected);
        }
    }

    private void SetCategorySelected(CategoryExtendedDto category, bool isSelected)
    {
        category.isSelected = isSelected;
    }

    private void AddDifficulty(Difficulty difficulty)
    {
        selectedDifficulties.Add(difficulty);
    }

    private void RemoveDifficulty(Difficulty difficulty)
    {
        selectedDifficulties.Remove(difficulty);
    }

    private async Task HandlePlayClick(bool isSingleAnswerMode)
    {
        if (!CanPlay) return;

        isLoading = true;

        if (selectedCategories == null) return;

        int[] categoryIds = GetSelectedCategoryIds();

        UniqueQuestionsRequestDto request = new UniqueQuestionsRequestDto
        {
            UserId = deviceIdentifier,
            NumberOfQuestions = 40,
            Difficulties = selectedDifficulties.ToArray(),
            CategoryIds = categoryIds,
            IsSingleAnswerMode = isSingleAnswerMode
        };

        fetchedQuestions = await QuestionService.GetRandomUniqueQuestions(request);

        if (fetchedQuestions.Any())
        {
            await modal!.ShowAsync();
        }

        isLoading = false;
    }

    private string GetDifficultyColor(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => "dodgerblue",
            Difficulty.Medium => "orange",
            Difficulty.Hard => "darkred",
            _ => "#6c757d"
        };
    }
}