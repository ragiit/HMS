using FluentValidation;
using Identity.Application.Commands;

namespace Identity.Application.Validators;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password harus memiliki minimal satu huruf kapital.")
            .Matches("[0-9]").WithMessage("Password harus memiliki minimal satu angka.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password harus memiliki minimal satu karakter khusus.");
    }
}