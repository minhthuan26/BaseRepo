using CommonLibs.DTOs;
using Booking.Contract.DTOs;

namespace Booking.Application.Interfaces
{
    public interface IFieldRepository
    {
        Task<FieldOutputDTO> CreateField(FieldInputDTO input, CancellationToken cancellationToken);
        Task<FieldOutputDTO> UpdateField(FieldInputDTO input, string callerSiteId, CancellationToken cancellationToken);
        Task<FieldOutputDTO> GetFieldById(string fieldId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<FieldOutputDTO>> GetAllFields(FieldFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteField(DeleteFieldInputDTO input, string callerSiteId, CancellationToken cancellationToken);
    }
}
