using StudentManagementApp.Models;

namespace StudentManagementApp.Services;

public class StudentService : IStudentService
{
    private readonly List<Student> _students = [];
    private int _nextId = 1;

    public void Add(Student student)
    {
        if (student is null)
            throw new ArgumentNullException(nameof(student), "Student cannot be null.");
        if (_students.Any(s => s.Email == student.Email))
            throw new InvalidOperationException($"A student with email '{student.Email}' already exists.");

        student.Id = _nextId++;
        _students.Add(student);
    }

    public IReadOnlyList<Student> GetAll() => _students.AsReadOnly();

    public Student? GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be a positive integer.", nameof(id));
        return _students.FirstOrDefault(s => s.Id == id);
    }

    public void UpdateGrade(int id, double grade)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be a positive integer.", nameof(id));

        var student = GetById(id)
            ?? throw new KeyNotFoundException($"No student found with Id {id}.");

        student.Grade = grade;
    }

    public void Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be a positive integer.", nameof(id));

        var student = GetById(id)
            ?? throw new KeyNotFoundException($"No student found with Id {id}.");

        _students.Remove(student);
    }
}
