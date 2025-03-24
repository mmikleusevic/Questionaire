using System.Text;
using BlazorBootstrap;
using Newtonsoft.Json;
using Shared.Models;
using Web.Interfaces;

namespace Web.Services;

public class UserService(
    HttpClient httpClient,
    ILogger<UserService> logger,
    ToastService toastService) : IUserService
{
    public async Task<List<UserDto>> GetUsers()
    {
        try
        {
            HttpResponseMessage? response = await httpClient.GetAsync("api/User");

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                List<UserDto>? users =
                    JsonConvert.DeserializeObject<List<UserDto>>(responseData);

                return users ?? new List<UserDto>();
            }

            string? responseResult = await response.Content.ReadAsStringAsync();
            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching users", logger);
        }

        return new List<UserDto>();
    }

    public async Task UpdateUser(UserDto updatedUser)
    {
        try
        {
            string? jsonContent = JsonConvert.SerializeObject(updatedUser);
            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = await httpClient.PutAsync("api/User", content);
            string responseResult = await response.Content.ReadAsStringAsync();

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "updating a user", logger);
        }
    }

    public async Task DeleteUser(string userName)
    {
        try
        {
            HttpResponseMessage? response = await httpClient.DeleteAsync($"api/User/{userName}");
            string responseResult = await response.Content.ReadAsStringAsync();

            ToastHandler.ShowToast(toastService, response.StatusCode, responseResult, responseResult);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "deleting a user", logger);
        }
    }
}