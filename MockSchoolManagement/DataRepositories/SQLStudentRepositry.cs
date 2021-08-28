using Microsoft.Extensions.Logging;
using MockSchoolManagement.Infrastructure;
using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockSchoolManagement.DataRepositories
{
    public class SQLStudentRepositry : IStudentRepository
    {
        private readonly AppDbContext _context;
        private ILogger _logger;
        public SQLStudentRepositry(AppDbContext context,ILogger<SQLStudentRepositry> logger)
        {
            _context = context;
            _logger = logger;
        }
        public Student Delete(int id)
        {
            Student student = _context.Students.Find(id);
            if (student!=null)
            {
                _context.Students.Remove(student);
                _context.SaveChanges();
            }
            return student;
        }

        public IEnumerable<Student> GEtAllStudents()
        {
            
            return _context.Students;
        }

        public Student GetStudent(int id)
        {
            return _context.Students.Find(id);
        }

        public Student Insert(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();
            return student;
        }

        public Student Update(Student updateStudent)
        {
            var student = _context.Students.Attach(updateStudent);
            student.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return updateStudent;
        }
    }
}

