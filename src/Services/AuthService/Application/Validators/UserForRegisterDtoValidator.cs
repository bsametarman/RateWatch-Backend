using FluentValidation;
using RateWatch.AuthService.Application.DTOs;

namespace RateWatch.AuthService.Application.Validators
{
    public class UserForRegisterDtoValidator : AbstractValidator<UserForRegisterDto>
    {
        public UserForRegisterDtoValidator()
        {
            RuleFor(user => user.Email)
                .NotEmpty().WithMessage("Email address is required.")
                .EmailAddress().WithMessage("Please enter a valid email address.");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Enter password.")
                .MinimumLength(6).WithMessage("Password must be at least 6 character long.");
        }
    }
}
