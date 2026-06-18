using StudentManagementApp.Models;
using StudentManagementApp.Services;

namespace StudentManagementApp.Test;

public class StudentServiceTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    private static StudentService CreateService() => new();

    private static Student MakeStudent(
        string name = "Alice Smith",
        string email = "alice@example.com",
        string date = "2023-09-01",
        double grade = 85.0)
        => new(name, email, DateTime.Parse(date), grade);

    // ── Add ────────────────────────────────────────────────────────────────────

    [Fact]
    public void Add_ValidStudent_AssignsIdAndStoresStudent()
    {
        // Arrange
        var service = CreateService();
        var student = MakeStudent();

        // Act
        service.Add(student);

        // Assert
        Assert.Equal(1, student.Id);
        Assert.Single(service.GetAll());
    }

    [Fact]
    public void Add_MultipleStudents_AssignsSequentialIds()
    {
        // Arrange
        var service = CreateService();
        var s1 = MakeStudent(name: "Alice Smith", email: "alice@example.com");
        var s2 = MakeStudent(name: "Bob Jones",  email: "bob@example.com");
        var s3 = MakeStudent(name: "Carol White", email: "carol@example.com");

        // Act
        service.Add(s1);
        service.Add(s2);
        service.Add(s3);

        // Assert
        Assert.Equal(1, s1.Id);
        Assert.Equal(2, s2.Id);
        Assert.Equal(3, s3.Id);
    }

    [Fact]
    public void Add_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        service.Add(MakeStudent(name: "Alice Smith", email: "alice@example.com"));
        var duplicate = MakeStudent(name: "Alicia Brown", email: "alice@example.com");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => service.Add(duplicate));
        Assert.Contains("alice@example.com", ex.Message);
    }

    [Fact]
    public void Add_NullStudent_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.Add(null!));
    }

    // ── GetAll ─────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_EmptyService_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.GetAll();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetAll_AfterAddingStudents_ReturnsAllStudents()
    {
        // Arrange
        var service = CreateService();
        service.Add(MakeStudent(name: "Alice Smith", email: "alice@example.com"));
        service.Add(MakeStudent(name: "Bob Jones",  email: "bob@example.com"));

        // Act
        var result = service.GetAll();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Email == "alice@example.com");
        Assert.Contains(result, s => s.Email == "bob@example.com");
    }

    // ── GetById ────────────────────────────────────────────────────────────────

    [Fact]
    public void GetById_ExistingId_ReturnsCorrectStudent()
    {
        // Arrange
        var service = CreateService();
        var student = MakeStudent();
        service.Add(student);

        // Act
        var result = service.GetById(student.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(student.Id, result.Id);
        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.GetById(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetById_InvalidId_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.GetById(0));
        Assert.Throws<ArgumentException>(() => service.GetById(-1));
    }

    // ── UpdateGrade ────────────────────────────────────────────────────────────

    [Fact]
    public void UpdateGrade_ValidGrade_UpdatesGradeSuccessfully()
    {
        // Arrange
        var service = CreateService();
        var student = MakeStudent(grade: 70.0);
        service.Add(student);

        // Act
        service.UpdateGrade(student.Id, 95.5);

        // Assert
        Assert.Equal(95.5, service.GetById(student.Id)!.Grade);
    }

    [Fact]
    public void UpdateGrade_BoundaryValues_AcceptsZeroAndHundred()
    {
        // Arrange
        var service = CreateService();
        var student = MakeStudent();
        service.Add(student);

        // Act & Assert — lower bound
        service.UpdateGrade(student.Id, 0);
        Assert.Equal(0, service.GetById(student.Id)!.Grade);

        // Act & Assert — upper bound
        service.UpdateGrade(student.Id, 100);
        Assert.Equal(100, service.GetById(student.Id)!.Grade);
    }

    [Fact]
    public void UpdateGrade_InvalidGrade_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateService();
        var student = MakeStudent();
        service.Add(student);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.UpdateGrade(student.Id, -1));
        Assert.Throws<ArgumentException>(() => service.UpdateGrade(student.Id, 101));
    }

    [Fact]
    public void UpdateGrade_NonExistingId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => service.UpdateGrade(999, 80));
    }

    // ── Delete ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ExistingStudent_RemovesFromList()
    {
        // Arrange
        var service = CreateService();
        var student = MakeStudent();
        service.Add(student);

        // Act
        service.Delete(student.Id);

        // Assert
        Assert.Empty(service.GetAll());
        Assert.Null(service.GetById(student.Id));
    }

    [Fact]
    public void Delete_OneOfManyStudents_RemovesOnlyTargetStudent()
    {
        // Arrange
        var service = CreateService();
        var s1 = MakeStudent(name: "Alice Smith", email: "alice@example.com");
        var s2 = MakeStudent(name: "Bob Jones",  email: "bob@example.com");
        service.Add(s1);
        service.Add(s2);

        // Act
        service.Delete(s1.Id);

        // Assert
        Assert.Single(service.GetAll());
        Assert.Null(service.GetById(s1.Id));
        Assert.NotNull(service.GetById(s2.Id));
    }

    [Fact]
    public void Delete_NonExistingId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => service.Delete(999));
    }

    // ── Model Validation ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("Alice123")]          // digits not allowed
    [InlineData("Alice@Smith")]       // special chars not allowed
    [InlineData("A")]                 // too short (< 2 chars)
    public void Student_InvalidName_ThrowsArgumentException(string invalidName)
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Student(invalidName, "alice@example.com", DateTime.Today, 80));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    public void Student_InvalidEmail_ThrowsArgumentException(string invalidEmail)
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Student("Alice Smith", invalidEmail, DateTime.Today, 80));
    }

    [Fact]
    public void Student_FutureEnrollmentDate_ThrowsArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Student("Alice Smith", "alice@example.com", DateTime.Today.AddDays(1), 80));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Student_InvalidGrade_ThrowsArgumentException(double invalidGrade)
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Student("Alice Smith", "alice@example.com", DateTime.Today, invalidGrade));
    }

    [Fact]
    public void Student_NameOver100Chars_ThrowsArgumentException()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Student(longName, "alice@example.com", DateTime.Today, 80));
    }

    [Fact]
    public void Student_NameWithTab_ThrowsArgumentException()
    {
        // Arrange — tab character should be rejected by the name regex
        Assert.Throws<ArgumentException>(() =>
            new Student("Alice\tSmith", "alice@example.com", DateTime.Today, 80));
    }

    // ── Immutability (post-add mutation cannot break invariants) ───────────────

    [Fact]
    public void Student_EmailIsImmutableAfterAdd_CannotBeChangedExternally()
    {
        // Arrange
        var service = CreateService();
        var s1 = MakeStudent(name: "Alice Smith", email: "alice@example.com");
        var s2 = MakeStudent(name: "Bob Jones",  email: "bob@example.com");
        service.Add(s1);
        service.Add(s2);

        // Act — Email setter is private; this must not compile if guard is in place.
        // Verify via reflection that Email cannot be set from outside (private setter).
        var prop = typeof(Student).GetProperty("Email");
        Assert.NotNull(prop);
        Assert.True(prop!.SetMethod?.IsPrivate ?? false,
            "Email setter must be private to prevent post-add uniqueness bypass.");
    }

    [Fact]
    public void Delete_WithIdZero_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert — must throw ArgumentException, not KeyNotFoundException
        Assert.Throws<ArgumentException>(() => service.Delete(0));
    }

    [Fact]
    public void UpdateGrade_WithNegativeId_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert — must throw ArgumentException, not KeyNotFoundException
        Assert.Throws<ArgumentException>(() => service.UpdateGrade(-1, 80));
    }
}
