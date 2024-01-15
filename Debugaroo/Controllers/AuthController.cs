using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Debugaroo.Data;
using Debugaroo.Dtos;
using Debugaroo.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace Debugaroo.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
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
                    byte[] passwordSalt = new byte[128/8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash = _authHelper.GetPasswordHash(accountForRegistration.Password, passwordSalt);

                    string sqlAddAuth = @"
                        INSERT INTO UserData.Auth ([Email],
                        [PasswordHash],
                        [PasswordSalt]) VALUES ('" + accountForRegistration.Email + 
                        "', @PasswordHash, @PasswordSalt)";
                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    SqlParameter passwordSaltParameter =  new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;
                    SqlParameter passwordHashParameter =  new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;

                    sqlParameters.Add(passwordSaltParameter);
                    sqlParameters.Add(passwordHashParameter);

                    if(_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
                    {
                         string sqlAddUser = @"
                            INSERT INTO UserData.Account(
                                [Username],
                                [Email],
                                [FirstName],
                                [LastName]
                            ) VALUES (" +
                                "'" + accountForRegistration.Username + 
                                "', '" + accountForRegistration.Email +
                                "', '" + accountForRegistration.FirstName + 
                                "', '" + accountForRegistration.LastName +  
                            "')";
                            if(_dapper.ExecuteSql(sqlAddUser))
                            {
                                return Ok();
                            }
                            Console.WriteLine(sqlAddUser);
                            throw new Exception("Failed to add user.");
                    }
                    throw new Exception("Failed to register user.");
                }
                throw new Exception("User with this email already exists!");
            }
            throw new Exception("Passwords do not match!");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(AccountForLoginDto accountForLogin)
        {
            string sqlForHashAndSalt = @"SELECT
                [PasswordHash],
                [PasswordSalt] FROM UserData.Auth WHERE Email = '" +
                accountForLogin.Email + "'";
            AccountForLoginConfirmationDto accountForConfirmation = _dapper.LoadDataSingle<AccountForLoginConfirmationDto>(sqlForHashAndSalt);
            
            byte[] passwordHash = _authHelper.GetPasswordHash(accountForLogin.Password, accountForConfirmation.PasswordSalt);

            for(int i = 0; i < passwordHash.Length; i++){
                if(passwordHash[i] != accountForConfirmation.PasswordHash[i]){
                    return StatusCode(401, "Incorrect Password!");
                }
            } 

            string accoundIdSql = @"
                SELECT AccountId FROM UserData.Account WHERE Email= '" + 
                accountForLogin.Email + "'";

            int accountId = _dapper.LoadDataSingle<int>(accoundIdSql);

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