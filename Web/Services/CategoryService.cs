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
    public async Task<List<Category>> GetCategories()
    {
        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync("api/Category");
        
            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                List<Category>? categories = JsonConvert.DeserializeObject<List<Category>>(responseData);

                if (categories != null){ return categories;}
            }
            else
            {
                string? responseResult = await response.Content.ReadAsStringAsync();
                Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
            }
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error fetching categories", ex.Message);
            logger.LogError(ex, "Error fetching categories");
        }
        
        return new List<Category>();
    }
    
    public async Task<Category> GetCategory(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync($"api/Category/{id}");
        
            if (response.IsSuccessStatusCode)
            {
                string responseStream = await response.Content.ReadAsStringAsync();
                Category? result = JsonConvert.DeserializeObject<Category>(responseStream);

                if (result != null) return result;
            }
            else
            {
                string? responseResult = await response.Content.ReadAsStringAsync();
                Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
            }

        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error fetching a category", ex.Message);
            logger.LogError(ex, "Error fetching a category");
        }
        
        return new Category();
    }
    
    public async Task CreateCategory(Category category)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(category);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            HttpResponseMessage? response = await httpClient.PostAsync("api/Category", content);
            string responseResult = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(responseResult)) responseResult = "Category created successfully";
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error creating a category", ex.Message);
            logger.LogError(ex, "Error creating a category");
        }
    }
    
    public async Task UpdateCategory(Category category)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(category);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            HttpResponseMessage? response = await httpClient.PutAsync($"api/Category/{category.Id}", content);
            string responseResult = await response.Content.ReadAsStringAsync();
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
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
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error deleting a category", ex.Message);
            logger.LogError(ex, "Error deleting a category");
        }
    }
}