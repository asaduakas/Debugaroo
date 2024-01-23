using Debugaroo.Data;
using Debugaroo.Dtos;
using Debugaroo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Debugaroo.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountCompleteController : ControllerBase
{
    DataContextDapper _dapper;
    public AccountCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    [HttpGet("GetAccounts/{accountId}")]
    public IEnumerable<Account> GetAccounts(int accountId)
    {
        string sql = "EXEC Procedures.spUsers_Get";
        if(accountId != 0){
            sql += " @AccountId=" + accountId.ToString();
        }
        IEnumerable<Account> accounts = _dapper.LoadData<Account>(sql);
        return accounts;   
    }

    [HttpPut("UpsertAccount")]
    public IActionResult UpsertAccount(Account account)
    {
        string sql = @"
        EXEC Procedures.spUser_Upsert
           @Username = '" + account.Username + 
           "', @FirstName = '" + account.FirstName + 
           "', @LastName = '" + account.LastName + 
           "', @Email = '" + account.Email + 
           "', @IsAdmin = '" + account.IsAdmin +
           "', @IsProjectManager = '" + account.IsProjectManager +
           "', @IsTeamLeader = '" + account.IsTeamLeader +
           "', @AccountId = " + account.AccountId;

        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to upsert account");
    }

    [HttpDelete("DeleteAccount/{accountId}")]
    public IActionResult DeleteAccount(int accountId)
    {
        string sql = @"EXEC Procedures.spUser_Delete 
            @AccountId = " +  accountId.ToString();

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to delete account");
    }
}
