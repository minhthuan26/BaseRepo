using CommonLibs.Entities;
using CommonLibs.Utils;
using System.Data;

namespace Booking.Domain.Entities
{
    public class FieldType : BaseEntity
    {
        public string FieldTypeId { get; private set; } = null!;
        public string FieldTypeName { get; private set; } = null!;

        protected FieldType()
        {
        }

        public FieldType(string fieldTypeId, string fieldTypeName)
        {
            SetFieldTypeId(fieldTypeId);
            SetFieldTypeName(fieldTypeName);
        }

        public FieldType(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetFieldTypeId(row.Field<string>("FieldTypeId") ?? throw new AppException("FieldTypeId is required"));
            SetFieldTypeName(row.Field<string>("FieldTypeName") ?? throw new AppException("FieldTypeName is required"));
        }

        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetFieldTypeId(string fieldTypeId)
        {
            if (string.IsNullOrWhiteSpace(fieldTypeId))
                throw new AppException("FieldTypeId could not be empty");

            FieldTypeId = fieldTypeId.Trim();
        }

        public void SetFieldTypeName(string fieldTypeName)
        {
            if (string.IsNullOrWhiteSpace(fieldTypeName))
                throw new AppException("FieldTypeName could not be empty");

            FieldTypeName = fieldTypeName.Trim();
        }
    }
}
