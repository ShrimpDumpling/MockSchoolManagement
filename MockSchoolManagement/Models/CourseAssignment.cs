namespace MockSchoolManagement.Models
{
    public class CourseAssignment
    {
        public int CourseAssignmentId { get; set; }
        public int TeacherId { get; set; }
        public int CourseId { get; set; }
        public Teacher Teacher { get; set; }
        public Course Course { get; set; }
    }
}
