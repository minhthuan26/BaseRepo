using CommonLibs.Entities;
using CommonLibs.Utils;
using System.Data;

namespace AdministrativeUnit.Domain.Entities
{
    public class Province : BaseEntity
    {
        // --- Properties ---
        public string Code { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string? Type { get; private set; }
        public string? NameWithType { get; private set; }
        public string? Slug { get; private set; }

        // --- Constructors ---
        protected Province()
        {
        }

        public Province(string code, string name)
        {
            SetCode(code);
            SetName(name);
        }

        public Province(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetCode(row.Field<string>("Code") ?? throw new AppException("Code is required"));
            SetName(row.Field<string>("Name") ?? throw new AppException("Name is required"));
            SetType(row.Field<string?>("Type"));
            SetNameWithType(row.Field<string?>("NameWithType"));
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

        public void SetSlug(string? slug) => Slug = slug;
    }
}
