using StudentManagementApp.Models;

namespace StudentManagementApp.Services;

public interface IStudentService
{
    void Add(Student student);
    IReadOnlyList<Student> GetAll();
    Student? GetById(int id);
    void UpdateGrade(int id, double grade);
    void Delete(int id);
}
