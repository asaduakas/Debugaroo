namespace Debugaroo.Dtos
{
    public partial class AccountToAddDto
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public bool IsAdmin { get; set; } 
        public bool IsProjectManager { get; set; } 
        public bool IsTeamLeader { get; set; }
    }

}