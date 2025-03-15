using System.Net;
using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Shared.Models;
using Web.Interfaces;

namespace Web.Services;

public class PendingQuestionService(
    HttpClient httpClient,
    ILogger<PendingQuestionService> logger,
    ToastService toastService) : IPendingQuestionService
{
    public async Task<PaginatedResponse<PendingQuestionValidationDto>> GetPendingQuestions(
        QuestionsRequestDto pendingQuestionsRequest)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(pendingQuestionsRequest);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = await httpClient.PostAsync(
                "api/PendingQuestion/paged", content);

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                PaginatedResponse<PendingQuestionValidationDto>? paginatedResponse =
                    JsonConvert.DeserializeObject<PaginatedResponse<PendingQuestionValidationDto>>(responseData);

                return paginatedResponse ?? new PaginatedResponse<PendingQuestionValidationDto>();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching pending questions", logger);
        }

        return new PaginatedResponse<PendingQuestionValidationDto>();
    }

    public async Task CreatePendingQuestion(PendingQuestionValidationDto newPendingQuestionValidation)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(newPendingQuestionValidation);
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

    public async Task UpdatePendingQuestion(PendingQuestionValidationDto updatedPendingQuestionValidation)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(updatedPendingQuestionValidation);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response =
                await httpClient.PutAsync($"api/PendingQuestion/{updatedPendingQuestionValidation.Id}", content);
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