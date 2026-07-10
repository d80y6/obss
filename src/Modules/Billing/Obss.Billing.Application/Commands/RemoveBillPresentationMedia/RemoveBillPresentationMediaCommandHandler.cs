using MediatR;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RemoveBillPresentationMedia;

public sealed class RemoveBillPresentationMediaCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveBillPresentationMediaCommand, Result>
{
    public async Task<Result> Handle(RemoveBillPresentationMediaCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        account.RemoveBillPresentationMedia(request.MediaId);
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
