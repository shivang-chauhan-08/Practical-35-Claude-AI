using System.Globalization;
using StudentManagementApp.Models;
using StudentManagementApp.Services;

namespace StudentManagementApp;

internal class Program
{
    private static readonly IStudentService _service = new StudentService();

    static void Main()
    {
        Console.Title = "Student Management System";

        while (true)
        {
            ShowMenu();
            var choice = Console.ReadLine()?.Trim();

            Console.WriteLine();
            try
            {
                switch (choice)
                {
                    case "1": AddStudent(); break;
                    case "2": ViewAllStudents(); break;
                    case "3": FindStudentById(); break;
                    case "4": UpdateGrade(); break;
                    case "5": DeleteStudent(); break;
                    case "6":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please enter a number between 1 and 6.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

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

    private static void AddStudent()
    {
        Console.WriteLine("--- Add New Student ---");

        string name = ReadName("Enter Name (letters only): ");
        string email = ReadEmail("Enter Email: ");
        DateTime enrollmentDate = ReadDate("Enter Enrollment Date (yyyy-MM-dd, not future): ");
        double grade = ReadGrade("Enter Grade (0-100): ");

        var student = new Student(name, email, enrollmentDate, grade);
        _service.Add(student);
        Console.WriteLine($"Student '{name}' added successfully with ID: {student.Id}.");
    }

    private static void ViewAllStudents()
    {
        Console.WriteLine("--- All Students ---");
        var students = _service.GetAll();

        if (students.Count == 0)
        {
            Console.WriteLine("No students found.");
            return;
        }

        foreach (var student in students)
            Console.WriteLine(student);
    }

    private static void FindStudentById()
    {
        Console.WriteLine("--- Find Student By Id ---");
        int id = ReadInt("Enter Student Id: ");

        var student = _service.GetById(id);
        if (student is null)
            Console.WriteLine($"No student found with Id {id}.");
        else
            Console.WriteLine(student);
    }

    private static void UpdateGrade()
    {
        Console.WriteLine("--- Update Student Grade ---");
        int id = ReadInt("Enter Student Id: ");
        double grade = ReadGrade("Enter New Grade (0-100): ");

        _service.UpdateGrade(id, grade);
        Console.WriteLine("Grade updated successfully.");
    }

    private static void DeleteStudent()
    {
        Console.WriteLine("--- Delete Student ---");
        int id = ReadInt("Enter Student Id: ");

        _service.Delete(id);
        Console.WriteLine($"Student with Id {id} deleted successfully.");
    }

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
            if (value.Length < 2)
            {
                Console.WriteLine("Name must be at least 2 characters.");
                continue;
            }
            if (value.Length > 100)
            {
                Console.WriteLine("Name must not exceed 100 characters.");
                continue;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z ]+$"))
            {
                Console.WriteLine("Name must contain letters only (no digits or special characters).");
                continue;
            }
            return value;
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
            if (!System.Text.RegularExpressions.Regex.IsMatch(value,
                    @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$"))
            {
                Console.WriteLine("Invalid email. Use format: user@example.com");
                continue;
            }
            return value;
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
