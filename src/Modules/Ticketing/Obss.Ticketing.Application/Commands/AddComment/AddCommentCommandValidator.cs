using FluentValidation;

namespace Obss.Ticketing.Application.Commands.AddComment;

internal sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(4000).WithMessage("Comment content must not exceed 4000 characters.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Author ID is required.")
            .MaximumLength(100).WithMessage("Author ID must not exceed 100 characters.");

        RuleFor(x => x.AuthorName)
            .NotEmpty().WithMessage("Author name is required.")
            .MaximumLength(200).WithMessage("Author name must not exceed 200 characters.");
    }
}
