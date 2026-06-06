using Microsoft.AspNetCore.Mvc;
using TaskService.Models;

namespace TaskService.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private static readonly List<TaskItem> Tasks =
    [
        new TaskItem
        {
            Id = 1,
            Title = "Deploy Kubernetes Cluster",
            Description = "Deploy application to Kubernetes",
            Completed = false
        }
    ];

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(Tasks);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == id);

        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public IActionResult Create(TaskItem task)
    {
        Tasks.Add(task);

        return CreatedAtAction(
            nameof(Get),
            new { id = task.Id },
            task
        );
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, TaskItem updatedTask)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == id);

        if (task == null)
            return NotFound();

        task.Title = updatedTask.Title;
        task.Description = updatedTask.Description;
        task.Completed = updatedTask.Completed;

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == id);

        if (task == null)
            return NotFound();

        Tasks.Remove(task);

        return NoContent();
    }
}