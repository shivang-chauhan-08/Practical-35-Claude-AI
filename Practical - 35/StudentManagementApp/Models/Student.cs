using System.Text.RegularExpressions;

namespace StudentManagementApp.Models;

public class Student
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private DateTime _enrollmentDate;
    private double _grade;

    public int Id { get; internal set; }

    public string Name
    {
        get => _name;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty.");
            var trimmed = value.Trim();
            if (trimmed.Length < 2 || trimmed.Length > 100)
                throw new ArgumentException("Name must be between 2 and 100 characters.");
            if (!Regex.IsMatch(trimmed, @"^[a-zA-Z ]+$"))
                throw new ArgumentException("Name must contain letters only (no digits or special characters).");
            _name = trimmed;
        }
    }

    public string Email
    {
        get => _email;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.");
            var trimmed = value.Trim();
            if (!Regex.IsMatch(trimmed, @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$"))
                throw new ArgumentException("Email format is invalid (e.g. user@example.com).");
            _email = trimmed.ToLower();
        }
    }

    public DateTime EnrollmentDate
    {
        get => _enrollmentDate;
        private set
        {
            if (value.Date > DateTime.Today)
                throw new ArgumentException("Enrollment date cannot be a future date.");
            _enrollmentDate = value.Date;
        }
    }

    public double Grade
    {
        get => _grade;
        internal set
        {
            if (value < 0 || value > 100)
                throw new ArgumentException("Grade must be between 0 and 100.");
            _grade = value;
        }
    }

    public Student(string name, string email, DateTime enrollmentDate, double grade)
    {
        Name = name;
        Email = email;
        EnrollmentDate = enrollmentDate;
        Grade = grade;
    }

    public override string ToString() =>
        $"[ID: {Id}] {Name} | {Email} | Enrolled: {EnrollmentDate:yyyy-MM-dd} | Grade: {Grade:F1}";
}
