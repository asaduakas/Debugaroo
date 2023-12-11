using Microsoft.AspNetCore.Mvc;

namespace Debugaroo.Controllers;

[ApiController]
[Route("[controller]")]
public class TicketController : ControllerBase
{
    public TicketController()
    {

    }
    

    [HttpGet("ticket")]
    public string[] Get()
    {
        string[] responseArray = new string[] {
            "Ticket 1",
            "Ticket 2"
        };
        return responseArray;
    }
}
