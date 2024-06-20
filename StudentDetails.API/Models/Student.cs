namespace StudentDetails.API.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public List<Class> Classes { get; set; }
        public List<Mark> Marks { get; set; }
    }

    public class Class
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int SchoolId { get; set; }
        public School School { get; set; }
    }

    public class School
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; }
    }

    public class Mark
    {
        public int MarkId { get; set; }
        public string Subject { get; set; }
        public int Score { get; set; }
    }

}
