using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using PostsViews;
using TweetModels;
using UserModels;

namespace Posts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public PostsController(AppDbContext context, IDbContextFactory<AppDbContext> contextFactory)
        {
            _context = context;
            _contextFactory = contextFactory;
        }

        [HttpGet("get_all_last_posts")]
        public async Task<ActionResult<IEnumerable<TweetExtend>>> GetAllLastPosts([FromQuery] DateTime time)
        {
            List<TweetExtend> tweetExtend = await _context.Tweets
                .Where((Tweet tweet) => tweet.datacreazione > time)
                .Join(
                    _context.Users,
                    (Tweet tweet) => tweet.idutente,
                    (User user) => user.id,
                    (Tweet tweet, User user) => new TweetExtend
                    {
                        id = tweet.id,
                        datacreazione = tweet.datacreazione,
                        testo = tweet.testo,
                        idutente = tweet.idutente,
                        immaginepost = tweet.immaginepost,
                        userProfilePic = user.profilepic,
                        userName = user.nome
                    }
                )
                .ToListAsync();

            return Ok(tweetExtend);
        }

        [HttpGet("get_profilo")]
        public async Task<ActionResult<Profilo>> GetProfiloById([FromQuery] string idutente)
        {
            try
            {
                using (AppDbContext newContext = _contextFactory.CreateDbContext())
                {
                    Task<User?> utenteTask = _context.Users.FirstOrDefaultAsync((User u) => u.id == idutente);
                    Task<List<Tweet>> tweetsTask = newContext.Tweets
                        .Where((Tweet t) => t.idutente == idutente)
                        .OrderByDescending((Tweet tweet) => tweet.datacreazione)
                        .Take(20)
                        .ToListAsync();

                    await Task.WhenAll(utenteTask, tweetsTask);

                    Profilo profilo = new Profilo(utenteTask.Result!, tweetsTask.Result);

                    return Ok(profilo);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}
