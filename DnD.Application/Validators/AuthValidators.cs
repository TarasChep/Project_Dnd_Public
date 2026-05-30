using System.Data;
using DnD.Application.DTOs;
using FluentValidation;

namespace DnD.Application.Validators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email Field is required")
            .EmailAddress()
            .WithMessage("Wrong  Format for email || Exemple (user@example.com)")
            .Must(BeAvalidEmail);
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Name is Required")
            .MinimumLength(3)
            .WithMessage("Name has to be at leas 3 symbols long");
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is  required")
            .MinimumLength(6)
            .WithMessage("Password has to be 6 or more symbols");
    }

    private bool BeAvalidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return false;

        var domain = parts[1];

        return domain.Contains(".")
            && !domain.StartsWith(".")
            && !domain.EndsWith(".")
            && domain.Split('.').Last().Length >= 2;
    }
}

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email обов'язковий")
            .EmailAddress()
            .WithMessage("Введіть коректний Email");

        RuleFor(x => x.Password).NotEmpty().WithMessage("Enter the password");
    }
}
