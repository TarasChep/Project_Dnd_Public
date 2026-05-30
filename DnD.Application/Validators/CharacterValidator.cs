using System.Data;
using DnD.Application.DTOs;
using DnD.Domain.Entities;
using FluentValidation;

namespace DnD.Application.Validators;

public class CharacterValidator : AbstractValidator<CharacterCreateDto>
{
    public CharacterValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .WithMessage("Character name is required")
            .MaximumLength(128)
            .WithMessage("Name is too long");

        RuleFor(c => c.Strength).InclusiveBetween(1, 30);
        RuleFor(c => c.Dexterity).InclusiveBetween(1, 30);
        RuleFor(c => c.Constitution).InclusiveBetween(1, 30);
        RuleFor(c => c.Intelligence).InclusiveBetween(1, 30);
        RuleFor(c => c.Wisdom).InclusiveBetween(1, 30);
        RuleFor(c => c.Charisma).InclusiveBetween(1, 30);

        RuleFor(c => c.MaxHp).GreaterThan(0);
        RuleFor(c => c.ArmorClass).GreaterThan(0);
        RuleFor(c => c.CurrentHp)
            .LessThanOrEqualTo(c => c.MaxHp)
            .WithMessage("Current hp has to be less than maxHp");

        RuleFor(c => c.Race).MaximumLength(64);
        RuleFor(c => c.Class).MaximumLength(64);

        // Рівень персонажа (1-20 за класикою)

        RuleFor(c => c.CurrentXp)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Досвід не може бути від'ємним");
    }
}
