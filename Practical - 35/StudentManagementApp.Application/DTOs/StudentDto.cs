namespace StudentManagementApp.Application.DTOs;

/// <summary>
/// Data Transfer Object for Student. Crosses layer boundaries; never exposes the domain entity directly.
/// </summary>
public class StudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public double Grade { get; set; }
    public DateTime CreatedOn { get; set; }
}
