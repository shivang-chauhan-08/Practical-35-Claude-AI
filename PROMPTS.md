# PROMPTS USED IN CLAUDE CODE

## Prompt 1 - Create Project Structure

Create a Student Management Console Application using Clean Architecture.

Projects:

* StudentManagementApp.Domain
* StudentManagementApp.Application
* StudentManagementApp.Infrastructure
* StudentManagementApp
* StudentManagementApp.Test

Follow SOLID principles and proper project references.

---

## Prompt 2 - Create Domain Layer

Create BaseEntity and Student entity.

BaseEntity:

* Id
* CreatedOn
* UpdatedOn

Student:

* Name
* Email
* EnrollmentDate
* Grade

Student should inherit from BaseEntity.

---

## Prompt 3 - Create Repository

Create IStudentRepository and StudentRepository.

Requirements:

* Use Private List<Student> as storage(In memory Storage).
* CRUD operations.
* No database.

---

## Prompt 4 - Create Service Layer

Create StudentDto, IStudentService and StudentService.

Methods:

* Add
* GetAll
* GetById
* UpdateGrade
* Delete

Use repository pattern and ILogger.

---

## Prompt 5 - Add Fluent Validation

Create StudentValidator using FluentValidation.

Rules:

* Name required
* Valid email
* Grade between 0 and 100
* EnrollmentDate should not be future date

---

## Prompt 6 - Add AutoMapper

Create AutoMapper profile for Student and StudentDto mapping.

---

## Prompt 7 - Add Logging

Implement:

* Built-in ILogger
* Console logging
* Custom file logger

Store logs in:
Logs/application-log.txt

---

## Prompt 9 - Create Console UI

Create menu-driven Program.cs.

Options:

1. Add Student
2. View All Students
3. Find Student By Id
4. Update Grade
5. Delete Student
6. Exit

Add validation, exception handling and logging.

---

## Prompt 10 - Generate Unit Tests

Generate xUnit tests for StudentService.

Test cases:

* Add Student
* Get Student By Id
* Get All Students
* Update Grade
* Delete Student
* Validation Failure
* Student Not Found
* Repository Verification

Use Arrange Act Assert pattern and Moq where needed.

---

## Prompt 11 - Review Code

Review the complete solution.

Check:

* SOLID principles
* Naming conventions
* Validation
* Logging
* Exception handling
* Code quality

Suggest improvements if required.
