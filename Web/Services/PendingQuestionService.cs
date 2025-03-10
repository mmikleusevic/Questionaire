using System.Net;
using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Web.Interfaces;
using Web.Models;

namespace Web.Services;

public class PendingQuestionService(
    HttpClient httpClient,
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
                PaginatedResponse<PendingQuestion>? paginatedResponse =
                    JsonConvert.DeserializeObject<PaginatedResponse<PendingQuestion>>(responseData);

                return paginatedResponse ?? new PaginatedResponse<PendingQuestion>();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching pending questions", logger);
        }

        return new PaginatedResponse<PendingQuestion>();
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

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "creating a pending question", logger);
        }
    }

    public async Task ApprovePendingQuestion(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.PutAsync($"api/PendingQuestion/approve/{id}", null);
            string responseResult = await response.Content.ReadAsStringAsync();

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "deleting a pending question", logger);
        }
    }

    public async Task UpdatePendingQuestion(PendingQuestion updatedPendingQuestion)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(updatedPendingQuestion);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response =
                await httpClient.PutAsync($"api/PendingQuestion/{updatedPendingQuestion.Id}", content);
            string responseResult = await response.Content.ReadAsStringAsync();

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "updating a pending question", logger);
        }
    }

    public async Task DeletePendingQuestion(int id)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.DeleteAsync($"api/PendingQuestion/{id}");
            string responseResult = await response.Content.ReadAsStringAsync();

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "deleting a pending question", logger);
        }
    }
}