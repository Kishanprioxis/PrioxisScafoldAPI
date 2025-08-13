using Models.Models.CommonModel;
using Models.Models.RequestModel;
using Models.Models.ResponseModel;
using Models.Models.School;

namespace Service.Repository.Interface;

public interface IStudentRepository
{
    // public List<StudentResponseModel> GetFilteredStudents(int page, int pageSize, string search);
    // public bool DeleteStudent(string sid);
    // public StudentResponseModel? GetStudentBySid(string sid);
    // public bool CreateOrUpdateStudent(StudentRequestModel model, string? sid);
    
    Task<List<StudentResponseModel>> List(Dictionary<string, object> parameters);
    Task<StudentResponseModel> CreateAsync(StudentRequestModel student);
    Task<StudentResponseModel> GetStudentBySid(string studentSid);
    Task<bool> UpdateAsync(string studentSid, StudentRequestModel student);
    Task<bool> DeleteAsync(string studentSid);
}