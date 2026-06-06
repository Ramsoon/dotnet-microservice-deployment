using Microsoft.AspNetCore.Mvc;
using UserService.Models;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private static readonly List<User> Users =
    [
        new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com"
        }
    ];

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(Users);
    }

    [HttpPost]
    public IActionResult Create(User user)
    {
        Users.Add(user);

        return Ok(user);
    }
}