using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.UpdateBillPresentationMedia;

public sealed class UpdateBillPresentationMediaCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBillPresentationMediaCommand, Result<BillPresentationMediaDto>>
{
    public async Task<Result<BillPresentationMediaDto>> Handle(UpdateBillPresentationMediaCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure<BillPresentationMediaDto>(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        var media = account.BillPresentationMedia.FirstOrDefault(m => m.Id == request.MediaId);
        if (media is null)
            return Result.Failure<BillPresentationMediaDto>(Error.NotFound("BillPresentationMedia", request.MediaId));

        media.Update(request.EmailAddress, request.PaperFormat, request.Language, request.IsPreferred);
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(media.Adapt<BillPresentationMediaDto>());
    }
}
