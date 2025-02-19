using System.Text;
using Newtonsoft.Json;
using Web.Interfaces;
using Web.Models;

namespace Web.Services;

public class CategoryService(HttpClient httpClient, ILogger<CategoryService> logger) : ICategoryService
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

                if (categories != null) return categories;
            }
            else
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseData);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            logger.LogError(ex, "Error fetching categories");
        }
        
        return new List<Category>();
    }
    
    public async Task<Category> GetCategory(int id)
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
            string? responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseData);
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

            if (string.IsNullOrEmpty(responseResult))
            {
                responseResult = "Category created successfully";
            }
            
            Console.WriteLine(responseResult);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
            
            Console.WriteLine(responseResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            logger.LogError(ex, "Error updating a category");
        }
    }
    
    public async Task DeleteCategory(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.DeleteAsync($"api/Category/{id}");
            string responseResult = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine(responseResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deleting a category:" + ex.Message);
            logger.LogError(ex, "Error deleting a category");
        }
    }
}