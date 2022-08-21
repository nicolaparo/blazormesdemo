using System;

namespace NicolaParo.BlazorMes.ManagerApp.Api
{
    public static class ConversionHelpers
    {
        public static DateTimeOffset? ConvertToDateTimeOffset(string str)
        {
            if (DateTimeOffset.TryParse(str, out var result))
                return result;
            return null;
        }
    }
}
