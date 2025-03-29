using System.Net;
using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Shared.Models;
using Web.Handlers;
using Web.Interfaces;

namespace Web.Services;

public class CategoryService(
    HttpClient httpClient,
    ILogger<CategoryService> logger,
    ToastService toastService) : ICategoryService
{
    private List<CategoryExtendedDto>? flatCategories;
    private List<CategoryExtendedDto>? nestedCategories;

    public async Task<CategoriesDto> GetCategories(bool forceRefresh = false)
    {
        string context = "fetching categories";
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync("api/Category");

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger))
            {
                return new CategoriesDto();
            }

            string responseData = await response.Content.ReadAsStringAsync();
            CategoriesDto? categories = JsonConvert.DeserializeObject<CategoriesDto>(responseData);

            if (categories != null)
            {
                flatCategories = categories.FlatCategories;
                nestedCategories = categories.NestedCategories;
            }

            return categories ?? new CategoriesDto();
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
            return new CategoriesDto();
        }
    }

    public async Task<List<CategoryExtendedDto>> GetNestedCategories()
    {
        string context = "fetching nested categories";
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync("api/Category/nested");

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger))
            {
                return new List<CategoryExtendedDto>();
            }

            string responseData = await response.Content.ReadAsStringAsync();
            nestedCategories = JsonConvert.DeserializeObject<List<CategoryExtendedDto>>(responseData);

            return nestedCategories ?? new List<CategoryExtendedDto>();
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
            return new List<CategoryExtendedDto>();
        }
    }

    public async Task<List<CategoryExtendedDto>> GetFlatCategories(string searchQuery = "")
    {
        string context = "fetching flat categories";
        try
        {
            string encodedSearchQuery = Uri.EscapeDataString(searchQuery);
            HttpResponseMessage response =
                await httpClient.GetAsync($"api/Category/flat?searchQuery={encodedSearchQuery}");

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger))
            {
                return new List<CategoryExtendedDto>();
            }

            string responseData = await response.Content.ReadAsStringAsync();
            flatCategories = JsonConvert.DeserializeObject<List<CategoryExtendedDto>>(responseData);

            return flatCategories ?? new List<CategoryExtendedDto>();
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
            return new List<CategoryExtendedDto>();
        }
    }

    public async Task CreateCategory(CategoryExtendedDto newCategory)
    {
        string context = "creating a category";
        try
        {
            string jsonContent = JsonConvert.SerializeObject(newCategory);
            using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("api/Category", content);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;

            ToastHandler.ShowToast(toastService, HttpStatusCode.OK, "Success", "Category created successfully.");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }

    public async Task UpdateCategory(CategoryExtendedDto updatedCategory)
    {
        string context = "updating a category";
        try
        {
            string jsonContent = JsonConvert.SerializeObject(updatedCategory);
            using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PutAsync($"api/Category/{updatedCategory.Id}", content);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;

            ToastHandler.ShowToast(toastService, HttpStatusCode.OK, "Success", "Category updated successfully.");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }

    public async Task DeleteCategory(int id)
    {
        string context = "deleting a category";
        try
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"api/Category/{id}");

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;

            ToastHandler.ShowToast(toastService, HttpStatusCode.OK, "Success", "Category deleted successfully.");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }
}