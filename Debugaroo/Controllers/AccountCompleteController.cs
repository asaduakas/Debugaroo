using System.Data;
using Dapper;
using Debugaroo.Data;
using Debugaroo.Helpers;
using Debugaroo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Debugaroo.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AccountCompleteController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;
    public AccountCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _reusableSql = new ReusableSql(config);
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
        if (_reusableSql.UpsertAccount(account))
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
