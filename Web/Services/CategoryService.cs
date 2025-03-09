using System.Net;
using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Web.Interfaces;
using Web.Models;

namespace Web.Services;

public class CategoryService(HttpClient httpClient, 
    ILogger<CategoryService> logger,
    ToastService toastService) : ICategoryService
{
    private List<Category>? flatCategories;
    private List<Category>? nestedCategories;
    private DateTime lastFetchTime;
    private readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(10);
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    
    public async Task<CategoryLists> GetCategories(bool forceRefresh = false)
    {   
        if (!forceRefresh && nestedCategories != null && flatCategories != null && DateTime.UtcNow - lastFetchTime < cacheDuration)
        {
            return new CategoryLists { FlatCategories = flatCategories, NestedCategories = nestedCategories };
        }
        
        await semaphore.WaitAsync();

        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync("api/Category");

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                CategoryLists? categories = JsonConvert.DeserializeObject<CategoryLists>(responseData);
                lastFetchTime = DateTime.UtcNow;

                if (categories != null)
                {
                    flatCategories = categories.FlatCategories;
                    nestedCategories = categories.NestedCategories;
                }

                return categories ?? new CategoryLists();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            Helper.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error fetching categories", ex.Message);
            logger.LogError(ex, "Error fetching categories");
        }
        finally
        {
            semaphore.Release();
        }
        
        return new CategoryLists();
    }
    
    public async Task<List<Category>> GetNestedCategories()
    {
        if (nestedCategories != null && DateTime.UtcNow - lastFetchTime < cacheDuration) return nestedCategories;
        
        await semaphore.WaitAsync();

        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync("api/Category/nested");

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                nestedCategories = JsonConvert.DeserializeObject<List<Category>>(responseData);
                lastFetchTime = DateTime.UtcNow;

                return nestedCategories ?? new List<Category>();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            Helper.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error fetching nested categories",
                ex.Message);
            logger.LogError(ex, "Error fetching nested categories");
        }
        finally
        {
            semaphore.Release();
        }
        
        return new List<Category>();
    }
    
    public async Task<List<Category>> GetFlatCategories()
    {
        if (flatCategories != null && DateTime.UtcNow - lastFetchTime < cacheDuration) return flatCategories;
        
        await semaphore.WaitAsync();

        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync("api/Category/flat");

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                flatCategories = JsonConvert.DeserializeObject<List<Category>>(responseData);
                lastFetchTime = DateTime.UtcNow;

                return flatCategories ?? new List<Category>();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            Helper.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error fetching flat categories",
                ex.Message);
            logger.LogError(ex, "Error fetching flat categories");
        }
        finally
        {
            semaphore.Release();
        }
        
        return new List<Category>();
    }
    
    public async Task CreateCategory(Category newCategory)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(newCategory);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            HttpResponseMessage? response = await httpClient.PostAsync("api/Category", content);
            string responseResult = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Created) responseResult = "Category created successfully";
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
            
            ClearCache();
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error creating a category", ex.Message);
            logger.LogError(ex, "Error creating a category");
        }
    }
    
    public async Task UpdateCategory(Category updatedCategory)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(updatedCategory);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            HttpResponseMessage? response = await httpClient.PutAsync($"api/Category/{updatedCategory.Id}", content);
            string responseResult = await response.Content.ReadAsStringAsync();
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
            
            ClearCache();
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error updating a category", ex.Message);
            logger.LogError(ex, "Error updating a category");
        }
    }
    
    public async Task DeleteCategory(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.DeleteAsync($"api/Category/{id}");
            string responseResult = await response.Content.ReadAsStringAsync();
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
            
            ClearCache();
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error deleting a category", ex.Message);
            logger.LogError(ex, "Error deleting a category");
        }
    }
    
    private void ClearCache()
    {
        flatCategories = null;
        nestedCategories = null;
    }
}