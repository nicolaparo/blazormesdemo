using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NicolaParo.BlazorMes.ManagerApp.Extensions
{

    public static class Extensions
    {
        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            return JsonSerializer.Deserialize<T>(await content.ReadAsStringAsync());
        }
        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content, CancellationToken cancellationToken)
        {
            return JsonSerializer.Deserialize<T>(await content.ReadAsStringAsync(cancellationToken));
        }
    }


}
