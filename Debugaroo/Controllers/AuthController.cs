using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Debugaroo.Data;
using Debugaroo.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
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
        private readonly IConfiguration _config;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
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

                    byte[] passwordHash = GetPasswordHash(accountForRegistration.Password, passwordSalt);

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
            
            byte[] passwordHash = GetPasswordHash(accountForLogin.Password, accountForConfirmation.PasswordSalt);

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
                {"token", CreateToken(accountId)}
            });
        }

        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            string accoundIdSql = @"
                SELECT AccountId FROM UserData.Account WHERE AccountId = '" + 
                User.FindFirst("accountId")?.Value + "'";

            int accoundId = _dapper.LoadDataSingle<int>(accoundIdSql);

            return CreateToken(accoundId);
        }

        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusKey = _config.GetSection("AppSettings:PasswordKey").Value
                + Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusKey),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000000,
                numBytesRequested: 256/8
            );
        }

        private string CreateToken(int accountId)
        {
            Claim[] claims = new Claim[] {
                new Claim("accountId", accountId.ToString())
            };

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _config.GetSection("Appsettings:TokenKey").Value
                    )
                );

            SigningCredentials credentials = new SigningCredentials(
                    tokenKey, 
                    SecurityAlgorithms.HmacSha512Signature
                );

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(claims),
                    SigningCredentials = credentials,
                    Expires = DateTime.Now.AddDays(1)
                };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}