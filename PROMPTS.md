# PROMPTS USED IN CLAUDE CODE

## Prompt 1 - Generate Application

Create a complete C# .NET Console Application named Student Management System.

Requirements:

Student class:
- Id (int)
- Name (string)
- Email (string)
- EnrollmentDate (DateTime)
- Grade (double)

StudentService class:
- Add(Student student)
- GetAll()
- GetById(int id)
- UpdateGrade(int id, double grade)
- Delete(int id)

Storage:
- Use private List<Student> as in-memory storage.
- No database.

Console UI:
- Menu-driven Program.cs
- Options:
  1. Add Student
  2. View All Students
  3. Find Student By Id
  4. Update Grade
  5. Delete Student
  6. Exit

Requirements:
- Follow clean coding practices.
- Separate files for Student and StudentService.
- Include input validation.
- Use proper namespaces.

---

## Prompt 2 - Generate Tests

Generate xUnit tests for StudentService.

Requirements:
- Use xUnit framework.
- Follow Arrange Act Assert pattern.
- Create at least 5 test cases.

Tests should verify:
1. Add student successfully.
2. Get student by id.
3. Get all students.
4. Update grade successfully.
5. Delete student successfully.

---

## Prompt 3 - Code Review

Review the generated Student Management System project.

Check:
- Logic errors
- Null handling
- Input validation issues
- Code quality improvements
- Naming conventions
- SOLID principles

List any improvements and provide corrected code where necessary.
