using GymManagement.Application.Common.Models;
using MediatR;

namespace GymManagement.Application.Common.Behaviours;

public interface ICommandBase { }
public interface ICommand : ICommandBase, IRequest<Result> { }
public interface ICommand<TResponse> : ICommandBase, IRequest<Result<TResponse>> { }
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
