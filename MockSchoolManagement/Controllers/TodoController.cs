using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MockSchoolManagement.Infrastructure;
using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class TodoController:ControllerBase
    {
        private readonly IRepository<TodoItem, int> _todoItemrepository;

        public TodoController(IRepository<TodoItem,int> todoItemrepository)
        {
            _todoItemrepository = todoItemrepository;
        }

        // GET:api/Todo
        [HttpGet]
        public async Task<ActionResult<List<TodoItem>>> GetTodo()
        {
            var models = await _todoItemrepository.GetAllListAsync();
            return models;
        }


        // GET:api/Todo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
        {
            var todoItem = await _todoItemrepository.FirstOrDefaultAsync(a => a.Id == id);

            if (todoItem==null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // PUT:api/Todo/5
        [HttpPut("{id}")]
        public async Task<ActionResult> PutTodoItem(long id,TodoItem todoItem)
        {
            if (id!=todoItem.Id)
            {
                return BadRequest();
            }
            await _todoItemrepository.UpdateAsync(todoItem);

            return NoContent();
        }


        // POST:api/Todo
        [HttpPost]
        public async Task<ActionResult> PostTodoItem(TodoItem todoItem)
        {
            await _todoItemrepository.InsertAsync(todoItem);
            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        // DELETE:api/Todo/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(long id)
        {
            var todoItem = await _todoItemrepository.FirstOrDefaultAsync(a => a.Id == id);
            if (todoItem==null)
            {
                return NotFound();
            }
            await _todoItemrepository.DeleteAsync(todoItem);
            return todoItem;
        }
    }
}
