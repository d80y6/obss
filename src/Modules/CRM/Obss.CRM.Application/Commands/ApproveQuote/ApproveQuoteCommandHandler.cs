using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Application.Commands.ApproveQuote;

public sealed class ApproveQuoteCommandHandler : IRequestHandler<ApproveQuoteCommand, Result>
{
    private readonly IQuoteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveQuoteCommandHandler(IQuoteRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ApproveQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (quote is null)
            return Result.Failure(Error.NotFound(nameof(Quote), request.Id));

        quote.Approve();
        await _repository.UpdateAsync(quote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
