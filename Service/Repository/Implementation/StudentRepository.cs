using AutoMapper;
using Common;
using Models.Models.RequestModel;
using Models.Models.ResponseModel;
using Models.Models.School;
using Serilog;
using Service.Repository.Interface;

namespace Service.Repository.Implementation;

public class StudentRepository : IStudentRepository
{
    public SchoolDbContext Context;
    private readonly IMapper _mapper;
    public StudentRepository(SchoolDbContext context,IMapper mapper)
    {
        Context = context;
        _mapper = mapper;
    }   
    public List<StudentResponseModel> GetFilteredStudents(int page, int pageSize, string search)
    {
        Log.Information("In StudentRepository.GetFilteredStudents");
    
        try
        {
            var query = Context.Students
                .Where(s => s.Status == (int)StatusEnum.Active && s.Course != null);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Name.Contains(search));
            }

            if (page > 0 && pageSize > 0)
            {
                query = query.Skip((page - 1) * pageSize).Take(pageSize);
                Log.Information($"For Page Number {page}");
            }

            List<StudentResponseModel> response = _mapper.Map<List<StudentResponseModel>>(query);
            return response;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred in GetFilteredStudents: {Message}", ex.Message);
            throw; // Re-throw the exception to be handled by the controller
        }
    }
    
    public bool DeleteStudent(string sid)
    {
        Log.Information("In StudentRepository.DeleteStudent for SID: {Sid}", sid);

        try
        {
            var student = Context.Students.FirstOrDefault(s => s.StudentSid == sid && s.Status == (int)StatusEnum.Active);

            if (student == null)
            {
                Log.Warning("No active student found with SID: {Sid}", sid);
                return false;
            }

            student.Status = (int)StatusEnum.Deleted; // assuming 3 means deleted
            return Context.SaveChanges() > 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while deleting student with SID: {Sid}. Error: {Message}", sid, ex.Message);
            throw;
        }
    }
    
    public StudentResponseModel? GetStudentBySid(string sid)
    {
        Log.Information("In StudentRepository.GetStudentBySid Method for SID: {Sid}", sid);

        try
        {
            var student = Context.Students
                .Where(s => s.Status == (int)StatusEnum.Active)
                .FirstOrDefault(s => s.StudentSid == sid);

            if (student == null)
            {
                Log.Warning("No active student found with SID: {Sid}", sid);
                return null;
            }

            StudentResponseModel response = _mapper.Map<StudentResponseModel>(student);
            return response;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in GetStudentBySid for SID: {Sid}. Error: {Message}", sid, ex.Message);
            throw;
        }
    }



    // public bool CreateOrUpdateStudent(StudentRequestModel model, string? sid)
    // {
    //     if (sid == null)
    //     {
    //        
    //         
    //         var student = new Student
    //         {
    //             Name = model.Name,
    //             Age = model.Age,
    //             StudentSid = "SID" + Guid.NewGuid().ToString(),
    //             Status = 1,
    //             Email = model.Email,
    //             Course = model.Course,
    //             Gender = model.Gender
    //         };
    //
    //         Context.Students.Add(student);
    //         return Context.SaveChanges() > 0;
    //     }
    //     else
    //     {
    //         // Update existing student
    //         var existingStudent = Context.Students.FirstOrDefault(s => s.StudentSid == sid);
    //         if (existingStudent == null)
    //             return false;
    //
    //         existingStudent.Name = model.Name;
    //         existingStudent.Age = model.Age;
    //         existingStudent.Email = model.Email;
    //         existingStudent.Course = model.Course;
    //         existingStudent.Gender = model.Gender;
    //
    //         return Context.SaveChanges() > 0;
    //     }
    // }
    public bool CreateOrUpdateStudent(StudentRequestModel model, string? sid)
    {
        Log.Information("In StudentRepository.CreateOrUpdateStudent Method");

        try
        {
            if (sid == null)
            {
                Log.Information("New Student is created");
                var student = _mapper.Map<Student>(model);
                student.StudentSid = "SID" + Guid.NewGuid().ToString();
                student.Status = (int)StatusEnum.Active;
                student.CreatedAt = DateTime.Now;
                student.ModifiedAt = DateTime.Now;
                Context.Students.Add(student);
            }
            else
            {
                var existingStudent = Context.Students.FirstOrDefault(s => s.StudentSid == sid && s.Status == (int)StatusEnum.Active);
                Log.Information($"Student is updated with StudentSID {sid}");
                if (existingStudent == null)
                {
                    Log.Warning("No active student found with SID: {Sid}", sid);
                    return false;
                }
                existingStudent.ModifiedAt = DateTime.Now;
                _mapper.Map(model, existingStudent); // Maps updated fields
            }

            return Context.SaveChanges() > 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in CreateOrUpdateStudent: {Message}", ex.Message);
            throw;
        }
    }


    
}