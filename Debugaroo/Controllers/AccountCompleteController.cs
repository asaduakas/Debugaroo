using System.Data;
using Dapper;
using Debugaroo.Data;
using Debugaroo.Models;
using Microsoft.AspNetCore.Mvc;

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
        DynamicParameters sqlParameters = new DynamicParameters();

        if(accountId != 0){
            sql += " @AccountId=@AccountIdParameter";
            sqlParameters.Add("@AccountIdParameter", accountId, DbType.Int32);
        }
        IEnumerable<Account> accounts = _dapper.LoadDataWithParameters<Account>(sql,sqlParameters);
        return accounts;   
    }

    [HttpPut("UpsertAccount")]
    public IActionResult UpsertAccount(Account account)
    {
        string sql = @"EXEC Procedures.spUser_Upsert
           @AccountId = @AccountIdParameter
           @Username = @UsernameParameter,
           @FirstName = @FirstNameParameter,
           @LastName = @LastNameParameter, 
           @Email = @EmailParameter,
           @IsAdmin = @IsAdminParameter,
           @IsProjectManager = @IsProjectManagerParameter,
           @IsTeamLeader = @IsTeamLeaderParameter";

        DynamicParameters sqlParameters = new();

        sqlParameters.Add("@AccountIdParameter", account.AccountId, DbType.Int32);
        sqlParameters.Add("@UsernameParameter", account.Username, DbType.String);
        sqlParameters.Add("@FirstNameParameter", account.FirstName, DbType.String);
        sqlParameters.Add("@LastNameParameter", account.LastName, DbType.String);
        sqlParameters.Add("@EmailParameter", account.Email, DbType.String);
        sqlParameters.Add("@IsAdminParameter", account.IsAdmin, DbType.Boolean);
        sqlParameters.Add("@IsProjectManagerParameter", account.IsProjectManager, DbType.Boolean);
        sqlParameters.Add("@IsTeamLeaderParameter", account.IsTeamLeader, DbType.Boolean);
       
        Console.WriteLine(sql);

        if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
        {
            return Ok();
        }
        throw new Exception("Failed to upsert account");
    }

    [HttpDelete("DeleteAccount/{accountId}")]
    public IActionResult DeleteAccount(int accountId)
    {
        string sql = @"EXEC Procedures.spUser_Delete 
            @AccountId = @AccountIdParameter";

        DynamicParameters sqlParameters = new DynamicParameters();
        sqlParameters.Add("@AccountIdParameter", accountId, DbType.Int32);

        if (_dapper.ExecuteSqlWithParameters(sql,sqlParameters))
        {
            return Ok();
        }
        throw new Exception("Failed to delete account");
    }
}
