using CommonLibs.Entities;
using CommonLibs.Utils;
using System.Data;

namespace AdministrativeUnit.Domain.Entities
{
    public class Ward : BaseEntity
    {
        // --- Properties ---
        public string Code { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string? Type { get; private set; }
        public string? NameWithType { get; private set; }
        public string ProvinceCode { get; private set; } = null!;
        public string? Slug { get; private set; }

        // --- Constructors ---
        protected Ward()
        {
        }

        public Ward(string code, string name, string provinceCode)
        {
            SetCode(code);
            SetName(name);
            SetProvinceCode(provinceCode);
        }

        public Ward(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetCode(row.Field<string>("Code") ?? throw new AppException("Code is required"));
            SetName(row.Field<string>("Name") ?? throw new AppException("Name is required"));
            SetType(row.Field<string?>("Type"));
            SetNameWithType(row.Field<string?>("NameWithType"));
            SetProvinceCode(row.Field<string>("ProvinceCode") ?? throw new AppException("ProvinceCode is required"));
            SetSlug(row.Field<string?>("Slug"));
        }

        // --- Guard Methods / Setters ---
        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new AppException("Code could not be empty");

            Code = code.Trim();
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new AppException("Name could not be empty");

            Name = name.Trim();
        }

        public void SetType(string? type) => Type = type;

        public void SetNameWithType(string? nameWithType) => NameWithType = nameWithType;

        public void SetProvinceCode(string provinceCode)
        {
            if (string.IsNullOrWhiteSpace(provinceCode))
                throw new AppException("ProvinceCode could not be empty");

            ProvinceCode = provinceCode.Trim();
        }

        public void SetSlug(string? slug) => Slug = slug;
    }
}
