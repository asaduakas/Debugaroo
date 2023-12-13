using Debugaroo.Data;
using Debugaroo.Dtos;
using Debugaroo.Models;
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
                [Username],
                [Email],
                [IsAdmin],
                [IsProjectManager],
                [IsTeamLeader] 
            FROM UserData.Account";
        IEnumerable<Account> accounts = _dapper.LoadData<Account>(sql);
        return accounts;   
    }

    [HttpGet("GetSingleAccount/{accountId}")]
    public Account GetSingleAccount(int accountId)
    {
        string sql = @"
            SELECT [AccountId],
                   [Usename],
                   [Email],
                   [IsAdmin],
                   [IsProjectManager],
                   [IsTeamLeader] 
            FROM UserData.Account
                WHERE AccountId = " + accountId;
        Account account = _dapper.LoadDataSingle<Account>(sql);
        return account;  
    }

    [HttpPut("EditAccount")]
    public IActionResult EditAccount(Account account)
    {
        string sql = @"
        UPDATE UserData.Account
            SET [Username] = '" + account.Username + 
                "',[Email] = '" + account.Email + 
                "',[IsAdmin] = '" + account.IsAdmin +
                "',[IsProjectManager] = '" + account.IsProjectManager +
                "',[IsTeamLeader] = '" + account.IsTeamLeader +
                "' WHERE AccountId = " + account.AccountId;

                Console.WriteLine(sql);

                if (_dapper.ExecuteSql(sql))
                {
                    return Ok();
                }
                throw new Exception("Failed to update account");
    }

    [HttpPost("AddAccount")]
    public IActionResult AddAccount(AccountToAddDto account)
    {
        string sql = @"
            INSERT INTO UserData.Account(
                [Username],
                [Email],
                [IsAdmin],
                [IsProjectManager],
                [IsTeamLeader]
            ) VALUES (" +
                "'" + account.Username + 
                "', '" + account.Email + 
                "', '" + account.IsAdmin +
                "', '" + account.IsProjectManager +
                "', '" + account.IsTeamLeader +
            "')";

        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to add account");
    }

    [HttpDelete("DeleteUser/{accountId}")]
    public IActionResult DeleteAccount(int accountId)
    {
        string sql = @"
        DELETE FROM UserData.Account 
            WHERE AccountId = " +  accountId.ToString();

        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to delete account");
    }
}
