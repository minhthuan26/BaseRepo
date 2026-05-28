using CommonLibs.Entities;
using CommonLibs.Utils;
using System.Data;

namespace Gateway.Domain.Entities
{
    public class User : BaseEntity
    {

        public string UserId { get; private set; } = null!;

        public string Password { get; private set; } = null!;

        public string UserName { get; private set; } = null!;

        public int Priority { get; private set; } = 1;

        public string? RefreshToken { get; private set; }

        public byte[]? Signature { get; private set; }

        public string? Email { get; private set; }

        public string? QuickCode { get; private set; }

        protected User() { }

        public User(
            string userId,
            string password,
            string userName,
            int priority = 1,
            string? refreshToken = null,
            string? email = null,
            string? quickCode = null,
            byte[]? signature = null)
        {
            SetUserId(userId);
            SetPassword(password);
            SetUserName(userName);
            SetPriority(priority);
            SetRefreshToken(refreshToken);
            SetEmail(email);
            SetQuickCode(quickCode);
            SetSignature(signature);
        }

        public User(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));

            SetUserId(
                row.Field<string>("UserId")
                ?? throw new AppException("UserId is required"));

            SetPassword(
                row.Field<string>("Password")
                ?? throw new AppException("Password is required"));

            SetUserName(
                row.Field<string>("UserName")
                ?? throw new AppException("UserName is required"));

            SetPriority(row.Field<int?>("Priority") ?? 1);

            SetRefreshToken(row.Field<string?>("RefreshToken"));

            SetEmail(row.Field<string?>("Email"));

            SetQuickCode(row.Field<string?>("QuickCode"));

            SetSignature(row.Field<byte[]?>("Signature"));
        }

        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");
            AutoId = autoId;
        }

        public void SetUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new AppException("UserId could not be empty");

            UserId = userId.Trim();
        }

        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password could not be empty");

            if (password.Length < 6)
                throw new AppException("Password must be at least 6 characters long");

            Password = password;
        }

        public void SetUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new AppException("UserName could not be empty");

            UserName = userName.Trim();
        }

        public void SetRefreshToken(string? refreshToken)
        {
            RefreshToken = refreshToken;
        }

        public void SetPriority(int priority)
        {
            if (priority < 0)
                throw new AppException("Priority must be greater than or equal to 0");

            Priority = priority;
        }

        public void SetEmail(string? email)
        {
            if (!string.IsNullOrWhiteSpace(email)
                && !email.Contains("@"))
            {
                throw new AppException("Invalid email format");
            }

            Email = email;
        }

        public void SetQuickCode(string? quickCode)
        {
            QuickCode = quickCode;
        }

        public void SetSignature(byte[]? signature)
        {
            Signature = signature;
        }

        public void UpdateRefreshToken(string refreshToken)
        {
            SetRefreshToken(refreshToken);
        }

        public void ChangePassword(string newPassword)
        {
            SetPassword(newPassword);
        }
    }
}
