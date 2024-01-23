namespace Debugaroo.Dtos
{
    public partial class AccountForRegistrationDto
    {
        public string Username { get; set; } = "";
        public string Email {get; set;} = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Password {get; set;} = "";
        public string PasswordConfirm {get; set;} = "";
        public bool IsAdmin { get; set; } = false;
        public bool IsProjectManager { get; set; } = false;
        public bool IsTeamLeader { get; set; } = false;
    }
}
