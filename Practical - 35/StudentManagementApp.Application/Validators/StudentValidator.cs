using FluentValidation;
using StudentManagementApp.Application.DTOs;

namespace StudentManagementApp.Application.Validators;

/// <summary>
/// FluentValidation rules for StudentDto. Executed inside StudentService before any repository call.
/// </summary>
public class StudentValidator : AbstractValidator<StudentDto>
{
    public StudentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required (e.g. user@example.com).");

        RuleFor(x => x.Grade)
            .InclusiveBetween(0, 100).WithMessage("Grade must be between 0 and 100.");

        RuleFor(x => x.EnrollmentDate)
            .Must(date => date.Date <= DateTime.Today)
            .WithMessage("Enrollment date cannot be a future date.");
    }
}
