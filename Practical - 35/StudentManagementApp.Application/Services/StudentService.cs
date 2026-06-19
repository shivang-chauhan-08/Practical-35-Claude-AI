using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using StudentManagementApp.Application.DTOs;
using StudentManagementApp.Application.Interfaces;
using StudentManagementApp.Domain.Entities;

namespace StudentManagementApp.Application.Services;

/// <summary>
/// Orchestrates validation, mapping, and repository calls for all student operations.
/// </summary>
public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<StudentDto> _validator;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IStudentRepository repository,
        IMapper mapper,
        IValidator<StudentDto> validator,
        ILogger<StudentService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public StudentDto Add(StudentDto studentDto)
    {
        _logger.LogInformation("Attempting to add student with email: {Email}", studentDto.Email);

        var validationResult = _validator.Validate(studentDto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for student {Email}: {Errors}", studentDto.Email, errors);
            throw new ValidationException(validationResult.Errors);
        }

        var normalizedEmail = studentDto.Email.Trim().ToLower();
        if (_repository.EmailExists(normalizedEmail))
        {
            _logger.LogWarning("Duplicate email detected: {Email}", normalizedEmail);
            throw new InvalidOperationException($"A student with email '{normalizedEmail}' already exists.");
        }

        var student = _mapper.Map<Student>(studentDto);
        student.Email = normalizedEmail;
        student.CreatedOn = DateTime.Now;
        student.UpdatedOn = DateTime.Now;

        _repository.Add(student);

        _logger.LogInformation("Student added successfully. ID: {Id}, Name: {Name}", student.Id, student.Name);
        return _mapper.Map<StudentDto>(student);
    }

    public IReadOnlyList<StudentDto> GetAll()
    {
        _logger.LogInformation("Retrieving all students.");
        var students = _repository.GetAll();
        _logger.LogInformation("Retrieved {Count} student(s).", students.Count);
        // Map to List<StudentDto>; List<T> satisfies IReadOnlyList<T>.
        return _mapper.Map<List<StudentDto>>(students);
    }

    public StudentDto? GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be a positive integer.", nameof(id));

        _logger.LogInformation("Retrieving student with ID: {Id}", id);
        var student = _repository.GetById(id);

        if (student is null)
        {
            _logger.LogWarning("Student with ID {Id} was not found.", id);
            return null;
        }

        return _mapper.Map<StudentDto>(student);
    }

    public void UpdateGrade(int id, double grade)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be a positive integer.", nameof(id));
        if (grade < 0 || grade > 100)
            throw new ArgumentException("Grade must be between 0 and 100.", nameof(grade));

        _logger.LogInformation("Updating grade for student ID: {Id} to {Grade}", id, grade);

        var student = _repository.GetById(id)
            ?? throw new KeyNotFoundException($"No student found with Id {id}.");

        student.Grade = grade;
        student.UpdatedOn = DateTime.Now;
        _repository.Update(student);

        _logger.LogInformation("Grade updated successfully for student ID: {Id}", id);
    }

    public void Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be a positive integer.", nameof(id));

        _logger.LogInformation("Deleting student with ID: {Id}", id);

        _ = _repository.GetById(id)
            ?? throw new KeyNotFoundException($"No student found with Id {id}.");

        _repository.Delete(id);
        _logger.LogInformation("Student with ID {Id} deleted successfully.", id);
    }
}
