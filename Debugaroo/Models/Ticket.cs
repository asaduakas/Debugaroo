namespace Debugaroo
{
    public partial class Tickets
    {
        public int TicketId { get; set; }
        public string Title { get; set; } = "";
        public string TicketDescription { get; set; } = "";
        public int AssignedDeveloperId { get; set; }
        public int SubmitterId { get; set; }
        public int Project{ get; set; }
        public int BugType { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        public DateTime UpdatedDateTime { get; set; }
        public DateTime ClosedDateTime { get; set; }
        public string TicketPriority { get; set; } = "";
        public string TicketStatus { get; set; } = "Created";
    }

}