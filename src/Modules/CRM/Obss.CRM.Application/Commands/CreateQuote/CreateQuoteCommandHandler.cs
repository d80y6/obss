using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Application.Commands.CreateQuote;

public sealed class CreateQuoteCommandHandler : IRequestHandler<CreateQuoteCommand, Result<QuoteDto>>
{
    private readonly IQuoteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateQuoteCommandHandler(IQuoteRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<QuoteDto>> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = Quote.Create(
            request.TenantId,
            request.CustomerId,
            request.ExternalId,
            request.Category,
            request.Description,
            request.ValidFrom,
            request.ValidUntil,
            request.ExpectedQuoteCompletionDate,
            request.ExpectedFulfillmentStartDate);

        await _repository.AddAsync(quote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(quote.Adapt<QuoteDto>());
    }
}
