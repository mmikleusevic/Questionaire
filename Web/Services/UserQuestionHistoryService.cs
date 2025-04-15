using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using SharedStandard.Models;
using Web.Handlers;
using Web.Interfaces;

namespace Web.Services;

public class UserQuestionHistoryService(
    HttpClient httpClient,
    ILogger<QuestionService> logger,
    ToastService toastService) : IUserQuestionHistoryService
{
    public async Task CreateUserHistory(UserQuestionHistoryDto userQuestionHistoryDto)
    {
        string context = "creating user question history";
        try
        {
            string jsonContent = JsonConvert.SerializeObject(userQuestionHistoryDto);
            using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("api/UserQuestionHistory", content);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }
}