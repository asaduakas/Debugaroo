using System.Data;
using Dapper;
using Debugaroo.Data;
using Debugaroo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Debugaroo.Controllers
{
    [Authorize]
    [ApiController]
    [Route("controller")]
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
            string stringParams = "";
            DynamicParameters sqlParams = new();
            
            if(postId != 0)
            {
                stringParams += ", @PostId=@PostIdParam";
                sqlParams.Add("@PostIdParam", postId, DbType.Int32);
            }
            if(accountId != 0)
            {
                stringParams += ", @AccountId=@AccountIdParam";
                sqlParams.Add("@AccountIdParam", accountId, DbType.Int32);
            }
            if(searchParam.ToLower() != "none")
            {
                stringParams += ", @SearchValue=@SearchParam";
                sqlParams.Add("@SearchParam", searchParam, DbType.String);
            }

            if(stringParams.Length > 0){
                sql += stringParams.Substring(1);
            }

            return _dapper.LoadDataWithParameters<Post>(sql,sqlParams);
        }


        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"EXEC Procedures.spPost_Get @AccountId = @AccountIdParam";
            DynamicParameters sqlParams = new();
            sqlParams.Add("@AccountIdParam",User.FindFirst("accountId")?.Value, DbType.Int32);

            return _dapper.LoadDataWithParameters<Post>(sql,sqlParams);
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToUpsert)
        {
            string sql = @"EXEC Procedures.spPost_Upsert
                @AccountId = @AccountIdParam
                , @PostTitle = @PostTitleParam
                , @PostContent = @PostContentParam";

            DynamicParameters sqlParams = new();
            sqlParams.Add("@AccountIdParam",User.FindFirst("accountId")?.Value, DbType.Int32);
            sqlParams.Add("@PostTitleParam",postToUpsert.PostTitle,DbType.String);
            sqlParams.Add("@PostContentParam",postToUpsert.PostContent, DbType.String);

            if(postToUpsert.PostId > 0){
                sql += ", @PostId = @PostIdParam";
                sqlParams.Add("@PostIdParam",postToUpsert.PostId, DbType.Int32);
            }

            if(_dapper.ExecuteSqlWithParameters(sql,sqlParams))
            {
                return Ok();
            }
            throw new Exception("Failed to upsert post!");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"EXEC Procedures.spPost_Delete 
                @PostId = @PostIdParam, 
                @AccountId = @AccountIdParam";
                 
            DynamicParameters sqlParams = new();
            sqlParams.Add("@PostIdParam",postId.ToString(),DbType.Int32);
            sqlParams.Add("@AccountIdParam",User.FindFirst("accountId")?.Value,DbType.Int32);

            if(_dapper.ExecuteSqlWithParameters(sql,sqlParams))
            {
                return Ok();
            }
            throw new Exception("Failed to delete post!");
        }

    }
}