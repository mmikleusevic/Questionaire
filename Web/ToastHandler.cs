using System.Net;
using BlazorBootstrap;

namespace Web;

public static class ToastHandler
{
    public static void ShowToast(ToastService toastService, HttpStatusCode statusCode, string? title, string? message)
    {
        ToastType toastType = ToastType.Info;

        if (statusCode is HttpStatusCode.Created or HttpStatusCode.OK)
        {
            toastType = ToastType.Success;
        }
        else if ((int)statusCode >= 400 && (int)statusCode < 500)
        {
            toastType = ToastType.Warning;
        }
        else if ((int)statusCode >= 500)
        {
            toastType = ToastType.Danger;
        }

        toastService.Notify(new ToastMessage
        {
            Type = toastType,
            Message = message,
            Title = title,
            HelpText = $"{DateTime.Now}"
        });
    }
}