namespace Debugaroo.Models
{
    public partial class Post
    {
        public int PostId {get; set;}
        public int AccountId {get; set;}
        public string PostTitle {get; set;} = "";
        public string PostContent {get; set;} = "";
        public DateTime PostCreated {get; set;}
        public DateTime PostUpdated {get; set;}
    }
}