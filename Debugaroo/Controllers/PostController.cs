using Debugaroo.Data;
using Debugaroo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Debugaroo.Controllers
{
    [Authorize]
    [ApiController]
    [Route("Post")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("Posts/{postId}/{accountId}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int accountId = 0, string searchParam = "None")
        {
            string sql = @"EXEC Procedures.spPost_Get";
            string parameters = "";
            
            if(postId != 0)
            {
                parameters += ", @PostId=" + postId.ToString();
            }
            if(accountId != 0)
            {
                parameters += ", @AccountId=" + accountId.ToString();
            }
            if(searchParam.ToLower() != "none")
            {
                parameters += "@SearchValue='" + searchParam + "'";
            }

            if(parameters.Length > 0){
                sql += parameters.Substring(1);
            }

            return _dapper.LoadData<Post>(sql);
        }


        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts(int accountId)
        {
            string sql = @"EXEC Procedures.spPost_Get @AccountId = " + 
                User.FindFirst("accountId")?.Value;
                    
            return _dapper.LoadData<Post>(sql);
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToUpsert)
        {
            string sql = @"EXEC Procedures.spPost_Upsert
                @AccountId =" + User.FindFirst("accountId")?.Value +
                ", @PostTitle = '" + postToUpsert.PostTitle +
                "', @PostContent ='" + postToUpsert.PostContent + "'";

            if(postToUpsert.PostId > 0){
                sql += ", @PostId = " + postToUpsert.PostId;
            }

            Console.WriteLine(sql);
            if(_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to upsert post!");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"EXEC Procedures.spPost_Delete @PostId =" + 
                postId.ToString() + 
                ", @AccountId = " + User.FindFirst("accountId")?.Value;;

            if(_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to delete post!");
        }

    }
}