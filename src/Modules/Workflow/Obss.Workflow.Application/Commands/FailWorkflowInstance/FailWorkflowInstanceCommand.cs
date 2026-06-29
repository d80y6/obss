using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Workflow.Application.Commands.FailWorkflowInstance;

public sealed record FailWorkflowInstanceCommand(Guid InstanceId, string Error) : IRequest<Result>;
