using System.Net;
using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Web.Interfaces;
using Web.Models;

namespace Web.Services;

public class QuestionService(
    HttpClient httpClient,
    ILogger<QuestionService> logger,
    ToastService toastService) : IQuestionService
{
    public async Task<PaginatedResponse<Question>> GetQuestions(int currentPage, int pageSize)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync(
                $"api/Question?pageNumber={currentPage}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                PaginatedResponse<Question>? paginatedResponse =
                    JsonConvert.DeserializeObject<PaginatedResponse<Question>>(responseData);

                return paginatedResponse ?? new PaginatedResponse<Question>();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching questions", logger);
        }

        return new PaginatedResponse<Question>();
    }

    public async Task UpdateQuestion(Question updatedQuestion)
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