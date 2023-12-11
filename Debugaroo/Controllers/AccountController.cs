using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("GetAccounts")]
    public IEnumerable<Account> GetAccounts()
    {
        string sql = @"
            SELECT [AccountId],
                [UserName],
                [Email],
                [IsAdmin],
                [IsProjectManager],
                [IsTeamLeader] 
            FROM UserData.Account";
        IEnumerable<Account> accounts = _dapper.LoadData<Account>(sql);
        return accounts;   
    }

    [HttpGet("GetSingleAccount/{userId}")]
    public Account GetSingleAccount(int userId)
    {
        string sql = @"
            SELECT [AccountId],
                [UserName],
                [Email],
                [IsAdmin],
                [IsProjectManager],
                [IsTeamLeader] 
            FROM UserData.Account
                WHERE AccountId = " + userId.ToString();
        Account account = _dapper.LoadDataSingle<Account>(sql);
        return account;  
    }
}
