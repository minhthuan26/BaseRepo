using CommonLibs.Entities;
using CommonLibs.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Domain.Entities
{
    [Table("tbl_User")]
    public class User : BaseEntity
    {
        public long Id { get; set; }
        public string? UserId { get; private set; }
        public string? Password { get; private set; }
        public string? RefreshToken { get; private set; }
        public int Priority { get; set; } = 1;

        public User() { }
        public User(long id, string accountName, string password, string? refreshToken, int priority)
        {
            SetId(id);
            SetAccountName(accountName);
            SetPassword(password);
            SetRefreshToken(refreshToken);
            SetPriority(priority);
        }
        public User(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetId(row.Field<long>("Id"));

            SetAccountName(row.Field<string>("AccountName")
                  ?? throw new AppException("AccountName is required"));

            SetPassword(row.Field<string>("Password")
                  ?? throw new AppException("Password is required"));

            SetRefreshToken(row.Field<string>("RefreshToken"));
            SetPriority(row.Field<int>("Priority"));
        }

        public void SetId(long id)
        {
            Id = id;
        }

        public void SetAccountName(string accountName)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new AppException("Account name could not be empty");
            }
            UserId = accountName;
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
