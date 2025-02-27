using System.Net;
using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Web.Interfaces;
using Web.Models;

namespace Web.Services;

public class PendingQuestionService(HttpClient httpClient, 
    ILogger<PendingQuestionService> logger,
    ToastService toastService) : IPendingQuestionService
{
    public async Task<PaginatedResponse<PendingQuestion>> GetPendingQuestions(int currentPage, int pageSize)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync(
                $"api/PendingQuestion?pageNumber={currentPage}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                PaginatedResponse<PendingQuestion>? paginatedResponse = JsonConvert.DeserializeObject<PaginatedResponse<PendingQuestion>>(responseData);

                return paginatedResponse ?? new PaginatedResponse<PendingQuestion>();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            Helper.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error fetching pending questions", ex.Message);
            logger.LogError(ex, "Error fetching pending questions");
        }
        
        return new PaginatedResponse<PendingQuestion>();
    }

    public async Task<PendingQuestion> GetPendingQuestion(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync($"api/PendingQuestion/{id}");
        
            if (response.IsSuccessStatusCode)
            {
                string responseStream = await response.Content.ReadAsStringAsync();
                PendingQuestion? result = JsonConvert.DeserializeObject<PendingQuestion>(responseStream);

                return result ?? new PendingQuestion();
            }
            
            string? responseResult = await response.Content.ReadAsStringAsync();
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error fetching a pending question", ex.Message);
            logger.LogError(ex, "Error fetching a pending question");
        }
        
        return new PendingQuestion();
    }

    public async Task CreatePendingQuestion(PendingQuestion newPendingQuestion)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(newPendingQuestion);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            HttpResponseMessage? response = await httpClient.PostAsync("api/PendingQuestion", content);
            string responseResult = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Created) responseResult = "Pending question created successfully";
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error creating a pending question", ex.Message);
            logger.LogError(ex, "Error creating a pending question");
        }
    }
    
    public async Task ApprovePendingQuestion(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.PutAsync($"api/PendingQuestion/approve/{id}", null);
            string responseResult = await response.Content.ReadAsStringAsync();
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error approving a pending question", ex.Message);
            logger.LogError(ex, "Error deleting a pending question");
        }
    }

    public async Task UpdatePendingQuestion(PendingQuestion updatedPendingQuestion)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(updatedPendingQuestion);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            HttpResponseMessage? response = await httpClient.PutAsync($"api/PendingQuestion/{updatedPendingQuestion.Id}", content);
            string responseResult = await response.Content.ReadAsStringAsync();
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error updating a pending question", ex.Message);
            logger.LogError(ex, "Error updating a pending question");
        }
    }

    public async Task DeletePendingQuestion(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.DeleteAsync($"api/PendingQuestion/{id}");
            string responseResult = await response.Content.ReadAsStringAsync();
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error deleting a pending question", ex.Message);
            logger.LogError(ex, "Error deleting a pending question");
        }
    }
}