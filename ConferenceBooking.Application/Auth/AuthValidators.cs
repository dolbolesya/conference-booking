using FluentValidation;

namespace ConferenceBooking.Application.Auth;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Електронна адреса обов'язкова.")
            .EmailAddress().WithMessage("Некоректна електронна адреса.")
            .MaximumLength(320);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обов'язковий.")
            .MinimumLength(8).WithMessage("Пароль має містити щонайменше 8 символів.")
            .MaximumLength(128)
            .Matches("[A-Za-z]").WithMessage("Пароль має містити літери.")
            .Matches("[0-9]").WithMessage("Пароль має містити цифри.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ім'я обов'язкове.")
            .MaximumLength(200);
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}