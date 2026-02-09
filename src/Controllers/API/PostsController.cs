using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using PostsViews;
using TweetModels;
using UserModels;
using PostsForms;
using System.Text.Json;

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
                .Where((Tweet tweet) => tweet.dataCreazione > time)
                .Join(
                    _context.Users,
                    (Tweet tweet) => tweet.idUtente,
                    (User user) => user.id,
                    (Tweet tweet, User user) => new TweetExtend
                    {
                        id = tweet.id,
                        dataCreazione = tweet.dataCreazione,
                        testo = tweet.testo,
                        idUtente = tweet.idUtente,
                        immaginePost = tweet.immaginePost,
                        userProfilePic = user.profilePic,
                        userName = user.nome
                    }
                )
                .ToListAsync();

            return Ok(tweetExtend);
        }

        [HttpGet("get_profilo")]
        public async Task<ActionResult<Profilo>> GetProfiloById([FromQuery] string idUtente)
        {
            try
            {
                using (AppDbContext newContext = _contextFactory.CreateDbContext())
                {
                    Task<User?> utenteTask = _context.Users.FirstOrDefaultAsync((User u) => u.id == idUtente);
                    Task<List<Tweet>> tweetsTask = newContext.Tweets
                        .Where((Tweet t) => t.idUtente == idUtente)
                        .OrderByDescending((Tweet tweet) => tweet.dataCreazione)
                        .Take(20)
                        .ToListAsync();

                    await Task.WhenAll(utenteTask, tweetsTask);

                    User userNew = utenteTask.Result!;
                    UserPost userPost = new UserPost
                    {
                        id = userNew.id,
                        nome = userNew.nome,
                        email = userNew.email,
                        password = userNew.password,
                        profilePic = userNew.profilePic,
                        stato = userNew.stato,
                        provincia = userNew.provincia,
                        bio = userNew.bio,
                        telefono = userNew.telefono,
                        compleanno = userNew.compleanno,
                        social = FormatSocial(userNew.social),
                    };

                    Profilo profilo = new Profilo(userPost, tweetsTask.Result);

                    return Ok(profilo);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("post_tweet")]
        public async Task<ActionResult> PostTweet([FromBody] PostsUtenteForm postForm)
        {
            try
            {
                Tweet newTweet = new Tweet
                {
                    dataCreazione = DateTime.UtcNow,
                    testo = postForm.testo,
                    idUtente = postForm.idUtente,
                    immaginePost = postForm.immaginePost
                };

                _context.Tweets.Add(newTweet);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Tweet aggiunto con successo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex}");
            }
        }

        [HttpPost("update_tweet/{id}")]
        public async Task<ActionResult> UpdateTweet([FromQuery] string id, [FromBody] PostsUtenteForm postForm)
        {
            try
            {
                Tweet? tweet = await _context.Tweets.FindAsync(id);

                if (tweet == null)
                {
                    return NotFound("Tweet non trovato");
                }

                tweet.dataCreazione = DateTime.UtcNow;
                tweet.testo = postForm.testo;
                tweet.idUtente = postForm.idUtente;
                tweet.immaginePost = postForm.immaginePost;

                _context.Tweets.Update(tweet);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Tweet aggiunto con successo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex}");
            }
        }

        [HttpDelete("delete_post/{id}")]
        public async Task<ActionResult> DeletePost(int id)
        {
            try
            {
                Tweet? tweet = await _context.Tweets.FindAsync(id);

                if (tweet == null)
                {
                    return NotFound("Tweet non trovato");
                }

                _context.Tweets.Remove(tweet);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Tweet eliminato con successo" });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Impossibile eliminare il tweet per vincoli di integrità referenziale");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        private static Dictionary<string, string>? FormatSocial(JsonElement? socialElement)
        {
            if (socialElement == null || !socialElement.HasValue || socialElement.Value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(socialElement.Value);
            }
            catch
            {
                return null;
            }
        }
    }
}
