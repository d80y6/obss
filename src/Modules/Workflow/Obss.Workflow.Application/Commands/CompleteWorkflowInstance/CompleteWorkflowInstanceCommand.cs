using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Workflow.Application.Commands.CompleteWorkflowInstance;

public sealed record CompleteWorkflowInstanceCommand(Guid InstanceId) : IRequest<Result>;
