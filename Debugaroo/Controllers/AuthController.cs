using System.Data;
using AutoMapper;
using Dapper;
using Debugaroo.Data;
using Debugaroo.Dtos;
using Debugaroo.Helpers;
using Debugaroo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Debugaroo.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        private readonly ReusableSql _reusableSql;
        private readonly IMapper _mapper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
            _reusableSql = new ReusableSql(config);
            _mapper = new Mapper(new MapperConfiguration(cfg => 
            {
                cfg.CreateMap<AccountForRegistrationDto, Account>();
            }));
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(AccountForRegistrationDto accountForRegistration)
        {
            if(accountForRegistration.Password == accountForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT Email FROM UserData.Auth WHERE Email = '" 
                    + accountForRegistration.Email + "'";

                IEnumerable<string> existingAccounts = _dapper.LoadData<string>(sqlCheckUserExists);
                if(existingAccounts.Count() == 0)
                {
                    AccountForLoginDto accountForSetPassword = new AccountForLoginDto(){
                        Email = accountForRegistration.Email,
                        Password = accountForRegistration.Password
                    };

                    if(_authHelper.SetPassword(accountForSetPassword))
                    {
                        Account account = _mapper.Map<Account>(accountForRegistration);

                            if(_reusableSql.UpsertAccount(account))
                            {
                                return Ok();
                            }
                            
                            throw new Exception("Failed to add user.");
                    }
                    throw new Exception("Failed to register user.");
                }
                throw new Exception("User with this email already exists!");
            }
            throw new Exception("Passwords do not match!");
        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(AccountForLoginDto accountForSetPassword)
        {
             if(_authHelper.SetPassword(accountForSetPassword))
             {
                return Ok();
             }
             throw new Exception("Failed to update password!");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] AccountForLoginDto accountForLogin)
        {
            string sqlForHashAndSalt = @"EXEC Procedures.spLoginConfirmation_Get
                 @Email = @EmailParam";

            DynamicParameters sqlParameters = new DynamicParameters();

            sqlParameters.Add("@EmailParam", accountForLogin.Email, DbType.String);

            AccountForLoginConfirmationDto accountForConfirmation = _dapper
                .LoadDataSingleWithParameters<AccountForLoginConfirmationDto>(sqlForHashAndSalt, sqlParameters);
            
            byte[] passwordHash = _authHelper.GetPasswordHash(accountForLogin.Password, accountForConfirmation.PasswordSalt);

            for(int i = 0; i < passwordHash.Length; i++){
                if(passwordHash[i] != accountForConfirmation.PasswordHash[i]){
                    return StatusCode(401, "Incorrect Password!");
                }
            } 

            string accountIdSql = @"
                SELECT AccountId FROM UserData.Account WHERE Email= '" + 
                accountForLogin.Email + "'";

            int accountId = _dapper.LoadDataSingle<int>(accountIdSql);

            return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(accountId)}
            });
        }

        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            string accoundIdSql = @"
                SELECT AccountId FROM UserData.Account WHERE AccountId = '" + 
                User.FindFirst("accountId")?.Value + "'";

            int accoundId = _dapper.LoadDataSingle<int>(accoundIdSql);

            return _authHelper.CreateToken(accoundId);
        }

    }
}