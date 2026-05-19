using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using PostsViews;
using TweetModels;
using UserModels;
using PostsForms;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Posts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : UtilitiesController
    {
        public PostsController(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory,
            IMemoryCache cache
        ) : base(context, contextFactory, cache) { }

        [HttpGet("get_all_last_posts")]
        public async Task<ActionResult<List<TweetExtend>>> GetAllLastPosts([FromQuery] DateTime time)
        {
            return await SingleTask(new SingleTaskOptions<List<TweetExtend>>
            {
                Task = () => _context.Tweets
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
                    .ToListAsync(),
                ErrorMessage = "Errore nel recupero dei ultimi post",
            });
        }

        [HttpGet("get_profilo")]
        public async Task<ActionResult<Profilo>> GetProfiloById([FromQuery] string idUtente)
        {
            using AppDbContext newContext = _contextFactory.CreateDbContext();

            return await MultiTask(new MultiTaskOptions<User?, List<Tweet>, Profilo>
            {
                Task1 = () => _context.Users.FirstOrDefaultAsync((User u) => u.id == idUtente),

                Task2 = () => newContext.Tweets
                    .Where((Tweet t) => t.idUtente == idUtente)
                    .OrderByDescending((Tweet tweet) => tweet.dataCreazione)
                    .Take(20)
                    .ToListAsync(),

                ResultFactory = (user, tweets) => new Profilo(
                    new UserPost
                    {
                        id = user!.id,
                        nome = user.nome,
                        email = user.email,
                        password = user.password,
                        profilePic = user.profilePic,
                        stato = user.stato,
                        provincia = user.provincia,
                        bio = user.bio,
                        telefono = user.telefono,
                        compleanno = user.compleanno,
                        social = FormatSocial(user.social),
                    },
                    tweets
                ),

                ErrorMessage = "Errore nel recupero del profilo",
            });
        }

        [HttpPost("post_tweet")]
        public async Task<ActionResult> PostTweet([FromBody] PostsUtenteForm postForm)
        {
            return await SqlFunc(new SqlTaskOptions
            {
                Sql = async () =>
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
                },

                ErrorMessage = "Errore nella creazione del tweet",
                SuccessMessage = "Tweet creato con successo"
            });
        }

        [HttpPost("update_tweet/{id}")]
        public async Task<ActionResult> UpdateTweet([FromRoute] int id, [FromBody] PostsUtenteForm postForm)
        {
            return await SqlFunc(new SqlTaskOptions
            {
                Sql = async () =>
                {
                    Tweet? tweet = await _context.Tweets.FindAsync(id);

                    if (tweet == null)
                        throw new Exception("Tweet non trovato");

                    tweet.dataCreazione = DateTime.UtcNow;
                    tweet.testo = postForm.testo;
                    tweet.idUtente = postForm.idUtente;
                    tweet.immaginePost = postForm.immaginePost;

                    _context.Tweets.Update(tweet);
                    await _context.SaveChangesAsync();
                },

                ErrorMessage = "Errore nell'aggiornamento del tweet",
                SuccessMessage = "Tweet aggiornato con successo"
            });
        }

        [HttpDelete("delete_post/{id}")]
        public async Task<ActionResult> DeletePost([FromRoute] int id)
        {
            return await SqlFunc(new SqlTaskOptions
            {
                Sql = async () =>
                {
                    Tweet? tweet = await _context.Tweets.FindAsync(id);

                    if (tweet == null)
                        throw new Exception("Tweet non trovato");

                    _context.Tweets.Remove(tweet);
                    await _context.SaveChangesAsync();
                },

                ErrorMessage = "Errore nell'eliminazione del tweet",
                SuccessMessage = "Tweet eliminato con successo"
            });
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
