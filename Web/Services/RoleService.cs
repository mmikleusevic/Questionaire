using BlazorBootstrap;
using Newtonsoft.Json;
using Web.Handlers;
using Web.Interfaces;

namespace Web.Services;

public class RoleService(
    HttpClient httpClient,
    ILogger<RoleService> logger,
    ToastService toastService) : IRoleService
{
    public async Task<IList<string>> GetRoles()
    {
        string context = "fetching roles";
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync("api/Role");

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger))
            {
                return new List<string>();
            }

            string responseData = await response.Content.ReadAsStringAsync();
            List<string>? rolesList = JsonConvert.DeserializeObject<List<string>>(responseData);

            return rolesList ?? new List<string>();
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
            return new List<string>();
        }
    }
}