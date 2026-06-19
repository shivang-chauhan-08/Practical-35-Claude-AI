using StudentManagementApp.Application.DTOs;

namespace StudentManagementApp.Application.Interfaces;

/// <summary>
/// Application-level student operations. Program.cs depends on this abstraction, not the concrete service.
/// </summary>
public interface IStudentService
{
    StudentDto Add(StudentDto studentDto);
    IReadOnlyList<StudentDto> GetAll();
    StudentDto? GetById(int id);
    void UpdateGrade(int id, double grade);
    void Delete(int id);
}
