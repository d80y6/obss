using FluentValidation;

namespace Obss.Audit.Application.Commands.PurgeOldEntries;

internal sealed class PurgeOldEntriesCommandValidator : AbstractValidator<PurgeOldEntriesCommand>
{
    public PurgeOldEntriesCommandValidator()
    {
    }
}
