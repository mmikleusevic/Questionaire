using System.Net;
using BlazorBootstrap;
using Newtonsoft.Json.Linq;

namespace Web;

public static class ApiResponseHandler
{
    public static async Task<bool> HandleResponse(
        HttpResponseMessage response, 
        ToastService toastService, 
        string errorContext)
    {
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        string responseData = await response.Content.ReadAsStringAsync();
        
        try
        {
            JObject jsonResponse = JObject.Parse(responseData);
            if (jsonResponse["errors"] is JObject errors)
            {
                string errorMessage = string.Join("\n", errors
                    .Properties()
                    .SelectMany(prop => prop.Value)
                    .Select(error => error.ToString()));

                ToastHandler.ShowToast(toastService, response.StatusCode, $"Error while {errorContext}", errorMessage);
                return false;
            }

            ToastHandler.ShowToast(toastService, response.StatusCode, $"Error while {errorContext}", $"Error while {errorContext}");
        }
        catch
        {
            ToastHandler.ShowToast(toastService, response.StatusCode, $"Error while parsing JSON", "Error while parsing JSON");
        }
        
        return false;
    }
    
    public static void HandleException(
        Exception ex, 
        ToastService toastService, 
        string errorContext,
        ILogger? logger = null)
    {
        ToastHandler.ShowToast(toastService, HttpStatusCode.InternalServerError, $"Error while {errorContext}", ex.Message);
        logger?.LogError(ex, $"Error while {errorContext}");
    }
}