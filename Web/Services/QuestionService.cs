using System.Net;
using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Shared.Models;
using SharedStandard.Models;
using Web.Handlers;
using Web.Interfaces;

namespace Web.Services;

public class QuestionService(
    HttpClient httpClient,
    ILogger<QuestionService> logger,
    ToastService toastService) : IQuestionService
{
    public async Task<PaginatedResponse<QuestionExtendedDto>> GetQuestions(QuestionsRequestDto questionsRequest)
    {
        string context = "fetching questions";
        try
        {
            string jsonContent = JsonConvert.SerializeObject(questionsRequest);
            using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("api/Question/paged", content);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger))
            {
                return new PaginatedResponse<QuestionExtendedDto>();
            }

            string responseData = await response.Content.ReadAsStringAsync();
            PaginatedResponse<QuestionExtendedDto>? paginatedResponse =
                JsonConvert.DeserializeObject<PaginatedResponse<QuestionExtendedDto>>(responseData);

            return paginatedResponse ?? new PaginatedResponse<QuestionExtendedDto>();
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
            return new PaginatedResponse<QuestionExtendedDto>();
        }
    }

    public async Task<List<QuestionExtendedDto>> GetRandomUniqueQuestions(
        UniqueQuestionsRequestDto uniqueQuestionsRequestDto)
    {
        string context = "fetching unique questions";
        try
        {
            string jsonContent = JsonConvert.SerializeObject(uniqueQuestionsRequestDto);
            using StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("api/Question/random", content);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger))
            {
                return new List<QuestionExtendedDto>();
            }

            string responseData = await response.Content.ReadAsStringAsync();
            List<QuestionExtendedDto>? paginatedResponse =
                JsonConvert.DeserializeObject<List<QuestionExtendedDto>>(responseData);

            return paginatedResponse ?? new List<QuestionExtendedDto>();
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
            return new List<QuestionExtendedDto>();
        }
    }

    public async Task CreateQuestion(QuestionExtendedDto newQuestion)
    {
        string context = "creating a question";
        try
        {
            string jsonContent = JsonConvert.SerializeObject(newQuestion);
            using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("api/Question/create", content);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;

            ToastHandler.ShowToast(toastService, HttpStatusCode.OK, "Success", "Question created successfully.");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }

    public async Task ApproveQuestion(int id)
    {
        string context = "approving a question";
        try
        {
            HttpResponseMessage response = await httpClient.PutAsync($"api/Question/approve/{id}", null);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;

            ToastHandler.ShowToast(toastService, HttpStatusCode.OK, "Success", "Question approved successfully.");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }

    public async Task UpdateQuestion(QuestionExtendedDto updatedQuestion)
    {
        string context = "updating a question";
        try
        {
            string jsonContent = JsonConvert.SerializeObject(updatedQuestion);
            using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PutAsync($"api/Question/{updatedQuestion.Id}", content);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;

            ToastHandler.ShowToast(toastService, HttpStatusCode.OK, "Success", "Question updated successfully.");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }

    public async Task DeleteQuestion(int id)
    {
        string context = "deleting a question";
        try
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"api/Question/{id}");

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;

            ToastHandler.ShowToast(toastService, HttpStatusCode.OK, "Success", "Question deleted successfully.");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }
}