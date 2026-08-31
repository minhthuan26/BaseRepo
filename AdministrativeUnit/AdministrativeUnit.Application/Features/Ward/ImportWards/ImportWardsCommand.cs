using AdministrativeUnit.Contract.DTOs;
using MediatR;

namespace AdministrativeUnit.Application.Features.Ward.ImportWards
{
    public class ImportWardsCommand : IRequest<List<WardOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
    }
}
