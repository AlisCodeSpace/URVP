using FEA.URVP.Application.Commands.News.Create;
using FEA.URVP.Application.Commands.News.Update;
using FluentValidation;

namespace FEA.URVP.Application.Validators.News;

public sealed class CreateNewsArticleCommandValidator
    : AbstractValidator<CreateNewsArticleCommand>
{
    public CreateNewsArticleCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(256);

        RuleFor(x => x.Excerpt)
            .NotEmpty().WithMessage("Excerpt is required.")
            .MaximumLength(1000);

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(64);

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(128);

        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Ticker is required.")
            .MaximumLength(256);

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required.");

        RuleFor(x => x.PublishedAt)
            .NotEmpty().WithMessage("Published date is required.");

        RuleFor(x => x.Slug)
            .MaximumLength(160)
            .When(x => x.Slug is not null);
    }
}

public sealed class UpdateNewsArticleCommandValidator
    : AbstractValidator<UpdateNewsArticleCommand>
{
    public UpdateNewsArticleCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(256);

        RuleFor(x => x.Excerpt)
            .NotEmpty().WithMessage("Excerpt is required.")
            .MaximumLength(1000);

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(64);

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(128);

        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Ticker is required.")
            .MaximumLength(256);

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required.");

        RuleFor(x => x.PublishedAt)
            .NotEmpty().WithMessage("Published date is required.");

        RuleFor(x => x.Slug)
            .MaximumLength(160)
            .When(x => x.Slug is not null);
    }
}
