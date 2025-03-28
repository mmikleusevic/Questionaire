using System.ComponentModel.DataAnnotations;
using Shared.Models;

namespace Shared.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class NotOwnParentAttribute(string errorMessage = "A category cannot be its own parent.")
    : ValidationAttribute(errorMessage)
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var parentCategoryIdValue = (int?)value;

        if (!(validationContext.ObjectInstance is CategoryExtendedDto category))
        {
            return new ValidationResult(
                $"Attribute {nameof(NotOwnParentAttribute)} must be applied to a property within a {nameof(CategoryExtendedDto)}.");
        }

        int categoryId = category.Id;

        if (!parentCategoryIdValue.HasValue || categoryId == 0)
        {
            return ValidationResult.Success;
        }

        if (categoryId == parentCategoryIdValue.Value)
        {
            return new ValidationResult(
                FormatErrorMessage(validationContext.DisplayName ?? validationContext.MemberName!),
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}