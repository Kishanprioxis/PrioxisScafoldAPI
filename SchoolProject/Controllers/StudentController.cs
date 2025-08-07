using Microsoft.AspNetCore.Mvc;
using Models.Models.RequestModel;
using Models.Models.ResponseModel;
using Models.Models.School;
using Serilog;
using Service.Repository.Interface;

namespace SchoolProject.Controllers;
[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
   private readonly IStudentRepository _studentRepository;

    public StudentController(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    [HttpGet("students")]
    public IActionResult Index([FromQuery]OptionsModel options)
    {
        Log.Information("In StudentController Index Method");

        var students = _studentRepository.GetFilteredStudents(options.Page, options.Pagesize, options.Search);

        if (students == null || !students.Any())
        {
            Log.Warning("No students found with the given criteria. Page: {Page}, PageSize: {PageSize}, Search: {Search}", options.Page, options.Pagesize, options.Search);
            return NotFound(new { Message = "No students found." });
        }

        return Ok(students);
    }



    [HttpPost("students/{sid?}")]
    public IActionResult CreateStudent([FromBody] StudentRequestModel reqStudent, [FromRoute] string? sid = null)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }
        Log.Information("In StudentController CreateStudent Method");

        bool result = _studentRepository.CreateOrUpdateStudent(reqStudent, sid);

        if (!result)
        {
            Log.Warning("Student not found for update with SID: {Sid}", sid);
            return NotFound(new { Message = "Student not found." });
        }

        string message = sid == null ? "Student inserted successfully." : "Student updated successfully.";
        return Ok(new { Message = message });
    }



    [HttpDelete("deleteStudent/{sid}")]
    public IActionResult DeleteStudent([FromRoute] string sid)
    {
        Log.Information("In StudentController DeleteStudent Method for SID: {Sid}", sid);

        var isDeleted = _studentRepository.DeleteStudent(sid);

        if (!isDeleted)
        {
            Log.Warning("Delete failed. No student found with SID: {Sid}", sid);
            return NotFound(new { Message = "Student not found." });
        }

        return Ok(new { Message = "Student deleted successfully." });
    }


    [HttpGet("students/{sid}")]
    public IActionResult GetStudent([FromRoute] string sid)
    {
        Log.Information("In StudentController GetStudent Method for SID: {Sid}", sid);

        var studentResponse = _studentRepository.GetStudentBySid(sid);

        if (studentResponse == null)
        {
            Log.Warning("No student found with SID: {Sid}", sid);
            return NotFound(new { Message = "Student not found." });
        }

        return Ok(studentResponse);
    }

    [HttpGet("exception-check")]
    public IActionResult ExceptionCheck()
    {
        throw new ArgumentException("In StudentController ExceptionCheck Method");
    }

}
