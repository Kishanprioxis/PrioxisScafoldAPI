using Models.Models.RequestModel;
using Models.Models.ResponseModel;
using Models.Models.School;

namespace Service.Repository.Interface;

public interface IStudentRepository
{
    public List<StudentResponseModel> GetFilteredStudents(int page, int pageSize, string search);
    public bool DeleteStudent(string sid);
    public StudentResponseModel? GetStudentBySid(string sid);
    public bool CreateOrUpdateStudent(StudentRequestModel model, string? sid);
}