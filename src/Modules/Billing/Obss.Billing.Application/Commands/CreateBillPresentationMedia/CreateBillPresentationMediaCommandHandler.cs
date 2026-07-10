using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.CreateBillPresentationMedia;

public sealed class CreateBillPresentationMediaCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBillPresentationMediaCommand, Result<BillPresentationMediaDto>>
{
    public async Task<Result<BillPresentationMediaDto>> Handle(CreateBillPresentationMediaCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure<BillPresentationMediaDto>(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        if (!Enum.TryParse<MediaType>(request.MediaType, true, out var mediaType))
            return Result.Failure<BillPresentationMediaDto>(Error.Validation($"Invalid media type: '{request.MediaType}'."));

        var media = new BillPresentationMedia(mediaType, request.EmailAddress, request.PaperFormat, request.Language, request.IsPreferred);
        account.AddBillPresentationMedia(media);
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(media.Adapt<BillPresentationMediaDto>());
    }
}
