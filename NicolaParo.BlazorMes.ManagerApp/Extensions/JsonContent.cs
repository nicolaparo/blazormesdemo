using System.Text.Json;

namespace NicolaParo.BlazorMes.ManagerApp.Extensions
{
    public class JsonContent : StringContent
    {
        public JsonContent(object data) : base(JsonSerializer.Serialize(data))
        {
            Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        }
    }


}
