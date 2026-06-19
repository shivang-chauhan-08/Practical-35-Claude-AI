namespace StudentManagementApp.Domain.Entities;

/// <summary>
/// Core student domain entity. Validation is enforced at the Application layer via FluentValidation.
/// </summary>
public class Student : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public double Grade { get; set; }
}
