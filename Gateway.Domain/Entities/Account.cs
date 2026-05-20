using CommonLibs.Entities;
using CommonLibs.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Domain.Entities
{
    public class Account : BaseEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? AccountName { get; private set; }
        public string? Password { get; private set; }
        public string? RefreshToken { get; private set; }
        public int Priority { get; set; } = 1;

        public Account() { }
        public Account(string Id, string accountName, string password, string? refreshToken, int priority)
        {
            SetId(Id);
            SetAccountName(accountName);
            SetPassword(password);
            SetRefreshToken(refreshToken);
            SetPriority(priority);
        }
        public Account(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetId(row.Field<string>("Id")
                  ?? throw new AppException("Id is required"));

            SetAccountName(row.Field<string>("AccountName")
                  ?? throw new AppException("AccountName is required"));

            SetPassword(row.Field<string>("Password")
                  ?? throw new AppException("Password is required"));

            SetRefreshToken(row.Field<string>("RefreshToken"));
            SetPriority(row.Field<int>("Priority"));
        }

        public void SetId(string id)
        {
            Id = id;
        }

        public void SetAccountName(string accountName)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new AppException("Account name could not be empty");
            }
            AccountName = accountName;
        }

        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new AppException("Password could not be empty");
            }
            if (password.Length < 6)
            {
                throw new AppException("Password must be at least 6 characters long");
            }
            Password = password;
        }

        public void SetRefreshToken(string? refreshToken)
        {
            RefreshToken = refreshToken;
        }
        public void SetPriority(int priority)
        {
            if (priority < 0)
            {
                throw new AppException("Priority must be greater than -1");
            }
            Priority = priority;
        }
    }
}
