using System.Net;
using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Shared.Models;
using Web.Handlers;
using Web.Interfaces;

namespace Web.Services;

public class UserService(
    HttpClient httpClient,
    ILogger<UserService> logger,
    ToastService toastService) : IUserService
{
    public async Task<List<UserDto>> GetUsers()
    {
        string context = "fetching users";
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync("api/User");

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger))
            {
                return new List<UserDto>();
            }

            string responseData = await response.Content.ReadAsStringAsync();
            List<UserDto>? users = JsonConvert.DeserializeObject<List<UserDto>>(responseData);

            return users ?? new List<UserDto>();
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
            return new List<UserDto>();
        }
    }

    public async Task UpdateUser(UserDto updatedUser)
    {
        string context = "updating a user";
        try
        {
            string jsonContent = JsonConvert.SerializeObject(updatedUser);
            using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PutAsync("api/User", content);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;

            ToastHandler.ShowToast(toastService, HttpStatusCode.OK, "Success", "User updated successfully.");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }

    public async Task DeleteUser(string userName)
    {
        string context = "deleting a user";
        try
        {
            string encodedUserName = Uri.EscapeDataString(userName);
            HttpResponseMessage response = await httpClient.DeleteAsync($"api/User/{encodedUserName}");

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;

            ToastHandler.ShowToast(toastService, HttpStatusCode.OK, "Success", "User deleted successfully.");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }
}