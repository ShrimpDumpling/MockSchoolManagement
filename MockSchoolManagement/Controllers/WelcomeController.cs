using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockSchoolManagement.Infrastructure;
using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Controllers
{
    public class WelcomeController:Controller
    {
        private readonly IRepository<Student, int> _studnetRepository;

        public WelcomeController(IRepository<Student,int> repository)
        {
            _studnetRepository = repository;
        }
        
        public async Task<string> Index()
        {
            var studnet = await _studnetRepository.GetAll().FirstOrDefaultAsync();
            var oop = await _studnetRepository.SingleAsync(a => a.ID == 1006);
            var longcount = await _studnetRepository.LongCountAsync();
            var count = await _studnetRepository.CountAsync();
            return $"Name:{oop.Name}+{studnet.Name}+{longcount}+{count}";
        }
        public async Task<string> GetUserInfo(int id)
        {
            var oop = await _studnetRepository.SingleAsync(a => a.ID == id);
            if (oop==null)
            {
                return "you id is null";
            }
            return $"Id={oop.ID},Name={oop.Name},Email={oop.Email},major={oop.Major}";
        }
    }
}
