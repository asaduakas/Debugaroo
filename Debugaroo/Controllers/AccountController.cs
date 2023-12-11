using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace Debugaroo.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    DataContextDapper _dapper;
    public AccountController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }
    
    [HttpGet("GetUsers")]
    public string[] GetUsers()
    {
        string[] responseArray = new string[] {
            "Ticket 1",
            "Ticket 2"
        };
        return responseArray;
    }
}
