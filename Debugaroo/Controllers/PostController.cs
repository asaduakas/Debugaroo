using Debugaroo.Data;
using Debugaroo.Dtos;
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

        [HttpGet("Posts")]
        public IEnumerable<Post> GetPosts()
        {
            string sql = @"SELECT [PostId],
                    [AccountId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM UserData.Posts";
            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("PostSingle/{postId}")]
        public Post GetPostSingle(int postId)
        {
            string sql = @"SELECT [PostId],
                    [AccountId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM UserData.Posts
                    WHERE PostId = " + postId.ToString();

            return _dapper.LoadDataSingle<Post>(sql);
        }

        [HttpGet("PostsByUser/{accountId}")]
        public IEnumerable<Post> GetPostsByUser(int accountId)
        {
            string sql = @"SELECT [PostId],
                    [AccountId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM UserData.Posts
                    WHERE AccountId = " + accountId.ToString();
                    
            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts(int accountId)
        {
            string sql = @"SELECT [PostId],
                    [AccountId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM UserData.Posts
                    WHERE AccountId = " + User.FindFirst("accountId")?.Value;
                    
            return _dapper.LoadData<Post>(sql);
        }

        [HttpPost("Post")]
        public IActionResult AddPost(PostToAddDto postToAddDto)
        {
            string sql = @"
            INSERT INTO UserData.Posts (
                [AccountId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]) VALUES (" + User.FindFirst("accountId")?.Value
                + ",'" + postToAddDto.PostTitle
                + "','" + postToAddDto.PostContent
                + "', GETDATE(), GETDATE() )";

            if(_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to create new post!");
        }

        [HttpPut("Post")]
        public IActionResult EditPost(PostToEditDto postToEditDto)
        {
            string sql = @"
            UPDATE UserData.Posts 
                SET PostContent = '" +  postToEditDto.PostContent 
                + "', PostTitle = '" + postToEditDto.PostTitle +
                @"', PostUpdated = GETDATE()
                    WHERE PostId = " + postToEditDto.PostId.ToString() +
                    "AND AccountId = " + User.FindFirst("accountId")?.Value;

            if(_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to edit post!");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"DELETE FROM UserData.Posts 
                WHERE PostId = " + postId.ToString() + 
                "AND AccountId = " + User.FindFirst("accountId")?.Value;;

            if(_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to delete post!");
        }

        [HttpGet("PostsBySearch/{searchParam}")]
        public IEnumerable<Post> PostsBySearch(string searchParam)
        {
            string sql = @"SELECT [PostId],
                    [AccountId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM UserData.Posts
                    WHERE PostTitle LIKE '%" + searchParam + @"%'
                    OR PostContent LIKE '%" + searchParam + "%'";
                    
            return _dapper.LoadData<Post>(sql);
        }
    }
}