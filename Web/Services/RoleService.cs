using BlazorBootstrap;
using Newtonsoft.Json;
using Shared.Models;
using Web.Handlers;
using Web.Interfaces;

namespace Web.Services;

public class RoleService(
    HttpClient httpClient,
    ILogger<RoleService> logger,
    ToastService toastService) : IRoleService
{
    public async Task<IList<RoleDto>> GetRoles()
    {
        string context = "fetching roles";
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync("api/Role");

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger))
            {
                return new List<RoleDto>();
            }

            string responseData = await response.Content.ReadAsStringAsync();
            List<RoleDto>? rolesList = JsonConvert.DeserializeObject<List<RoleDto>>(responseData);

            return rolesList ?? new List<RoleDto>();
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
            return new List<RoleDto>();
        }
    }
}