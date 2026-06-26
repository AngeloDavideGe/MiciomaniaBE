using Microsoft.AspNetCore.Mvc;
using PostsViews;
using TweetModels;
using UserModels;
using PostsForms;
using TaskOption;
using Posts.Services;
using AppTask.Services;
using BackGroundName;
using CronModels;

namespace Posts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController
    {
        private readonly PostsService _postsService;
        private readonly AppTaskService _tasks;
        private readonly BackGroundService _backgroundService;

        public PostsController(PostsService postsService, AppTaskService tasks, BackGroundService backgroundService)
        {
            _postsService = postsService;
            _tasks = tasks;
            _backgroundService = backgroundService;
        }

        [HttpGet("get_all_last_posts")]
        public async Task<ActionResult<List<TweetExtend>>> GetAllLastPosts([FromQuery] DateTime time)
        {
            return await _tasks.SingleTask(new SingleTaskOptions<List<TweetExtend>>
            {
                Task = () => _postsService.GetLastPosts(time),
                ErrorMessage = "Errore nel recupero dei ultimi post",
            });
        }

        [HttpGet("get_profilo")]
        public async Task<ActionResult<Profilo>> GetProfiloById([FromQuery] string idUtente)
        {
            return await _tasks.MultiTask(new MultiTaskOptions<User, List<Tweet>, Profilo>
            {
                Task1 = () => _postsService.GetUser(idUtente),
                Task2 = () => _postsService.GetUserTweets(idUtente),
                ResultFactory = (user, tweets) => new Profilo(
                    new UserPost
                    {
                        id = user.id,
                        nome = user.nome,
                        email = user.email,
                        password = user.password,
                        profilePic = user.profilePic,
                        stato = user.stato,
                        provincia = user.provincia,
                        bio = user.bio,
                        telefono = user.telefono,
                        compleanno = user.compleanno,
                        social = _postsService.FormatSocial(user.social),
                    },
                    tweets
                ),
                ErrorMessage = "Errore nel recupero del profilo",
            });
        }

        [HttpPost("post_tweet")]
        public async Task<ActionResult> PostTweet([FromBody] PostsUtenteForm postForm)
        {
            ActionResult result = await _tasks.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _postsService.AggungiTweet(postForm),
                ErrorMessage = "Errore nella creazione del tweet",
                SuccessMessage = "Tweet creato con successo"
            });

            if (result is OkObjectResult)
            {
                _backgroundService.FireAndForget(async sp =>
                {
                    PostsService postsService = sp.GetRequiredService<PostsService>();

                    await postsService.PostUtentiCron(
                        idUtente: postForm.idUtente,
                        azione: $"Ha aggiunto un nuovo post",
                        sezione: SezioneCron.Posts
                    );
                });
            }

            return result;
        }

        [HttpPost("update_tweet/{id}")]
        public async Task<ActionResult> UpdateTweet([FromRoute] int id, [FromBody] PostsUtenteForm postForm)
        {
            return await _tasks.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _postsService.AggiornaTweet(id, postForm),
                ErrorMessage = "Errore nell'aggiornamento del tweet",
                SuccessMessage = "Tweet aggiornato con successo"
            });
        }

        [HttpDelete("delete_post/{id}")]
        public async Task<ActionResult> DeletePost([FromRoute] int id)
        {
            return await _tasks.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _postsService.Elimiweet(id),
                ErrorMessage = "Errore nell'eliminazione del tweet",
                SuccessMessage = "Tweet eliminato con successo"
            });
        }
    }
}
