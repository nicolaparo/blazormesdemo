using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

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

        public static Uri WithQueryParam(this Uri uri, string name, string value)
        {
            var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

            httpValueCollection.Remove(name);
            httpValueCollection.Add(name, value);

            var ub = new UriBuilder(uri);
            ub.Query = httpValueCollection.ToString();

            return ub.Uri;
        }
    }


}
