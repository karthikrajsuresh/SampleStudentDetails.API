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
                       sch.SchoolId, sch.Name AS SchoolName,
                       m.MarkId, m.Subject, m.Score
                FROM Students s
                INNER JOIN Classes c ON s.ClassId = c.ClassId
                INNER JOIN Schools sch ON c.SchoolId = sch.SchoolId
                LEFT JOIN Marks m ON s.StudentId = m.StudentId
                WHERE s.StudentId = @StudentId;
            ";

            var studentDictionary = new Dictionary<int, Student>();

            var student = (await connection.QueryAsync<Student, Class, School, Mark, Student>(
                studentQuery,
                (student, classItem, school, mark) =>
                {
                    if (!studentDictionary.TryGetValue(student.StudentId, out var currentStudent))
                    {
                        currentStudent = student;
                        studentDictionary.Add(currentStudent.StudentId, currentStudent);
                    }

                    if (currentStudent.Classes == null)
                    {
                        currentStudent.Classes = new List<Class>();
                    }

                    if (!currentStudent.Classes.Any(c => c.ClassId == classItem.ClassId))
                    {
                        classItem.School = school;
                        currentStudent.Classes.Add(classItem);
                    }

                    if (currentStudent.Marks == null)
                    {
                        currentStudent.Marks = new List<Mark>();
                    }

                    if (mark != null && !currentStudent.Marks.Any(m => m.MarkId == mark.MarkId))
                    {
                        currentStudent.Marks.Add(mark);
                    }

                    return currentStudent;
                },
                new { StudentId = studentId },
                splitOn: "ClassId,SchoolId,MarkId"
            )).FirstOrDefault();

            return student;
        }
    }
}
