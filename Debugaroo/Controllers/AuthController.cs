using System.Data;
using System.Security.Cryptography;
using System.Text;
using Debugaroo.Data;
using Debugaroo.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Debugaroo.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

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

                    string passwordSaltPlusKey = _config.GetSection("AppSettings:PasswordKey").Value
                        + Convert.ToBase64String(passwordSalt);

                    byte[] passwordHash = KeyDerivation.Pbkdf2(
                        password: accountForRegistration.Password,
                        salt: Encoding.ASCII.GetBytes(passwordSaltPlusKey),
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 1000000,
                        numBytesRequested: 256/8
                    );

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
                        return Ok();
                    }
                    throw new Exception("Failed to register user.");
                }
                throw new Exception("User with this email already exists!");
            }
            throw new Exception("Passwords do not match!");
        }

        [HttpPost("Login")]
        public IActionResult Login(AccountForLoginDto accountForLogin)
        {
            return Ok();
        }
    }
}