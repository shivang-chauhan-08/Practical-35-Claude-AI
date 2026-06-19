using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StudentManagementApp.Application.DTOs;
using StudentManagementApp.Application.Interfaces;
using StudentManagementApp.Infrastructure.DependencyInjection;

namespace StudentManagementApp;

internal class Program
{
    private static IStudentService _service = null!;
    private static IPromptLogger _promptLogger = null!;

    static void Main()
    {
        // ── DI Setup ──────────────────────────────────────────────────────────
        var services = new ServiceCollection();

        var baseDir = AppContext.BaseDirectory;
        var logFilePath    = Path.Combine(baseDir, "Logs", "application-log.txt");
        var promptLogPath  = Path.Combine(baseDir, "Logs", "prompts.txt");

        services.AddInfrastructure(logFilePath, promptLogPath);
        var provider = services.BuildServiceProvider();

        _service      = provider.GetRequiredService<IStudentService>();
        _promptLogger = provider.GetRequiredService<IPromptLogger>();

        // ── Menu Loop ─────────────────────────────────────────────────────────
        Console.Title = "Student Management System";

        while (true)
        {
            ShowMenu();
            var choice = Console.ReadLine()?.Trim();
            _promptLogger.Log($"Menu selection: {choice}");

            Console.WriteLine();
            try
            {
                switch (choice)
                {
                    case "1": AddStudent();      break;
                    case "2": ViewAllStudents(); break;
                    case "3": FindStudentById(); break;
                    case "4": UpdateGrade();     break;
                    case "5": DeleteStudent();   break;
                    case "6":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please enter a number between 1 and 6.");
                        break;
                }
            }
            catch (ValidationException ex)
            {
                Console.WriteLine("Validation Error(s):");
                foreach (var error in ex.Errors)
                    Console.WriteLine($"  - {error.ErrorMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    // ── Menu ──────────────────────────────────────────────────────────────────

    private static void ShowMenu()
    {
        Console.WriteLine("==============================");
        Console.WriteLine("   Student Management System  ");
        Console.WriteLine("==============================");
        Console.WriteLine("1. Add Student");
        Console.WriteLine("2. View All Students");
        Console.WriteLine("3. Find Student By Id");
        Console.WriteLine("4. Update Grade");
        Console.WriteLine("5. Delete Student");
        Console.WriteLine("6. Exit");
        Console.WriteLine("==============================");
        Console.Write("Select an option: ");
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    private static void AddStudent()
    {
        Console.WriteLine("--- Add New Student ---");

        var name           = ReadName("Enter Name: ");
        var email          = ReadEmail("Enter Email: ");
        var enrollmentDate = ReadDate("Enter Enrollment Date (yyyy-MM-dd, not future): ");
        var grade          = ReadGrade("Enter Grade (0-100): ");

        var dto = new StudentDto
        {
            Name           = name,
            Email          = email,
            EnrollmentDate = enrollmentDate,
            Grade          = grade
        };

        _promptLogger.Log($"Add Student → Name={name}, Email={email}, Date={enrollmentDate:yyyy-MM-dd}, Grade={grade}");

        var added = _service.Add(dto);
        Console.WriteLine($"Student '{added.Name}' added successfully with ID: {added.Id}.");
    }

    private static void ViewAllStudents()
    {
        Console.WriteLine("--- All Students ---");
        _promptLogger.Log("View All Students");

        var students = _service.GetAll();
        if (students.Count == 0)
        {
            Console.WriteLine("No students found.");
            return;
        }

        foreach (var s in students)
            Console.WriteLine(FormatStudent(s));
    }

    private static void FindStudentById()
    {
        Console.WriteLine("--- Find Student By Id ---");
        var id = ReadInt("Enter Student Id: ");
        _promptLogger.Log($"Find Student By Id → {id}");

        var student = _service.GetById(id);
        if (student is null)
            Console.WriteLine($"No student found with Id {id}.");
        else
            Console.WriteLine(FormatStudent(student));
    }

    private static void UpdateGrade()
    {
        Console.WriteLine("--- Update Student Grade ---");
        var id    = ReadInt("Enter Student Id: ");
        var grade = ReadGrade("Enter New Grade (0-100): ");
        _promptLogger.Log($"Update Grade → StudentId={id}, NewGrade={grade}");

        _service.UpdateGrade(id, grade);
        Console.WriteLine("Grade updated successfully.");
    }

    private static void DeleteStudent()
    {
        Console.WriteLine("--- Delete Student ---");
        var id = ReadInt("Enter Student Id: ");
        _promptLogger.Log($"Delete Student → Id={id}");

        _service.Delete(id);
        Console.WriteLine($"Student with Id {id} deleted successfully.");
    }

    // ── Input Helpers ─────────────────────────────────────────────────────────

    private static string FormatStudent(StudentDto s) =>
        $"[ID: {s.Id}] {s.Name} | {s.Email} | Enrolled: {s.EnrollmentDate:yyyy-MM-dd} | Grade: {s.Grade:F1} | Created: {s.CreatedOn:yyyy-MM-dd HH:mm}";

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var line = Console.ReadLine();
            if (line is null) ExitOnEof();
            if (int.TryParse(line, out int value) && value > 0)
                return value;
            Console.WriteLine("Invalid input. Please enter a positive integer.");
        }
    }

    private static string ReadName(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var line = Console.ReadLine();
            if (line is null) ExitOnEof();
            var value = line!.Trim();
            if (value.Length >= 3 && value.Length <= 100 &&
                System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z ]+$"))
                return value;
            Console.WriteLine("Name must be 3–100 characters, letters and spaces only.");
        }
    }

    private static string ReadEmail(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var line = Console.ReadLine();
            if (line is null) ExitOnEof();
            var value = line!.Trim();
            if (System.Text.RegularExpressions.Regex.IsMatch(value,
                    @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$"))
                return value;
            Console.WriteLine("Invalid email. Use format: user@example.com");
        }
    }

    private static double ReadGrade(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var line = Console.ReadLine();
            if (line is null) ExitOnEof();
            if (double.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
                && value >= 0 && value <= 100)
                return value;
            Console.WriteLine("Grade must be a number between 0 and 100.");
        }
    }

    private static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var line = Console.ReadLine();
            if (line is null) ExitOnEof();
            if (!DateTime.TryParseExact(line, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                Console.WriteLine("Invalid format. Use yyyy-MM-dd (e.g., 2024-09-01).");
                continue;
            }
            if (date.Date > DateTime.Today)
            {
                Console.WriteLine("Enrollment date cannot be a future date.");
                continue;
            }
            return date;
        }
    }

    private static void ExitOnEof()
    {
        Console.WriteLine("\nInput stream closed. Exiting.");
        Environment.Exit(0);
    }
}
