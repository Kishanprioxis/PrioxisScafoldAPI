using AutoMapper;
using Models.Models.RequestModel;
using Models.Models.School;

namespace Models.Models.ResponseModel;

public class StudentResponseModel
{
    public string StudentSid { get; set; }
    public string Name { get; set; }
    
    public int? Age { get; set; }
    public string? Gender { get; set; }

    public string? Email { get; set; }
    public string? Course { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class StudentMapper : Profile
{
    public StudentMapper()
    {
        CreateMap<StudentRequestModel, Student>()
            .ForMember(dest => dest.StudentSid, opt => opt.Ignore()) 
            .ForMember(dest => dest.Status, opt => opt.Ignore())    

            
            .ForMember(dest => dest.StudentId, opt => opt.Ignore());

        // Entity to Response model
        CreateMap<Student, StudentResponseModel>();
    }
}