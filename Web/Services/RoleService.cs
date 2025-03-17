using BlazorBootstrap;
using Newtonsoft.Json;
using Web.Interfaces;

namespace Web.Services;

public class RoleService(
    HttpClient httpClient,
    ILogger<RoleService> logger,
    ToastService toastService) : IRoleService
{
    public async Task<IList<string>> GetRoles()
    {
        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync("api/Role");

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                IList<string>? roles =
                    JsonConvert.DeserializeObject<IList<string>>(responseData);

                return roles ?? new List<string>();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching roles", logger);
        }

        return new List<string>();
    }
}