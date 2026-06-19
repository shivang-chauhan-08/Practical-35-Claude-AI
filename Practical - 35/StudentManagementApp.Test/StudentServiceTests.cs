using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using StudentManagementApp.Application.DTOs;
using StudentManagementApp.Application.Interfaces;
using StudentManagementApp.Application.Mappings;
using StudentManagementApp.Application.Services;
using StudentManagementApp.Application.Validators;
using StudentManagementApp.Domain.Entities;

namespace StudentManagementApp.Test;

public class StudentServiceTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<StudentMappingProfile>());
        return config.CreateMapper();
    }

    private static IValidator<StudentDto> CreateValidator() => new StudentValidator();

    private static ILogger<StudentService> CreateLogger() =>
        new Mock<ILogger<StudentService>>().Object;

    private static StudentDto MakeDto(
        string name  = "Alice Smith",
        string email = "alice@example.com",
        string date  = "2023-09-01",
        double grade = 85.0)
        => new()
        {
            Name           = name,
            Email          = email,
            EnrollmentDate = DateTime.Parse(date),
            Grade          = grade
        };

    /// <summary>
    /// Builds a service backed by a real repository (no mock) for integration-style tests.
    /// </summary>
    private static (StudentService service, Mock<IStudentRepository> repoMock) CreateWithMock()
    {
        var repoMock = new Mock<IStudentRepository>();
        var service  = new StudentService(repoMock.Object, CreateMapper(), CreateValidator(), CreateLogger());
        return (service, repoMock);
    }

    // ── Add — Success ──────────────────────────────────────────────────────────

    [Fact]
    public void Add_ValidStudent_CallsRepositoryAddAndReturnsDto()
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        repoMock.Setup(r => r.EmailExists(It.IsAny<string>(), It.IsAny<int?>())).Returns(false);
        repoMock.Setup(r => r.Add(It.IsAny<Student>())).Callback<Student>(s => s.Id = 1);

        // Act
        var result = service.Add(MakeDto());

        // Assert
        repoMock.Verify(r => r.Add(It.IsAny<Student>()), Times.Once);
        Assert.Equal(1, result.Id);
        Assert.Equal("alice@example.com", result.Email);
    }

    // ── Add — Duplicate Email ──────────────────────────────────────────────────

    [Fact]
    public void Add_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        repoMock.Setup(r => r.EmailExists("alice@example.com", null)).Returns(true);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => service.Add(MakeDto()));
        Assert.Contains("alice@example.com", ex.Message);
        repoMock.Verify(r => r.Add(It.IsAny<Student>()), Times.Never);
    }

    // ── Add — Validation Failure ───────────────────────────────────────────────

    [Fact]
    public void Add_NameTooShort_ThrowsValidationException()
    {
        // Arrange
        var (service, _) = CreateWithMock();

        // Act & Assert
        Assert.Throws<ValidationException>(() => service.Add(MakeDto(name: "Al")));
    }

    [Fact]
    public void Add_InvalidEmail_ThrowsValidationException()
    {
        // Arrange
        var (service, _) = CreateWithMock();

        // Act & Assert
        Assert.Throws<ValidationException>(() => service.Add(MakeDto(email: "not-an-email")));
    }

    [Fact]
    public void Add_FutureEnrollmentDate_ThrowsValidationException()
    {
        // Arrange
        var (service, _) = CreateWithMock();
        var dto = MakeDto();
        dto.EnrollmentDate = DateTime.Today.AddDays(1);

        // Act & Assert
        Assert.Throws<ValidationException>(() => service.Add(dto));
    }

    [Fact]
    public void Add_GradeOutOfRange_ThrowsValidationException()
    {
        // Arrange
        var (service, _) = CreateWithMock();

        // Act & Assert
        Assert.Throws<ValidationException>(() => service.Add(MakeDto(grade: 101)));
    }

    // ── GetAll ─────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_WhenStudentsExist_ReturnsAllMappedDtos()
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        var students = new List<Student>
        {
            new() { Id = 1, Name = "Alice Smith", Email = "alice@example.com", EnrollmentDate = DateTime.Today, Grade = 85 },
            new() { Id = 2, Name = "Bob Jones",   Email = "bob@example.com",   EnrollmentDate = DateTime.Today, Grade = 90 }
        };
        repoMock.Setup(r => r.GetAll()).Returns(students.AsReadOnly());

        // Act
        var result = service.GetAll();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Email == "alice@example.com");
        Assert.Contains(result, s => s.Email == "bob@example.com");
    }

    [Fact]
    public void GetAll_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        repoMock.Setup(r => r.GetAll()).Returns(new List<Student>().AsReadOnly());

        // Act & Assert
        Assert.Empty(service.GetAll());
    }

    // ── GetById ────────────────────────────────────────────────────────────────

    [Fact]
    public void GetById_ExistingId_ReturnsCorrectDto()
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        var student = new Student { Id = 5, Name = "Alice Smith", Email = "alice@example.com", EnrollmentDate = DateTime.Today, Grade = 85 };
        repoMock.Setup(r => r.GetById(5)).Returns(student);

        // Act
        var result = service.GetById(5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        repoMock.Setup(r => r.GetById(999)).Returns((Student?)null);

        // Act & Assert
        Assert.Null(service.GetById(999));
    }

    // ── UpdateGrade ────────────────────────────────────────────────────────────

    [Fact]
    public void UpdateGrade_ValidGrade_CallsRepositoryUpdateWithNewGrade()
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        var student = new Student { Id = 1, Name = "Alice Smith", Email = "alice@example.com", EnrollmentDate = DateTime.Today, Grade = 70 };
        repoMock.Setup(r => r.GetById(1)).Returns(student);

        // Act
        service.UpdateGrade(1, 95.5);

        // Assert
        repoMock.Verify(r => r.Update(It.Is<Student>(s => s.Grade == 95.5)), Times.Once);
    }

    [Fact]
    public void UpdateGrade_StudentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        repoMock.Setup(r => r.GetById(999)).Returns((Student?)null);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => service.UpdateGrade(999, 80));
    }

    // ── Delete ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ExistingStudent_CallsRepositoryDelete()
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        var student = new Student { Id = 3, Name = "Alice Smith", Email = "alice@example.com", EnrollmentDate = DateTime.Today, Grade = 85 };
        repoMock.Setup(r => r.GetById(3)).Returns(student);

        // Act
        service.Delete(3);

        // Assert
        repoMock.Verify(r => r.Delete(3), Times.Once);
    }

    [Fact]
    public void Delete_NonExistingStudent_ThrowsKeyNotFoundException()
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        repoMock.Setup(r => r.GetById(999)).Returns((Student?)null);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => service.Delete(999));
        repoMock.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    // ── Repository Interaction Verification ───────────────────────────────────

    [Fact]
    public void Add_ValidStudent_VerifiesEmailExistsCheckRunsBeforeRepositoryAdd()
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        var callOrder = new List<string>();
        repoMock.Setup(r => r.EmailExists(It.IsAny<string>(), It.IsAny<int?>()))
                .Callback(() => callOrder.Add("EmailExists"))
                .Returns(false);
        repoMock.Setup(r => r.Add(It.IsAny<Student>()))
                .Callback<Student>(s => { s.Id = 1; callOrder.Add("Add"); });

        // Act
        service.Add(MakeDto());

        // Assert — uniqueness guard runs before persistence
        Assert.Equal(new[] { "EmailExists", "Add" }, callOrder);
    }

    // ── GetById — Invalid Id ───────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99)]
    public void GetById_InvalidId_ThrowsArgumentException(int id)
    {
        // Arrange
        var (service, _) = CreateWithMock();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.GetById(id));
    }

    // ── UpdateGrade — Invalid Id ───────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateGrade_InvalidId_ThrowsArgumentException(int id)
    {
        // Arrange
        var (service, _) = CreateWithMock();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.UpdateGrade(id, 80));
    }

    // ── UpdateGrade — Grade Boundaries ────────────────────────────────────────

    [Theory]
    [InlineData(0.0)]
    [InlineData(100.0)]
    public void UpdateGrade_BoundaryGrades_Succeeds(double grade)
    {
        // Arrange
        var (service, repoMock) = CreateWithMock();
        var student = new Student { Id = 1, Name = "Alice Smith", Email = "alice@example.com", EnrollmentDate = DateTime.Today, Grade = 50 };
        repoMock.Setup(r => r.GetById(1)).Returns(student);

        // Act — should not throw
        service.UpdateGrade(1, grade);

        // Assert
        repoMock.Verify(r => r.Update(It.Is<Student>(s => s.Grade == grade)), Times.Once);
    }

    // ── UpdateGrade — Out-Of-Range Grade ──────────────────────────────────────

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    [InlineData(200)]
    public void UpdateGrade_GradeOutOfRange_ThrowsArgumentException(double grade)
    {
        // Arrange
        var (service, _) = CreateWithMock();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.UpdateGrade(1, grade));
    }

    // ── Delete — Invalid Id ────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Delete_InvalidId_ThrowsArgumentException(int id)
    {
        // Arrange
        var (service, _) = CreateWithMock();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.Delete(id));
    }
}

// ── StudentRepository Tests ────────────────────────────────────────────────────

public class StudentRepositoryTests
{
    private static Student MakeStudent(int id = 0, string name = "Alice Smith", string email = "alice@example.com", double grade = 85) =>
        new() { Id = id, Name = name, Email = email, EnrollmentDate = DateTime.Today, Grade = grade };

    // ── Add & GetAll ───────────────────────────────────────────────────────────

    [Fact]
    public void Add_SingleStudent_AssignsSequentialIdStartingAtOne()
    {
        // Arrange
        var repo = new StudentManagementApp.Infrastructure.Repositories.StudentRepository();

        // Act
        repo.Add(MakeStudent());

        // Assert
        Assert.Equal(1, repo.GetAll()[0].Id);
    }

    [Fact]
    public void Add_MultipleStudents_AssignsSequentialIds()
    {
        // Arrange
        var repo = new StudentManagementApp.Infrastructure.Repositories.StudentRepository();

        // Act
        repo.Add(MakeStudent());
        repo.Add(MakeStudent(email: "bob@example.com"));

        // Assert
        var ids = repo.GetAll().Select(s => s.Id).ToList();
        Assert.Equal(new[] { 1, 2 }, ids);
    }

    [Fact]
    public void GetAll_EmptyRepo_ReturnsEmptyList()
    {
        var repo = new StudentManagementApp.Infrastructure.Repositories.StudentRepository();
        Assert.Empty(repo.GetAll());
    }

    // ── GetById ────────────────────────────────────────────────────────────────

    [Fact]
    public void GetById_ExistingId_ReturnsCorrectStudent()
    {
        // Arrange
        var repo = new StudentManagementApp.Infrastructure.Repositories.StudentRepository();
        repo.Add(MakeStudent());

        // Act
        var result = repo.GetById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var repo = new StudentManagementApp.Infrastructure.Repositories.StudentRepository();
        Assert.Null(repo.GetById(999));
    }

    // ── Update ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ExistingStudent_ReplacesInList()
    {
        // Arrange
        var repo = new StudentManagementApp.Infrastructure.Repositories.StudentRepository();
        repo.Add(MakeStudent());
        var student = repo.GetById(1)!;
        student.Grade = 99;

        // Act
        repo.Update(student);

        // Assert
        Assert.Equal(99, repo.GetById(1)!.Grade);
    }

    // ── Delete ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ExistingStudent_RemovesFromList()
    {
        // Arrange
        var repo = new StudentManagementApp.Infrastructure.Repositories.StudentRepository();
        repo.Add(MakeStudent());

        // Act
        repo.Delete(1);

        // Assert
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void Delete_DoesNotReuseId_GapExpected()
    {
        // Arrange
        var repo = new StudentManagementApp.Infrastructure.Repositories.StudentRepository();
        repo.Add(MakeStudent());
        repo.Add(MakeStudent(email: "bob@example.com"));
        repo.Delete(1);

        // Act
        repo.Add(MakeStudent(email: "carol@example.com"));

        // Assert — next id is 3, not 1
        Assert.Equal(3, repo.GetAll().Last().Id);
    }

    // ── EmailExists ────────────────────────────────────────────────────────────

    [Fact]
    public void EmailExists_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        var repo = new StudentManagementApp.Infrastructure.Repositories.StudentRepository();
        repo.Add(MakeStudent());

        // Act & Assert
        Assert.True(repo.EmailExists("alice@example.com"));
    }

    [Fact]
    public void EmailExists_NonExistingEmail_ReturnsFalse()
    {
        var repo = new StudentManagementApp.Infrastructure.Repositories.StudentRepository();
        Assert.False(repo.EmailExists("nobody@example.com"));
    }

    [Fact]
    public void EmailExists_WithExcludeId_ReturnsFalseForOwner()
    {
        // Arrange
        var repo = new StudentManagementApp.Infrastructure.Repositories.StudentRepository();
        repo.Add(MakeStudent());

        // Act — excluding the owner's own id should report no duplicate
        Assert.False(repo.EmailExists("alice@example.com", excludeId: 1));
    }
}
