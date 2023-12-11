namespace Debugaroo
{
    public partial class Account
    {
        public int AccountId { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsAdmin { get; set; } 
        public bool IsProjectManager { get; set; } 
        public bool IsTeamLeader { get; set; }
    }

}