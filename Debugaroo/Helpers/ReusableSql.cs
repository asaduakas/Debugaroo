using System.Data;
using Dapper;
using Debugaroo.Data;
using Debugaroo.Models;

namespace Debugaroo.Helpers
{
    public class ReusableSql
    {
        private readonly DataContextDapper _dapper;
        public ReusableSql(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        public bool UpsertAccount(Account account)
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

            return _dapper.ExecuteSqlWithParameters(sql, sqlParameters);
        }
    }
}