using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.SendDunningNotice;

public sealed record SendDunningNoticeCommand(Guid CaseId) : IRequest<Result<CollectionCaseDto>>;
