using CommonLibs.Entities;
using CommonLibs.Utils;
using System;
using System.Data;

namespace Site.Domain.Entities
{
    public class Function : BaseEntity
    {
        // --- Properties ---
        public string FunctionId { get; private set; } = null!;
        public string FunctionName { get; private set; } = null!;
        public string ScreenId { get; private set; } = null!;

        // --- Constructors ---
        protected Function()
        {
        }

        public Function(string functionId, string functionName, string screenId)
        {
            SetFunctionId(functionId);
            SetFunctionName(functionName);
            SetScreenId(screenId);
        }

        public Function(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetFunctionId(row.Field<string>("FunctionId") ?? throw new AppException("FunctionId is required"));
            SetFunctionName(row.Field<string>("FunctionName") ?? throw new AppException("FunctionName is required"));
            SetScreenId(row.Field<string>("ScreenId") ?? throw new AppException("ScreenId is required"));
        }

        // --- Guard Methods / Setters ---
        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetFunctionId(string functionId)
        {
            if (string.IsNullOrWhiteSpace(functionId))
                throw new AppException("FunctionId could not be empty");

            FunctionId = functionId.Trim();
        }

        public void SetFunctionName(string functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
                throw new AppException("FunctionName could not be empty");

            FunctionName = functionName.Trim();
        }

        public void SetScreenId(string screenId)
        {
            if (string.IsNullOrWhiteSpace(screenId))
                throw new AppException("ScreenId could not be empty");

            ScreenId = screenId.Trim();
        }
    }
}