using StudentDetails.API.Models;

namespace StudentDetails.API.Contracts
{
    public interface IStudentRepository
    {
        Task<Student> GetStudentByIdAsync(int studentId);
    }
}
