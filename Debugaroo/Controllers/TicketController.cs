using System.Data;
using Dapper;
using Debugaroo.Data;
using Debugaroo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Debugaroo.Controllers;

[Authorize]
[ApiController]
[Route("[Ticket]")]
public class TicketController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    public TicketController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    //TODO: REMOVE ANONYMOUS HERE
    [AllowAnonymous]
    [HttpGet("Tickets/{ticketId}/{submitterId}/{assignedDeveloperId}/{searchParam}")]
    public IEnumerable<Ticket> GetTickets(int ticketId = 0, int submitterId = 0, int assignedDeveloperId = 0, string searchParam = "none")
    {
        string sql = @"EXEC Procedures.spTicket_Get";
        string stringParams = "";
        DynamicParameters sqlParams = new();
        
        if(ticketId != 0)
        {
            stringParams += ", @TicketId=@TicketIdParam";
            sqlParams.Add("@TicketIdParam", ticketId, DbType.Int32);
        }
        if(submitterId != 0)
        {
            stringParams += ", @SubmitterId=@SubmitterIdParam";
            sqlParams.Add("@AccountIdParam", submitterId, DbType.Int32);
        }
        if(assignedDeveloperId != 0)
        {
            stringParams += ", @AssignedDeveloperId=@AssignedDevIdParam";
            sqlParams.Add("@AssignedDevIdParam", assignedDeveloperId, DbType.Int32);
        }
        if(searchParam.ToLower() != "none")
        {
            stringParams += ", @SearchValue=@SearchParam";
            sqlParams.Add("@SearchParam", searchParam, DbType.String);
        }

        if(stringParams.Length > 0){
            sql += stringParams.Substring(1);
        }

        return _dapper.LoadDataWithParameters<Ticket>(sql,sqlParams);
    }

    [HttpPut("UpsertTicket")]
    public IActionResult UpsertTicket(Ticket ticketToUpsert)
    {
        string sql = @"EXEC Procedures.spTicket_Upsert
            @SubmitterId = @SubmitterIdParam,
            @AssignedDeveloperId = @AssignedDevParam,
            @Title = @TitleParam,
            @TicketDescription = @TicketDescriptionParam,
            @Project = @ProjectParam,
            @BugType = @BugTypeParam,
            @TicketPriority = @TicketPriorityParam,
            @TicketStatus = @TicketStatusParam";

        DynamicParameters sqlParams = new();
        sqlParams.Add("@SubmitterIdParam",User.FindFirst("accountId")?.Value, DbType.Int32);
        sqlParams.Add("@AssignedDevParam",ticketToUpsert.AssignedDeveloperId,DbType.Int32);
        sqlParams.Add("@TitleParam",ticketToUpsert.Title, DbType.String);
        sqlParams.Add("@TicketDescriptionParam",ticketToUpsert.TicketDescription, DbType.String);
        sqlParams.Add("@ProjectParam",ticketToUpsert.Project, DbType.Int32);
        sqlParams.Add("@BugTypeParam",ticketToUpsert.BugType, DbType.Int32);
        sqlParams.Add("@TicketPriorityParam",ticketToUpsert.TicketPriority, DbType.String);
        sqlParams.Add("@TicketStatusParam",ticketToUpsert.TicketStatus, DbType.String);

        if(ticketToUpsert.TicketId > 0){
            sql += ", @TicketId = @TicketIdParam";
            sqlParams.Add("@TicketIdParam",ticketToUpsert.TicketId, DbType.Int32);
        }

        if(_dapper.ExecuteSqlWithParameters(sql,sqlParams))
        {
            return Ok();
        }
        throw new Exception("Failed to upsert ticket!");
    }

    //TODO: Assigned Developers can delete tickets. Make sure project managers and team leads can do it too.
    [HttpDelete("Ticket/{ticketId}")]
        public IActionResult DeleteTicket(int ticketId)
        {
            string sql = @"EXEC Procedures.spTicket_Delete 
                @TicketId = @TicketIdParam, 
                @AccountId = @AccountIdParam";
                 
            DynamicParameters sqlParams = new();
            sqlParams.Add("@TicketIdParam",ticketId.ToString(),DbType.Int32);
            sqlParams.Add("@AccountIdParam",User.FindFirst("accountId")?.Value,DbType.Int32);

            if(_dapper.ExecuteSqlWithParameters(sql,sqlParams))
            {
                return Ok();
            }
            throw new Exception("Failed to delete post!");
        }
}
