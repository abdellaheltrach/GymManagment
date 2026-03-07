using GymManagement.Application.Common.DTOs;
using GymManagement.Application.Features.Receptionists.Queries.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Receptionists.Queries.Handlers
{
    public class GetReceptionistByIdQueryHandler(
        IUnitOfWork uow) : IRequestHandler<GetReceptionistByIdQuery, Result<ReceptionistDetailDto>>
    {
        public async Task<Result<ReceptionistDetailDto>> Handle(
            GetReceptionistByIdQuery query, CancellationToken ct)
        {
            var r = await uow.Receptionists.GetByIdAsync(query.Id, ct);

            if (r is null)
                return Result<ReceptionistDetailDto>.NotFound("Receptionist", query.Id);

            return Result<ReceptionistDetailDto>.Success(new ReceptionistDetailDto(
                r.Id,
                r.FirstName,
                r.LastName,
                r.Email,
                r.Phone,
                r.HireDate,
                r.IsActive,
                (Domain.Enums.ReceptionistPermission)r.Permissions,
                r.CreatedAt));
        }
    }
}
