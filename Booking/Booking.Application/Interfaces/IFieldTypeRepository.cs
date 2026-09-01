using CommonLibs.DTOs;
using Booking.Contract.DTOs;

namespace Booking.Application.Interfaces
{
    public interface IFieldTypeRepository
    {
        Task<FieldTypeOutputDTO> CreateFieldType(FieldTypeInputDTO input, CancellationToken cancellationToken);
        Task<FieldTypeOutputDTO> UpdateFieldType(FieldTypeInputDTO input, CancellationToken cancellationToken);
        Task<FieldTypeOutputDTO> GetFieldTypeById(string fieldTypeId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<FieldTypeOutputDTO>> GetAllFieldTypes(FieldTypeFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteFieldType(DeleteFieldTypeInputDTO input, CancellationToken cancellationToken);
    }
}
