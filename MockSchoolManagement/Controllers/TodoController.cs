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

        /// <summary>
        /// 获取所有待办事项
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<TodoItem>>> GetTodo()
        {
            var models = await _todoItemrepository.GetAllListAsync();
            return models;
        }


        /// <summary>
        /// 根据Id获取待办事项
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 更新待办事项
        /// </summary>
        /// <param name="id"></param>
        /// <param name="todoItem"></param>
        /// <returns></returns>
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


        /// <summary>
        /// 添加待办事项
        /// </summary>
        /// <param name="todoItem"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> PostTodoItem(TodoItem todoItem)
        {
            await _todoItemrepository.InsertAsync(todoItem);
            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        /// <summary>
        /// 删除指定Id的待办事项
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
