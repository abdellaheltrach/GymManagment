using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;

namespace GymManagement.Application.Features.Dashboard.Queries.Models;

public record GetAdminDashboardQuery : IQuery<AdminDashboardDto>;
