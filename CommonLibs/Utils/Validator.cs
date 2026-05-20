using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace CommonLibs.Utils
{
    public static partial class Validator
    {
        public static readonly Regex PasswordRegex =
        new Regex(
            "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*.,-]).{8,}$",
            RegexOptions.Compiled);

        public static readonly Regex EmailRegex =
            new Regex(
                "\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex PhoneRegex =
            new Regex(
                @"^(\+?[0-9]{1,3})?0?\d{9}$",
                RegexOptions.Compiled);

        public static readonly Regex CitizenIdRegex =
            new Regex(
                @"^(\d{9}|\d{12})$",
                RegexOptions.Compiled);

        public static bool IsValidPassword(string password) => PasswordRegex.IsMatch(password);

        public static bool IsValidEmail(string email) => EmailRegex.IsMatch(email);

        public static bool IsValidPhone(string phone) => PhoneRegex.IsMatch(phone);

        public static bool IsValidCitizenIdNumber(string citizenIdNumber)
            => CitizenIdRegex.IsMatch(citizenIdNumber);

        public static bool IsMaxLength(string value, int maxLength)
         => !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= maxLength;

        public static bool IsMinLength(string value, int minLength)
            => !string.IsNullOrWhiteSpace(value) && value.Trim().Length >= minLength;

        public static bool IsInRange(string value, int min, int max)
            => !string.IsNullOrWhiteSpace(value) && value.Trim().Length >= min && value.Trim().Length <= max;

        public static bool IsSqlInjection(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            var pattern = @"('|--|;|/\*|\*/|xp_|sp_|exec|drop\b|select\b|insert\b|update\b|delete\b|union\b)";
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }
    }
}
