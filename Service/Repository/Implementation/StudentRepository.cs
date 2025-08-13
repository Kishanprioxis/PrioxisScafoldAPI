using AutoMapper;
using Common;
using Microsoft.EntityFrameworkCore;
using Models.Models.CommonModel;
using Models.Models.RequestModel;
using Models.Models.ResponseModel;
using Models.Models.School;
using Models.Models.SpDbContext;
using Newtonsoft.Json;
using Serilog;
using Service.Repository.Interface;
using Service.RepositoryFactory;
using Service.UnitOfWork;
using UserDB = Models.Models.School.Student;

namespace Service.Repository.Implementation;

public class StudentRepository : IStudentRepository
{
    // public SchoolDbContext Context;
    // private readonly IMapper _mapper;
    // public StudentRepository(SchoolDbContext context,IMapper mapper)
    // {
    //     Context = context;
    //     _mapper = mapper;
    // } 
    private readonly SchoolDbContext _context;
    private readonly SchoolManagementSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public StudentRepository(SchoolDbContext context, SchoolManagementSpContext spContext, IUnitOfWork unitOfWork,IMapper mapper)
    {
        _context = context;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<List<StudentResponseModel>> List(Dictionary<string, object> parameters)
    {
        try
        {
            var xmlPara = CommonHelper.DictionaryToXml(parameters, "Search");
            string sqlQuery = "sp_DynamicGetAllStudent {0}";
            object[] param = { xmlPara };
            var res = await _spContext.ExecutreStoreProcedureResultList(sqlQuery, param);
            List<StudentResponseModel> list =
                JsonConvert.DeserializeObject<List<StudentResponseModel>>(res.Result?.ToString() ?? "[]") ?? [];
            if (list != null)
            {
                return list;
            }
            return new List<StudentResponseModel>();   
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in List: {ex.Message}");
            throw new HttpStatusCodeException(500,ex.Message); 
        }
    }

    public async Task<StudentResponseModel> CreateAsync(StudentRequestModel student)
    {
        try
        {
            // var newStudent = new Student
            // {
            //     Name = student.Name,
            //     Gender = student.Gender,
            //     Age = student.Age,
            //     Email = student.Email,
            //     Status = (int)StatusEnum.Active,
            //     CreatedAt = DateTime.UtcNow,
            //     ModifiedAt = DateTime.UtcNow,
            //     Course = student.Course
            // };
            Student s = _mapper.Map<Student>(student);
            s.StudentSid = "SID" + Guid.NewGuid().ToString();
            s.Status = (int)StatusEnum.Active;
            await _unitOfWork.GetRepository<Student>().InsertAsync(s);
            await _unitOfWork.CommitAsync();
            StudentResponseModel res = _mapper.Map<StudentResponseModel>(s);
            return res;
        }
        catch (Exception ex)
        {
            if (_unitOfWork.dbContextTransaction != null)
            {
                await _unitOfWork.dbContextTransaction.RollbackAsync();
            }
            Log.Error(ex, "Error while creating student");
            throw new HttpStatusCodeException(500, "Error while creating student");
        }
    }

    public async Task<StudentResponseModel?> GetStudentBySid(string studentSid)
    {
        try
        {
            string query = "sp_getBySid {0}";
            object[] param = { studentSid };
            var student = await _spContext.ExecuteStoreProcedure(query,param);
            StudentResponseModel res = Newtonsoft.Json.JsonConvert.DeserializeObject<StudentResponseModel>(student?.ToString() ?? "{}");
    
            if (res == null)
            {
                return null; 
            }
            // var resUser = _mapper.Map<UserDB, StudentResponseModel>(student);
            return res;
            // return new StudentResponseModel
            // {
            //     StudentSid = user.StudentSid,
            //     Name = user.Name,
            //     Gender = user.Gender,
            //     Age = user.Age,
            //     Email = user.Email,
            //     Course = user.Course,
            //     CreatedAt = user.CreatedAt,
            //     ModifiedAt = user.ModifiedAt
            // };
        }
        catch (Exception ex)
        {
           
            Console.WriteLine($"Error in GetStudentBySid: {ex.Message}");
            throw; 
        }
    }

    public async Task<bool> UpdateAsync(string studentSid,StudentRequestModel student)
    {
        try
        {
            var user = await _unitOfWork.GetRepository<UserDB>()
                .SingleOrDefaultAsync(x => x.StudentSid == studentSid);
            user.Name = student.Name;
            user.Gender = student.Gender;
            user.Age = student.Age;
            user.Email = student.Email;
            user.Course = student.Course;
            user.ModifiedAt = DateTime.UtcNow;
            _unitOfWork.GetRepository<UserDB>().Update(user);
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.dbContextTransaction.RollbackAsync();
            Console.WriteLine($"Error in Update: {ex.Message}");
            throw; 
        }
    }

    public async Task<bool> DeleteAsync(string studentSid)
    {
        try
        {
            var user  = await _unitOfWork.GetRepository<UserDB>().SingleOrDefaultAsync(x => x.StudentSid == studentSid);
            user.Status = (int)StatusEnum.Deleted;
            _unitOfWork.GetRepository<UserDB>().Update(user);
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            //_unitOfWork.dbContextTransaction.RollbackAsync();
            Console.WriteLine($"Error in Delete: {ex.Message}");
            throw;
        }
    }
    // public List<StudentResponseModel> GetFilteredStudents(int page, int pageSize, string search)
    // {
    //     Log.Information("In StudentRepository.GetFilteredStudents");
    //
    //     try
    //     {
    //         var query = Context.Students
    //             .Where(s => s.Status == (int)StatusEnum.Active && s.Course != null);
    //
    //         if (!string.IsNullOrEmpty(search))
    //         {
    //             query = query.Where(s => s.Name.Contains(search));
    //         }
    //
    //         if (page > 0 && pageSize > 0)
    //         {
    //             query = query.Skip((page - 1) * pageSize).Take(pageSize);
    //             Log.Information($"For Page Number {page}");
    //         }
    //
    //         List<StudentResponseModel> response = _mapper.Map<List<StudentResponseModel>>(query);
    //         return response;
    //     }
    //     catch (Exception ex)
    //     {
    //         Log.Error(ex, "Error occurred in GetFilteredStudents: {Message}", ex.Message);
    //         throw; // Re-throw the exception to be handled by the controller
    //     }
    // }
    //
    // public bool DeleteStudent(string sid)
    // {
    //     Log.Information("In StudentRepository.DeleteStudent for SID: {Sid}", sid);
    //
    //     try
    //     {
    //         var student = Context.Students.FirstOrDefault(s => s.StudentSid == sid && s.Status == (int)StatusEnum.Active);
    //
    //         if (student == null)
    //         {
    //             Log.Warning("No active student found with SID: {Sid}", sid);
    //             return false;
    //         }
    //
    //         student.Status = (int)StatusEnum.Deleted; // assuming 3 means deleted
    //         return Context.SaveChanges() > 0;
    //     }
    //     catch (Exception ex)
    //     {
    //         Log.Error(ex, "Error while deleting student with SID: {Sid}. Error: {Message}", sid, ex.Message);
    //         throw;
    //     }
    // }
    
    // public StudentResponseModel? GetStudentBySid(string sid)
    // {
    //     Log.Information("In StudentRepository.GetStudentBySid Method for SID: {Sid}", sid);
    //
    //     try
    //     {
    //         var student = Context.Students
    //             .Where(s => s.Status == (int)StatusEnum.Active)
    //             .FirstOrDefault(s => s.StudentSid == sid);
    //
    //         if (student == null)
    //         {
    //             Log.Warning("No active student found with SID: {Sid}", sid);
    //             return null;
    //         }
    //
    //         StudentResponseModel response = _mapper.Map<StudentResponseModel>(student);
    //         return response;
    //     }
    //     catch (Exception ex)
    //     {
    //         Log.Error(ex, "Error in GetStudentBySid for SID: {Sid}. Error: {Message}", sid, ex.Message);
    //         throw;
    //     }
    // }



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
    // public bool CreateOrUpdateStudent(StudentRequestModel model, string? sid)
    // {
    //     Log.Information("In StudentRepository.CreateOrUpdateStudent Method");
    //
    //     try
    //     {
    //         if (sid == null)
    //         {
    //             Log.Information("New Student is created");
    //             var student = _mapper.Map<Student>(model);
    //             student.StudentSid = "SID" + Guid.NewGuid().ToString();
    //             student.Status = (int)StatusEnum.Active;
    //             student.CreatedAt = DateTime.Now;
    //             student.ModifiedAt = DateTime.Now;
    //             Context.Students.Add(student);
    //         }
    //         else
    //         {
    //             var existingStudent = Context.Students.FirstOrDefault(s => s.StudentSid == sid && s.Status == (int)StatusEnum.Active);
    //             Log.Information($"Student is updated with StudentSID {sid}");
    //             if (existingStudent == null)
    //             {
    //                 Log.Warning("No active student found with SID: {Sid}", sid);
    //                 return false;
    //             }
    //             existingStudent.ModifiedAt = DateTime.Now;
    //             _mapper.Map(model, existingStudent); // Maps updated fields
    //         }
    //
    //         return Context.SaveChanges() > 0;
    //     }
    //     catch (Exception ex)
    //     {
    //         Log.Error(ex, "Error in CreateOrUpdateStudent: {Message}", ex.Message);
    //         throw;
    //     }
    // }



}