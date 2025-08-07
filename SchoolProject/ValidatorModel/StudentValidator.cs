using FluentValidation;
using Models.Models.RequestModel;
using Models.Models.School;

namespace Models.Models.ValidatorModel;

public class StudentValidator : AbstractValidator<StudentRequestModel>
{
    public StudentValidator()
    {
        RuleFor(s => s.Name).NotNull().NotEmpty().WithMessage("Name is required").MaximumLength(20);
        RuleFor(s => s.Gender).NotEmpty().WithMessage("Gender is required")
            .Must(gender => gender.Equals("Male") || gender.Equals("Female")).WithMessage("Gender must be Male, Female");
        RuleFor(s => s.Age).NotEmpty().WithMessage("Age is required").GreaterThan(0).WithMessage("Age must be greater than 0").LessThan(20).WithMessage("Age must be less than 20");
        RuleFor(s => s.Email).NotEmpty().WithMessage("Email is required").EmailAddress();
        RuleFor(s => s.Course).NotEmpty().WithMessage("Course is required");
    }
}