using System.Net;
using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Shared.Models;
using Web.Interfaces;

namespace Web.Services;

public class QuestionService(
    HttpClient httpClient,
    ILogger<QuestionService> logger,
    ToastService toastService) : IQuestionService
{
    public async Task<PaginatedResponse<QuestionExtendedDto>> GetQuestions(QuestionsRequestDto questionsRequest)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(questionsRequest);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = await httpClient.PostAsync(
                "api/Question/paged", content);

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                PaginatedResponse<QuestionExtendedDto>? paginatedResponse =
                    JsonConvert.DeserializeObject<PaginatedResponse<QuestionExtendedDto>>(responseData);

                return paginatedResponse ?? new PaginatedResponse<QuestionExtendedDto>();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching questions", logger);
        }

        return new PaginatedResponse<QuestionExtendedDto>();
    }

    public async Task CreateQuestion(QuestionExtendedDto newQuestion)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(newQuestion);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = await httpClient.PostAsync("api/Question/create", content);
            string responseResult = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Created) responseResult = "Question created successfully";

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "creating a question", logger);
        }
    }

    public async Task ApproveQuestion(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.PutAsync($"api/Question/approve/{id}", null);
            string responseResult = await response.Content.ReadAsStringAsync();

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "Approving a question", logger);
        }
    }

    public async Task UpdateQuestion(QuestionExtendedDto updatedQuestion)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(updatedQuestion);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = await httpClient.PutAsync($"api/Question/{updatedQuestion.Id}", content);
            string responseResult = await response.Content.ReadAsStringAsync();

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "updating a question", logger);
        }
    }

    public async Task DeleteQuestion(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.DeleteAsync($"api/Question/{id}");
            string responseResult = await response.Content.ReadAsStringAsync();

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "deleting a question", logger);
        }
    }
}