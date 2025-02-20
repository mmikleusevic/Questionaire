using System.Net;
using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Web.Interfaces;
using Web.Models;

namespace Web.Services;

public class QuestionService(HttpClient httpClient, 
    ILogger<QuestionService> logger,
    ToastService toastService) : IQuestionService
{
    public async Task<List<Question>> GetQuestions()
    {
        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync("api/Question");
        
            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                List<Question>? questions = JsonConvert.DeserializeObject<List<Question>>(responseData);

                return questions ?? new List<Question>();
            }
            
            string? responseResult = await response.Content.ReadAsStringAsync();
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error fetching questions", ex.Message);
            logger.LogError(ex, "Error fetching questions");
        }
        
        return new List<Question>();
    }
    
    public async Task<Question> GetQuestion(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync($"api/Question/{id}");
        
            if (response.IsSuccessStatusCode)
            {
                string responseStream = await response.Content.ReadAsStringAsync();
                Question? result = JsonConvert.DeserializeObject<Question>(responseStream);

                return result ?? new Question();
            }
            
            string? responseResult = await response.Content.ReadAsStringAsync();
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error fetching a question", ex.Message);
            logger.LogError(ex, "Error fetching a question");
        }
        
        return new Question();
    }
    
    public async Task CreateQuestion(Question newQuestion)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(newQuestion);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            HttpResponseMessage? response = await httpClient.PostAsync("api/Question", content);
            string responseResult = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Created) responseResult = "Question created successfully";
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error creating a question", ex.Message);
            logger.LogError(ex, "Error creating a question");
        }
    }
    
    public async Task UpdateQuestion(Question updatedQuestion)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(updatedQuestion);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            HttpResponseMessage? response = await httpClient.PutAsync($"api/Question/{updatedQuestion.Id}", content);
            string responseResult = await response.Content.ReadAsStringAsync();
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error updating a question", ex.Message);
            logger.LogError(ex, "Error updating a question");
        }
    }
    
    public async Task DeleteQuestion(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.DeleteAsync($"api/Question/{id}");
            string responseResult = await response.Content.ReadAsStringAsync();
            
            Helper.ShowToast(toastService, response.StatusCode, responseResult ,responseResult);
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error deleting a question", ex.Message);
            logger.LogError(ex, "Error deleting a question");
        }
    }
}