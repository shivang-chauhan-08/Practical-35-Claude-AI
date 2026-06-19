using StudentManagementApp.Domain.Entities;

namespace StudentManagementApp.Application.Interfaces;

/// <summary>
/// Repository abstraction for student persistence. Infrastructure implements this; Application depends on it.
/// </summary>
public interface IStudentRepository
{
    void Add(Student student);
    IReadOnlyList<Student> GetAll();
    Student? GetById(int id);
    void Update(Student student);
    void Delete(int id);

    /// <param name="excludeId">When updating, exclude the owner's own ID from the uniqueness check.</param>
    bool EmailExists(string email, int? excludeId = null);
}
