using BlazorBootstrap;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using SharedStandard.Models;
using Web.Interfaces;
using Web.Pages.Play.PlayModals;

namespace Web.Pages.Play;

public partial class Play : ComponentBase
{
    private const int TargetQuestionNumber = 50;
    private readonly List<QuestionExtendedDto> questions = new List<QuestionExtendedDto>();

    private readonly HashSet<Difficulty> selectedDifficulties = new HashSet<Difficulty>
        { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };

    private string? deviceIdentifier;
    private bool isInitializing = true;
    private bool isLoading;
    private UniqueQuestionsRequestDto? lastQuestionsRequestState;

    private Modal? modal;
    private List<CategoryExtendedDto>? nestedCategories;
    private List<CategoryExtendedDto>? selectedCategories;
    [Inject] private ICategoryService CategoryService { get; set; }
    [Inject] private ILocalStorageService LocalStorage { get; set; }
    [Inject] private IQuestionService QuestionService { get; set; }

    private List<Difficulty> AvailableDifficultiesToAdd =>
        Enum.GetValues<Difficulty>().Except(selectedDifficulties).ToList();

    private bool CanPlay => !isInitializing &&
                            selectedCategories != null &&
                            GetSelectedCategoryIds().Count > 0 &&
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

    private HashSet<int> GetSelectedCategoryIds()
    {
        HashSet<int> selectedIds = new HashSet<int>();
        CollectSelectedCategoryIdsRecursive(nestedCategories, selectedIds);
        return selectedIds.ToHashSet();
    }

    private void CollectSelectedCategoryIdsRecursive(List<CategoryExtendedDto>? categories, HashSet<int> selectedIds)
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

        isInitializing = true;
        isLoading = true;

        HashSet<int> currentCategoryIds = GetSelectedCategoryIds();
        HashSet<Difficulty> currentDifficulties = selectedDifficulties;

        bool criteriaChanged = true;
        if (lastQuestionsRequestState != null)
        {
            HashSet<int> lastCategoryIds = lastQuestionsRequestState?.CategoryIds?.ToHashSet() ?? new HashSet<int>();
            HashSet<Difficulty> lastDifficulties =
                lastQuestionsRequestState?.Difficulties?.ToHashSet() ?? new HashSet<Difficulty>();

            bool categoriesSame = currentCategoryIds.SetEquals(lastCategoryIds);
            bool difficultiesSame = currentDifficulties.SetEquals(lastDifficulties);
            criteriaChanged = !categoriesSame || !difficultiesSame;
        }

        int numberOfQuestionsToFetch = 0;

        if (criteriaChanged)
        {
            questions.Clear();
            numberOfQuestionsToFetch = TargetQuestionNumber;
        }
        else
        {
            questions.RemoveAll(q => q.isRead);

            int currentQuestionCount = questions.Count;
            numberOfQuestionsToFetch = TargetQuestionNumber - currentQuestionCount;
            numberOfQuestionsToFetch = Math.Max(0, numberOfQuestionsToFetch);
        }

        UniqueQuestionsRequestDto currentRequestDto = new UniqueQuestionsRequestDto
        {
            UserId = deviceIdentifier,
            NumberOfQuestions = numberOfQuestionsToFetch,
            Difficulties = currentDifficulties.ToArray(),
            CategoryIds = currentCategoryIds.ToArray(),
            IsSingleAnswerMode = isSingleAnswerMode
        };

        lastQuestionsRequestState = currentRequestDto;

        if (numberOfQuestionsToFetch > 0)
        {
            List<QuestionExtendedDto> fetchedQuestions =
                await QuestionService.GetRandomUniqueQuestions(currentRequestDto);
            questions.AddRange(fetchedQuestions);
        }

        if (questions.Any())
        {
            await ShowReadQuestion(isSingleAnswerMode);
        }

        isLoading = false;
        isInitializing = false;
    }

    private async Task ShowReadQuestion(bool isSingleAnswerMode)
    {
        if (modal == null || deviceIdentifier == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", modal },
            { "DeviceIdentifier", deviceIdentifier },
            { "Questions", questions },
            { "IsSingleAnswerMode", isSingleAnswerMode }
        };

        await modal.ShowAsync<ReadQuestions>("Questions", parameters: parameters);
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