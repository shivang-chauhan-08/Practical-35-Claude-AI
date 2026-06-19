using AutoMapper;
using StudentManagementApp.Application.DTOs;
using StudentManagementApp.Domain.Entities;

namespace StudentManagementApp.Application.Mappings;

/// <summary>
/// Bidirectional mapping between Student domain entity and StudentDto.
/// Id, CreatedOn, and UpdatedOn are excluded on DTO → Entity to prevent callers from forging audit data.
/// </summary>
public class StudentMappingProfile : Profile
{
    public StudentMappingProfile()
    {
        CreateMap<Student, StudentDto>();

        CreateMap<StudentDto, Student>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore());
    }
}
