# Student Management App — Project Reference

## Project Overview

A .NET 10 console application for managing student records using in-memory storage (no database). Menu-driven UI with full CRUD operations and field-level validation.

**Solution:** `Practical - 35.slnx`
**Main project:** `StudentManagementApp/`
**Test project:** `StudentManagementApp.Test/`

---

## Build & Run

```bash
# Build
dotnet build StudentManagementApp/StudentManagementApp.csproj

# Run
dotnet run --project StudentManagementApp/StudentManagementApp.csproj

# Test
dotnet test StudentManagementApp.Test/StudentManagementApp.Test.csproj
```

Target framework: `net10.0`  
Nullable enabled, implicit usings enabled.

---

## Project Structure

```
Practical - 35/
├── .claude/
│   └── settings.json                  # Claude Code hook: logs every prompt to prompt_logs.txt
├── StudentManagementApp/
│   ├── Models/
│   │   └── Student.cs                 # Domain model with property-level validation
│   ├── Services/
│   │   ├── IStudentService.cs         # Abstraction — Program depends on this, not the concrete class
│   │   └── StudentService.cs          # CRUD implementation, in-memory List<Student>
│   ├── Program.cs                     # Menu-driven console UI, input helpers
│   └── StudentManagementApp.csproj
├── StudentManagementApp.Test/
│   ├── StudentServiceTests.cs         # 30 xUnit tests (Fact + Theory, AAA pattern)
│   └── StudentManagementApp.Test.csproj
├── prompt_logs.txt                    # Auto-created; every Claude prompt logged here with timestamp
└── CLAUDE.md                          # This file
```

---

## Architecture

### Student.cs — `StudentManagementApp.Models`

Domain model. All validation lives in property setters so it is enforced regardless of caller. Properties are immutable after construction except `Grade` (updated via service) and `Id` (assigned by service).

| Property | Type | Setter | Validation |
|----------|------|--------|------------|
| `Id` | `int` | `internal set` | Assigned by `StudentService`; never user-supplied |
| `Name` | `string` | `private set` | Letters and spaces only (`^[a-zA-Z ]+$`), 2–100 chars |
| `Email` | `string` | `private set` | Regex-validated, stored lowercase |
| `EnrollmentDate` | `DateTime` | `private set` | Cannot be a future date; stored as `.Date` |
| `Grade` | `double` | `internal set` | 0–100 inclusive; only `StudentService` updates it |

**Why private setters matter:** `Name`, `Email`, and `EnrollmentDate` are set only in the constructor and must never change after a student is added. Making their setters `private` prevents any external code from mutating a stored student and silently breaking service-level invariants (e.g., email uniqueness). Only `Grade` is updateable post-construction, and only through the service.

**Email regex:** `^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$`  
**Name regex:** `^[a-zA-Z ]+$` — literal space only; `\s` is intentionally avoided because it matches tabs and newlines.

**Constructor:** `Student(string name, string email, DateTime enrollmentDate, double grade)`  
`Id` is NOT a constructor parameter.

**`ToString()`** output format:
```
[ID: 1] John Doe | john@example.com | Enrolled: 2024-09-01 | Grade: 88.5
```

---

### IStudentService.cs — `StudentManagementApp.Services`

Interface that `Program.cs` depends on. Allows substituting implementations (e.g., JSON-backed, EF Core) without modifying `Program.cs`.

```csharp
public interface IStudentService
{
    void Add(Student student);
    IReadOnlyList<Student> GetAll();
    Student? GetById(int id);
    void UpdateGrade(int id, double grade);
    void Delete(int id);
}
```

---

### StudentService.cs — `StudentManagementApp.Services`

Implements `IStudentService`. Holds `private readonly List<Student> _students` as in-memory storage. Uses `private int _nextId = 1` to auto-increment IDs.

| Method | Behaviour |
|--------|-----------|
| `Add(Student)` | Validates non-null + unique email, assigns `Id`, appends to list |
| `GetAll()` | Returns `IReadOnlyList<Student>` (read-only collection view) |
| `GetById(int id)` | Throws `ArgumentException` for `id ≤ 0`; returns `Student?` (null if not found) |
| `UpdateGrade(int id, double grade)` | Guards `id ≤ 0` directly; throws `KeyNotFoundException` if not found |
| `Delete(int id)` | Guards `id ≤ 0` directly; throws `KeyNotFoundException` if not found |

**Exception contract:**
- `id ≤ 0` → `ArgumentException` (thrown by the method itself, not delegated to `GetById`)
- Valid id not found → `KeyNotFoundException`
- Duplicate email on Add → `InvalidOperationException`

`Delete` and `UpdateGrade` each validate `id` directly before calling `GetById`. This ensures the exception type is consistent regardless of which internal path executes.

**ID rules:** Auto-generated, sequential, positive. IDs are NOT re-assigned after a delete — gaps are expected.

---

### Program.cs — `StudentManagementApp`

`_service` is typed as `IStudentService` (not the concrete class). Single `Main()` loop. All exceptions are caught and printed; the loop continues.

**Menu options:**
1. Add Student
2. View All Students
3. Find Student By Id
4. Update Grade
5. Delete Student
6. Exit

**Input helper methods** (all loop until valid input is given):

| Method | Validates |
|--------|-----------|
| `ReadName(prompt)` | Letters/spaces only, 2–100 chars |
| `ReadEmail(prompt)` | Regex email format |
| `ReadGrade(prompt)` | Double 0–100, `CultureInfo.InvariantCulture` |
| `ReadDate(prompt)` | `yyyy-MM-dd` format via `CultureInfo.InvariantCulture`, not future |
| `ReadInt(prompt)` | Positive integer |

**EOF handling:** All helpers call `ExitOnEof()` when `Console.ReadLine()` returns `null` (piped stdin, closed stream). This calls `Environment.Exit(0)` with a message instead of spinning in an infinite loop.

**Culture:** All numeric and date parsing uses `CultureInfo.InvariantCulture` explicitly to avoid locale-dependent failures (e.g., comma vs. dot decimal separators on European locales).

---

## Validation Rules

| Field | Rule | Error message |
|-------|------|---------------|
| Name | Letters and single spaces only (`^[a-zA-Z ]+$`) | "Name must contain letters only (no digits or special characters)." |
| Name | 2–100 characters | "Name must be between 2 and 100 characters." |
| Email | Must match email regex | "Email format is invalid (e.g. user@example.com)." |
| Email | Must be unique per student | "A student with email '...' already exists." |
| EnrollmentDate | Cannot be future | "Enrollment date cannot be a future date." |
| EnrollmentDate | Format `yyyy-MM-dd` | "Invalid format. Use yyyy-MM-dd (e.g., 2024-09-01)." |
| Grade | 0–100 inclusive | "Grade must be between 0 and 100." |

---

## Tests — `StudentManagementApp.Test`

**30 tests, all passing.** Framework: xUnit 2.9.3, pattern: Arrange / Act / Assert.

| Group | Count | Covers |
|-------|-------|--------|
| Add | 4 | Success, sequential IDs, duplicate email, null |
| GetAll | 2 | Empty list, returns all |
| GetById | 3 | Found, not found, invalid id |
| UpdateGrade | 4 | Success, boundaries (0/100), invalid grade, missing id |
| Delete | 3 | Removes student, removes only target, missing id |
| Model validation | 9 | Invalid name, email, future date, grade (Theory + InlineData) |
| Post-review additions | 5 | Tab in name, 101-char name, `Delete(0)`, `UpdateGrade(-1)`, Email immutability |

---

## Claude Code Hook

Configured in `.claude/settings.json`.

**Event:** `UserPromptSubmit`  
**Effect:** Every prompt submitted in this Claude Code session is appended to `prompt_logs.txt`.

**Log format:**
```
[2026-06-18 15:32:42] your prompt text here
```

Runs asynchronously (`"async": true`) — never blocks the session. To disable: open `/hooks` in Claude Code and toggle off.

---

## Design Decisions

- **No database** — `List<Student>` in-memory; data resets on each run. Intentional for this exercise.
- **`IStudentService` interface** — `Program._service` is typed as `IStudentService`, not `StudentService`. Enables substitution (JSON, EF Core) and mocking in tests without modifying `Program.cs`.
- **Private setters on immutable fields** — `Name`, `Email`, `EnrollmentDate` are `private set`. This prevents any external code from mutating a stored student and silently breaking the email-uniqueness invariant that `Add()` enforces. Only `Grade` is `internal set` (updateable by the service) and `Id` is `internal set` (assigned by the service).
- **Id guards in `Delete` / `UpdateGrade`** — Both methods validate `id ≤ 0` directly before calling `GetById`. This ensures callers always get `ArgumentException` for invalid IDs and `KeyNotFoundException` for valid-but-missing IDs — the exception type is predictable without reading internal code.
- **`CultureInfo.InvariantCulture`** — All `double.TryParse` and `DateTime.TryParseExact` calls specify `InvariantCulture`. Without this, the app silently fails on non-English locales (comma decimal separator, non-Gregorian calendar systems).
- **`ExitOnEof()`** — When `Console.ReadLine()` returns `null` (piped stdin), all helpers call `Environment.Exit(0)` rather than looping forever. `null` is a sentinel for "stream closed", not bad user input.
- **Literal space in name regex** — `^[a-zA-Z ]+$` uses a literal space, not `\s`. In .NET, `\s` matches tab, newline, carriage return, and other control characters. A tab-containing name would pass `\s`-based validation silently.
- **`IReadOnlyList<Student>` from `GetAll()`** — Prevents callers from adding/removing items from the collection. Combined with private setters, stored student objects are fully protected from external mutation.
- **No partial updates** — Only grade can be updated post-creation. To change name/email, delete and re-add.

---

## Extending This Project

### Add persistence (JSON file)
- Add `IStudentRepository` interface
- Implement `JsonStudentRepository` using `System.Text.Json`
- Inject into `StudentService` via constructor
- `Program.cs` stays unchanged — it only knows `IStudentService`

### Add sorting/filtering to `GetAll()`
```csharp
public IReadOnlyList<Student> GetAll(string? sortBy = null) =>
    sortBy switch
    {
        "name"  => _students.OrderBy(s => s.Name).ToList().AsReadOnly(),
        "grade" => _students.OrderByDescending(s => s.Grade).ToList().AsReadOnly(),
        _       => _students.AsReadOnly()
    };
```

### Add `UpdateEmail` to the service
If email updates are ever needed, they must go through the service (not via `student.Email =`) so the uniqueness check runs:
```csharp
public void UpdateEmail(int id, string newEmail)
{
    if (_students.Any(s => s.Id != id && s.Email == newEmail.Trim().ToLower()))
        throw new InvalidOperationException($"Email '{newEmail}' is already in use.");
    var student = GetById(id) ?? throw new KeyNotFoundException(...);
    // Email setter is private — use reflection or expose an internal method
}
```
