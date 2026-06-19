# Student Management App — Project Reference

## Project Overview

A .NET 10 console application for managing student records using in-memory storage (no database). Menu-driven UI with full CRUD operations. Refactored from a single-project structure to a clean 4-layer architecture (Domain → Application → Infrastructure → Presentation).

**Solution:** `Practical - 35.slnx`

---

## Build & Run

```bash
# Build entire solution
dotnet build "Practical - 35.slnx"

# Run the app
dotnet run --project StudentManagementApp/StudentManagementApp.csproj

# Run all tests
dotnet test StudentManagementApp.Test/StudentManagementApp.Test.csproj
```

Target framework: `net10.0`  
Nullable enabled, implicit usings enabled.

---

## Project Structure

```
Practical - 35/
├── .claude/
│   └── settings.json                          # Claude Code hook: logs every prompt to prompt_logs.txt
├── StudentManagementApp.Domain/               # Core domain — no dependencies
│   └── Entities/
│       ├── BaseEntity.cs                      # Id, CreatedOn, UpdatedOn
│       └── Student.cs                         # Name, Email, EnrollmentDate, Grade
├── StudentManagementApp.Application/          # Business logic — depends on Domain only
│   ├── DTOs/
│   │   └── StudentDto.cs                      # Data Transfer Object (crosses layer boundaries)
│   ├── Interfaces/
│   │   ├── IStudentService.cs                 # Service abstraction (Presentation depends on this)
│   │   ├── IStudentRepository.cs              # Repository abstraction (Application depends on this)
│   │   └── IPromptLogger.cs                   # Prompt logging abstraction
│   ├── Mappings/
│   │   └── StudentMappingProfile.cs           # AutoMapper: Student ↔ StudentDto
│   ├── Validators/
│   │   └── StudentValidator.cs                # FluentValidation rules for StudentDto
│   └── Services/
│       └── StudentService.cs                  # Orchestrates validation, mapping, repository calls
├── StudentManagementApp.Infrastructure/       # Implementations — depends on Domain + Application
│   ├── Repositories/
│   │   └── StudentRepository.cs               # In-memory List<Student> with auto-increment IDs
│   ├── Logging/
│   │   ├── FileLogger.cs                      # ILogger implementation writing to a .txt file
│   │   ├── FileLoggerProvider.cs              # ILoggerProvider factory for FileLogger
│   │   └── PromptLogger.cs                    # IPromptLogger implementation (appends to prompts.txt)
│   └── DependencyInjection/
│       └── InfrastructureServiceRegistration.cs  # AddInfrastructure() extension — wires all DI
├── StudentManagementApp/                      # Presentation (console) — depends on Application + Infrastructure
│   ├── Program.cs                             # DI setup, menu loop, input helpers
│   └── StudentManagementApp.csproj
├── StudentManagementApp.Test/                 # xUnit tests — 38 tests, all passing
│   └── StudentServiceTests.cs                 # StudentServiceTests + StudentRepositoryTests
└── CLAUDE.md                                  # This file
```

---

## Layer Dependency Rules

```
Presentation → Application ← Infrastructure
                    ↓
                  Domain
```

- **Domain** has zero dependencies.
- **Application** depends only on Domain. Defines interfaces; never references Infrastructure.
- **Infrastructure** implements Application interfaces. Depends on Domain + Application.
- **Presentation** wires everything via DI; depends on Application (interfaces) + Infrastructure (registration only).

---

## Domain Layer

### BaseEntity.cs — `StudentManagementApp.Domain.Entities`

| Property | Type | Notes |
|----------|------|-------|
| `Id` | `int` | Auto-assigned by repository |
| `CreatedOn` | `DateTime` | Set once at creation |
| `UpdatedOn` | `DateTime` | Refreshed on every repository write |

### Student.cs — `StudentManagementApp.Domain.Entities`

Inherits `BaseEntity`. Plain properties with no validation — validation lives in the Application layer via FluentValidation.

| Property | Type |
|----------|------|
| `Name` | `string` |
| `Email` | `string` |
| `EnrollmentDate` | `DateTime` |
| `Grade` | `double` |

---

## Application Layer

### StudentDto.cs

DTO used across layer boundaries. Never exposes the domain entity directly. Includes `CreatedOn` for display.

### IStudentService.cs

```csharp
StudentDto Add(StudentDto studentDto);
IReadOnlyList<StudentDto> GetAll();
StudentDto? GetById(int id);
void UpdateGrade(int id, double grade);
void Delete(int id);
```

### IStudentRepository.cs

```csharp
void Add(Student student);
IReadOnlyList<Student> GetAll();
Student? GetById(int id);
void Update(Student student);
void Delete(int id);
bool EmailExists(string email, int? excludeId = null);
```

### StudentValidator.cs (FluentValidation)

| Field | Rule |
|-------|------|
| Name | Required, 3–100 chars |
| Email | Required, valid email format |
| Grade | 0–100 inclusive |
| EnrollmentDate | Cannot be a future date |

### StudentMappingProfile.cs (AutoMapper)

- `Student → StudentDto` — all properties mapped automatically.
- `StudentDto → Student` — `Id`, `CreatedOn`, `UpdatedOn` ignored (prevents callers from forging audit data).

### StudentService.cs

Orchestrates validation → email uniqueness check → repository call. Logging via `ILogger<StudentService>`.

| Method | Guards | Exception |
|--------|--------|-----------|
| `Add` | FluentValidation + duplicate email | `ValidationException` / `InvalidOperationException` |
| `GetById` | `id ≤ 0` | `ArgumentException` |
| `UpdateGrade` | `id ≤ 0`, `grade` out of 0–100 | `ArgumentException`; `KeyNotFoundException` if not found |
| `Delete` | `id ≤ 0` | `ArgumentException`; `KeyNotFoundException` if not found |

---

## Infrastructure Layer

### StudentRepository.cs

In-memory `List<Student>`. Auto-increments `_nextId` starting at 1. IDs are never reused after deletion — gaps are expected. `EmailExists` supports an `excludeId` parameter for update scenarios.

### FileLogger / FileLoggerProvider

Custom `ILogger` / `ILoggerProvider` that appends structured log lines to `Logs/application-log.txt`.  
Format: `[yyyy-MM-dd HH:mm:ss] [LogLevel] Message`

### PromptLogger

Implements `IPromptLogger`. Appends every user-initiated action to `Logs/prompts.txt`.  
Format: `---\n[timestamp]\nprompt text`

Both loggers use `private static readonly Lock _fileLock` for thread safety.

### InfrastructureServiceRegistration.cs

`AddInfrastructure(services, logFilePath, promptLogFilePath)` extension method registers:
- `IStudentRepository` → `StudentRepository` (Singleton — in-memory list must survive across calls)
- `IStudentService` → `StudentService` (Scoped)
- AutoMapper with `StudentMappingProfile`
- FluentValidation `IValidator<StudentDto>` → `StudentValidator`
- `IPromptLogger` → `PromptLogger` (Singleton)
- Logging: Console + `FileLoggerProvider`

---

## Presentation Layer

### Program.cs

DI container built via `ServiceCollection` + `AddInfrastructure()`. `_service` typed as `IStudentService` (never the concrete class). `_promptLogger` logs every menu action.

**Menu options:**
1. Add Student
2. View All Students
3. Find Student By Id
4. Update Grade
5. Delete Student
6. Exit

**Input helper methods** (all loop until valid input):

| Method | Validates |
|--------|-----------|
| `ReadName` | Letters/spaces only, 3–100 chars |
| `ReadEmail` | Email regex format |
| `ReadGrade` | Double 0–100, `InvariantCulture` |
| `ReadDate` | `yyyy-MM-dd`, not future, `InvariantCulture` |
| `ReadInt` | Positive integer |

**EOF handling:** All helpers call `ExitOnEof()` when `Console.ReadLine()` returns `null`.

---

## Tests — `StudentManagementApp.Test`

**38 tests, all passing.** Framework: xUnit 2.9.3 + Moq 4.20, pattern: Arrange / Act / Assert.

| Class | Count | Covers |
|-------|-------|--------|
| `StudentServiceTests` | 27 | Service-level tests using mocked `IStudentRepository` |
| `StudentRepositoryTests` | 11 | Repository-level integration tests against real in-memory store |

### StudentServiceTests groups

| Group | Tests |
|-------|-------|
| Add — success | Valid student calls repo and returns DTO |
| Add — validation | Short name, invalid email, future date, grade > 100 |
| Add — duplicate email | Throws `InvalidOperationException`; `Add` never called |
| Add — call order | `EmailExists` runs before `Add` |
| GetAll | Empty list, returns all mapped DTOs |
| GetById | Found, not found, invalid id (`ArgumentException`) |
| UpdateGrade | Success, boundary grades (0/100), invalid grade, missing id, invalid id |
| Delete | Success, missing id, invalid id |

### StudentRepositoryTests groups

| Group | Tests |
|-------|-------|
| Add & GetAll | Sequential IDs from 1, multiple students, empty list |
| GetById | Found, not found |
| Update | Replaces in list |
| Delete | Removes from list, IDs not reused after delete |
| EmailExists | Existing, non-existing, excludeId for owner |

---

## NuGet Packages

| Package | Version | Used in |
|---------|---------|---------|
| AutoMapper | 12.0.1 | Application, Infrastructure, Test |
| AutoMapper.Extensions.Microsoft.DependencyInjection | 12.0.1 | Infrastructure |
| FluentValidation | 11.10.0 | Application, Infrastructure, Presentation, Test |
| FluentValidation.DependencyInjectionExtensions | 11.10.0 | Infrastructure |
| Microsoft.Extensions.DependencyInjection | 9.0.6 | Infrastructure, Presentation |
| Microsoft.Extensions.Logging | 9.0.6 | Infrastructure |
| Microsoft.Extensions.Logging.Abstractions | 9.0.6 | Application, Test |
| Microsoft.Extensions.Logging.Console | 9.0.6 | Infrastructure |
| xUnit | 2.9.3 | Test |
| Moq | 4.20.72 | Test |

> **Note:** AutoMapper 12.0.1 has a known vulnerability (GHSA-rvv3-g6hj-g44x) flagged as NU1903. No v13 of `AutoMapper.Extensions.Microsoft.DependencyInjection` exists yet. The vulnerability is in mapping profiles using `UseDestinationValue` — not exercised here. Safe to leave as-is for a learning project.

---

## Design Decisions

- **4-layer architecture** — Domain / Application / Infrastructure / Presentation. Each layer depends only inward; Presentation never references concrete infrastructure types.
- **DTOs cross layer boundaries** — `Program.cs` and tests work with `StudentDto`, never with `Student` directly. This decouples the UI from the domain model.
- **AutoMapper** — bidirectional mapping in `StudentMappingProfile`. `Id`, `CreatedOn`, `UpdatedOn` are ignored on `StudentDto → Student` to prevent callers from forging audit data.
- **FluentValidation** — rules live in `StudentValidator`, executed by `StudentService` before any repository call. Keeps domain entity clean (no validation attributes).
- **Repository pattern** — `IStudentRepository` defined in Application; `StudentRepository` lives in Infrastructure. Swapping to EF Core or JSON requires only a new Infrastructure implementation.
- **Singleton repository** — `StudentRepository` registered as Singleton because the in-memory list must survive across scoped service resolutions.
- **Thread-safe loggers** — both `FileLogger` and `PromptLogger` use `private static readonly Lock _fileLock` so concurrent log writes don't interleave.
- **`IReadOnlyList<T>` from GetAll** — prevents callers from mutating the internal collection.
- **`CultureInfo.InvariantCulture`** — all numeric and date parsing uses `InvariantCulture` to avoid locale-dependent failures.
- **`ExitOnEof()`** — when `Console.ReadLine()` returns `null` (piped stdin), helpers call `Environment.Exit(0)` instead of looping forever.

---

## Claude Code Hook

Configured in `.claude/settings.json`.

**Event:** `UserPromptSubmit`  
**Effect:** Every prompt submitted in this Claude Code session is appended to `prompt_logs.txt`.

---

## Extending This Project

### Swap in-memory store for JSON file persistence
- Add `IStudentRepository` is already defined — implement `JsonStudentRepository` using `System.Text.Json`
- Register it in `InfrastructureServiceRegistration` in place of `StudentRepository`
- `Program.cs` and `StudentService` stay unchanged

### Add EF Core persistence
- Implement `EfStudentRepository : IStudentRepository`
- Add a `DbContext` in Infrastructure
- `Program.cs` stays unchanged

### Add sorting to `GetAll()`
```csharp
// In IStudentService / StudentService
IReadOnlyList<StudentDto> GetAll(string? sortBy = null);
```
