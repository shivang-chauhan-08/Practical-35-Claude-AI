using StudentManagementApp.Application.Interfaces;
using StudentManagementApp.Domain.Entities;

namespace StudentManagementApp.Infrastructure.Repositories;

/// <summary>
/// In-memory student repository. IDs are auto-incremented and never reused after deletion.
/// </summary>
public class StudentRepository : IStudentRepository
{
    private readonly List<Student> _students = [];
    private int _nextId = 1;

    public void Add(Student student)
    {
        student.Id = _nextId++;
        _students.Add(student);
    }

    public IReadOnlyList<Student> GetAll() => _students.AsReadOnly();

    public Student? GetById(int id) =>
        _students.FirstOrDefault(s => s.Id == id);

    public void Update(Student student)
    {
        var index = _students.FindIndex(s => s.Id == student.Id);
        if (index >= 0)
            _students[index] = student;
    }

    public void Delete(int id)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);
        if (student is not null)
            _students.Remove(student);
    }

    public bool EmailExists(string email, int? excludeId = null)
    {
        var normalized = email.Trim().ToLower();
        return _students.Any(s =>
            s.Email.ToLower() == normalized &&
            (excludeId is null || s.Id != excludeId.Value));
    }
}
