namespace Debugaroo.Dtos
{
    public partial class AccountForRegistrationDto
    {
        public string Email {get; set;} = "";
        public string Password {get; set;} = "";
        public string PasswordConfirm {get; set;} = "";
    }
}