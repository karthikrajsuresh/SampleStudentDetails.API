using Microsoft.Extensions.Configuration;
using StudentDetails.API.Contracts;
using StudentDetails.API.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace StudentDetails.API.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly string _connectionString;

        public StudentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentDetailsConnection");
        }

        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            using var connection = new SqlConnection(_connectionString);

            var studentQuery = @"
                SELECT s.StudentId, s.Name, 
                       c.ClassId, c.Name AS ClassName, 
                       sch.SchoolId, sch.Name AS SchoolName
                FROM Students s
                INNER JOIN Classes c ON s.ClassId = c.ClassId
                INNER JOIN Schools sch ON c.SchoolId = sch.SchoolId
                WHERE s.StudentId = @StudentId;

                SELECT m.MarkId, m.Subject, m.Score
                FROM Marks m
                WHERE m.StudentId = @StudentId;
            ";

            using var multi = await connection.QueryMultipleAsync(studentQuery, new { StudentId = studentId });

            var student = (await multi.ReadAsync<Student>()).FirstOrDefault();
            if (student == null)
            {
                return null;
            }

            var classes = (await multi.ReadAsync<Class>()).ToList();
            var schools = (await multi.ReadAsync<School>()).ToList();

            // Assign the correct School to each Class
            foreach (var classItem in classes)
            {
                classItem.School = schools.FirstOrDefault(s => s.SchoolId == classItem.SchoolId);
            }

            student.Classes = classes;
            student.Marks = (await multi.ReadAsync<Mark>()).ToList();

            return student;
        }
    }
}
