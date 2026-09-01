using Dapper;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace CommonLibs.Logging
{
    internal static class SqlLoggingHelper
    {
        private static readonly string[] SensitiveNames =
        {
            "password", "token", "secret", "authorization", "connectionstring", "apikey"
        };

        public static IReadOnlyDictionary<string, object?> GetSafeParameters(object? parameters)
        {
            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (parameters == null)
                return result;

            if (parameters is DynamicParameters dynamicParameters)
            {
                foreach (var name in dynamicParameters.ParameterNames.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        result[name] = SanitizeValue(name, dynamicParameters.Get<object?>(name));
                    }
                    catch
                    {
                        result[name] = "[Unavailable]";
                    }
                }

                return result;
            }

            if (parameters is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    var name = entry.Key?.ToString() ?? "Unknown";
                    result[name] = SanitizeValue(name, entry.Value);
                }

                return result;
            }

            var type = parameters.GetType();
            if (IsSimpleType(type))
            {
                result["Value"] = SanitizeValue("Value", parameters);
                return result;
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .Where(property => property.CanRead && property.GetIndexParameters().Length == 0))
            {
                try
                {
                    result[property.Name] = SanitizeValue(property.Name, property.GetValue(parameters));
                }
                catch
                {
                    result[property.Name] = "[Unavailable]";
                }
            }

            return result;
        }

        private static object? SanitizeValue(string name, object? value)
        {
            if (SensitiveNames.Any(sensitive => name.Contains(sensitive, StringComparison.OrdinalIgnoreCase)))
                return "***";

            if (value == null || value == DBNull.Value)
                return null;

            if (value is byte[] bytes)
                return $"[Binary: {bytes.Length} bytes]";

            if (value is string text)
                return text.Length <= 1000 ? text : $"{text[..1000]}...[truncated]";

            return IsSimpleType(value.GetType()) ? value : value.ToString();
        }

        private static bool IsSimpleType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) ||
                   type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan) ||
                   type == typeof(Guid);
        }

        // Build sẵn script "DECLARE @Param TYPE = value;" từ Parameters để copy thẳng vào SSMS chạy lại
        // câu SQL kèm giá trị thật khi cần debug, thay vì phải tự gõ tay từng biến từ object/JSON.
        public static string BuildDeclareStatements(IReadOnlyDictionary<string, object?> parameters)
        {
            if (parameters.Count == 0)
                return "(none)";

            return string.Join(" ", parameters.Select(kv => $"DECLARE @{kv.Key} {ToSqlDeclaration(kv.Value)};"));
        }

        private static string ToSqlDeclaration(object? value)
        {
            return value switch
            {
                null => "NVARCHAR(MAX) = NULL",
                bool b => $"BIT = {(b ? 1 : 0)}",
                byte or sbyte or short or ushort or int or uint => $"INT = {Convert.ToInt64(value)}",
                long or ulong => $"BIGINT = {Convert.ToInt64(value)}",
                float or double => $"FLOAT = {Convert.ToDouble(value).ToString(CultureInfo.InvariantCulture)}",
                decimal dec => $"DECIMAL(38,10) = {dec.ToString(CultureInfo.InvariantCulture)}",
                DateTime dt => $"DATETIME = '{dt:yyyy-MM-dd HH:mm:ss.fff}'",
                DateTimeOffset dto => $"DATETIMEOFFSET = '{dto:yyyy-MM-dd HH:mm:ss.fff zzz}'",
                Guid guid => $"UNIQUEIDENTIFIER = '{guid}'",
                Enum e => $"INT = {Convert.ToInt64(e)}",
                string s => $"NVARCHAR(MAX) = N'{s.Replace("'", "''")}'",
                _ => $"NVARCHAR(MAX) = N'{value.ToString()?.Replace("'", "''")}'"
            };
        }
    }
}
