using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace NicolaParo.BlazorMes.ManagerApp.Api
{
    public static class Responses
    {
        public static IActionResult Ok(object result) => new ContentResult()
        {
            Content = JsonSerializer.Serialize(result),
            ContentType = "application/json",
            StatusCode = 200
        };
        public static IActionResult Ok() => new OkResult();
        public static IActionResult NoContent() => new NoContentResult();
    }
}
