using System.ComponentModel.DataAnnotations;

namespace JobLinkHub.Web.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class FutureDateAttribute : ValidationAttribute
{
    public FutureDateAttribute() : base("The deadline must be today or a future date.") { }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is DateTime date)
            return date.Date >= DateTime.Today;
        return true;
    }
}
