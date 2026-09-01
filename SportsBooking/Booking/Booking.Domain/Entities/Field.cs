using CommonLibs.Entities;
using CommonLibs.Utils;
using System.Data;

namespace Booking.Domain.Entities
{
    public enum FieldOperationalStatus
    {
        Active = 1,
        Maintenance = 2
    }

    public class Field : BaseEntity
    {
        public string FieldId { get; private set; } = null!;
        public string SiteId { get; private set; } = null!;
        public string FieldTypeId { get; private set; } = null!;
        public string FieldName { get; private set; } = null!;
        public FieldOperationalStatus OperationalStatus { get; private set; } = FieldOperationalStatus.Active;

        protected Field()
        {
        }

        public Field(string fieldId, string siteId, string fieldTypeId, string fieldName, FieldOperationalStatus operationalStatus = FieldOperationalStatus.Active)
        {
            SetFieldId(fieldId);
            SetSiteId(siteId);
            SetFieldTypeId(fieldTypeId);
            SetFieldName(fieldName);
            SetOperationalStatus(operationalStatus);
        }

        public Field(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetFieldId(row.Field<string>("FieldId") ?? throw new AppException("FieldId is required"));
            SetSiteId(row.Field<string>("SiteId") ?? throw new AppException("SiteId is required"));
            SetFieldTypeId(row.Field<string>("FieldTypeId") ?? throw new AppException("FieldTypeId is required"));
            SetFieldName(row.Field<string>("FieldName") ?? throw new AppException("FieldName is required"));
            SetOperationalStatus((FieldOperationalStatus)row.Field<int>("OperationalStatus"));
        }

        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetFieldId(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new AppException("FieldId could not be empty");

            FieldId = fieldId.Trim();
        }

        public void SetSiteId(string siteId)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                throw new AppException("SiteId could not be empty");

            SiteId = siteId.Trim();
        }

        public void SetFieldTypeId(string fieldTypeId)
        {
            if (string.IsNullOrWhiteSpace(fieldTypeId))
                throw new AppException("FieldTypeId could not be empty");

            FieldTypeId = fieldTypeId.Trim();
        }

        public void SetFieldName(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new AppException("FieldName could not be empty");

            FieldName = fieldName.Trim();
        }

        public void SetOperationalStatus(FieldOperationalStatus operationalStatus)
        {
            OperationalStatus = operationalStatus;
        }
    }
}
