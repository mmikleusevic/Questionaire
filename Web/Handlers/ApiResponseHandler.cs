using System.Net;
using BlazorBootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Web.Handlers;

public static class ApiResponseHandler
{
    /// <summary>
    ///     Handles an HttpResponseMessage, showing a toast notification on failure.
    ///     Extracts validation errors from JSON responses if possible.
    /// </summary>
    /// <param name="response">The HttpResponseMessage received from an API call.</param>
    /// <param name="toastService">The service used to display toast notifications.</param>
    /// <param name="errorContext">
    ///     A descriptive string indicating the action being performed (e.g., "saving user", "loading
    ///     data").
    /// </param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <returns>True if the response indicates success; otherwise, false.</returns>
    public static async Task<bool> HandleResponse(
        HttpResponseMessage response,
        ToastService toastService,
        string errorContext,
        ILogger? logger = null)
    {
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        string errorTitle = $"Error while {errorContext}";
        string errorMessage = $"An error occurred. Status: {response.StatusCode}";

        try
        {
            string responseResult = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseResult))
            {
                errorMessage = $"Received error status {response.StatusCode} with no details.";
            }
            else if (response.Content?.Headers?.ContentType?.MediaType?.Contains("json",
                         StringComparison.OrdinalIgnoreCase) ?? false)
            {
                try
                {
                    JObject jsonResponse = JObject.Parse(responseResult);

                    if (jsonResponse["errors"] is JObject errors)
                    {
                        var validationMessages = errors.Properties()
                            .SelectMany(prop => prop.Value.Select(e => e.ToString()))
                            .Where(msg => !string.IsNullOrWhiteSpace(msg));

                        string combinedValidationErrors = string.Join(". ", validationMessages);

                        errorMessage = string.IsNullOrWhiteSpace(combinedValidationErrors)
                            ? "Validation errors occurred but no specific messages were found."
                            : $"Validation Failed: {combinedValidationErrors}";
                    }

                    else if (jsonResponse["title"] is JValue titleValue &&
                             !string.IsNullOrWhiteSpace(titleValue.Value<string>()))
                    {
                        errorMessage = titleValue.Value<string>()!;

                        if (jsonResponse["detail"] is JValue detailValue &&
                            !string.IsNullOrWhiteSpace(detailValue.Value<string>()))
                        {
                            errorMessage += $". {detailValue.Value<string>()}";
                        }
                    }
                    else
                    {
                        logger?.LogWarning(
                            "Received JSON error response for {Context} but could not extract specific 'errors' or 'title'. Content: {Content}",
                            errorContext, responseResult);
                        errorMessage = $"Received error status {response.StatusCode}. Check logs for details.";
                    }
                }
                catch (JsonReaderException jsonEx)
                {
                    logger?.LogWarning(jsonEx, "Failed to parse JSON error response for {Context}. Content: {Content}",
                        errorContext, responseResult);
                    errorMessage = responseResult;
                }
            }

            else
            {
                errorMessage = responseResult;
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Exception processing error response for {Context}.", errorContext);
            errorMessage = $"An unexpected error occurred while processing the response: {ex.Message}";
        }

        ToastHandler.ShowToast(toastService, response.StatusCode, errorTitle, errorMessage);

        return false;
    }

    /// <summary>
    ///     Handles an exception by showing a toast notification and logging the error.
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="toastService">The service used to display toast notifications.</param>
    /// <param name="errorContext">A descriptive string indicating the action being performed when the exception occurred.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public static void HandleException(
        Exception ex,
        ToastService toastService,
        string errorContext,
        ILogger? logger = null)
    {
        string errorTitle = $"Error while {errorContext}";
        ToastHandler.ShowToast(toastService, HttpStatusCode.InternalServerError, errorTitle, ex.Message);
        logger?.LogError(ex, "Exception caught while {Context}", errorContext);
    }
}