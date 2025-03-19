using System.Net;
using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Shared.Models;
using Web.Interfaces;

namespace Web.Services;

public class CategoryService(
    HttpClient httpClient,
    ILogger<CategoryService> logger,
    ToastService toastService) : ICategoryService
{
    private readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(10);
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    private List<CategoryExtendedDto>? flatCategories;
    private DateTime lastFetchTime;
    private List<CategoryExtendedDto>? nestedCategories;

    public async Task<CategoriesDto> GetCategories(bool forceRefresh = false)
    {
        if (!forceRefresh && nestedCategories != null && flatCategories != null &&
            DateTime.UtcNow - lastFetchTime < cacheDuration)
        {
            return new CategoriesDto { FlatCategories = flatCategories, NestedCategories = nestedCategories };
        }

        await semaphore.WaitAsync();

        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync("api/Category");

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                CategoriesDto? categories = JsonConvert.DeserializeObject<CategoriesDto>(responseData);
                lastFetchTime = DateTime.UtcNow;

                if (categories != null)
                {
                    flatCategories = categories.FlatCategories;
                    nestedCategories = categories.NestedCategories;
                }

                return categories ?? new CategoriesDto();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching categories", logger);
        }
        finally
        {
            semaphore.Release();
        }

        return new CategoriesDto();
    }

    public async Task<List<CategoryExtendedDto>> GetNestedCategories()
    {
        if (nestedCategories != null && DateTime.UtcNow - lastFetchTime < cacheDuration) return nestedCategories;

        await semaphore.WaitAsync();

        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync("api/Category/nested");

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                nestedCategories = JsonConvert.DeserializeObject<List<CategoryExtendedDto>>(responseData);
                lastFetchTime = DateTime.UtcNow;

                return nestedCategories ?? new List<CategoryExtendedDto>();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching nested categories", logger);
        }
        finally
        {
            semaphore.Release();
        }

        return new List<CategoryExtendedDto>();
    }

    public async Task<List<CategoryExtendedDto>> GetFlatCategories()
    {
        if (flatCategories != null && DateTime.UtcNow - lastFetchTime < cacheDuration) return flatCategories;

        await semaphore.WaitAsync();

        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync("api/Category/flat");

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                flatCategories = JsonConvert.DeserializeObject<List<CategoryExtendedDto>>(responseData);
                lastFetchTime = DateTime.UtcNow;

                return flatCategories ?? new List<CategoryExtendedDto>();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching flat categories", logger);
        }
        finally
        {
            semaphore.Release();
        }

        return new List<CategoryExtendedDto>();
    }

    public async Task CreateCategory(CategoryExtendedDto newCategory)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(newCategory);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = await httpClient.PostAsync("api/Category", content);
            string responseResult = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Created) responseResult = "Category created successfully";

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);

            ClearCache();
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "creating a category", logger);
        }
    }

    public async Task UpdateCategory(CategoryExtendedDto updatedCategory)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(updatedCategory);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = await httpClient.PutAsync($"api/Category/{updatedCategory.Id}", content);
            string responseResult = await response.Content.ReadAsStringAsync();

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);

            ClearCache();
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "updating a category", logger);
        }
    }

    public async Task DeleteCategory(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.DeleteAsync($"api/Category/{id}");
            string responseResult = await response.Content.ReadAsStringAsync();

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);

            ClearCache();
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "deleting a category", logger);
        }
    }

    private void ClearCache()
    {
        flatCategories = null;
        nestedCategories = null;
    }
}