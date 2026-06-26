using System.Text.Json;
using CronModels;
using Data.ApplicationDbContext;
using Library.Extensions;
using Microsoft.EntityFrameworkCore;
using PostsForms;
using PostsViews;
using TweetModels;
using UserModels;

namespace Posts.Services
{
    public class PostsService
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public PostsService(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory)
        {
            _context = context;
            _contextFactory = contextFactory;
        }

        public async Task<User> GetUser(string idUtente)
        {
            await using var context = _contextFactory.CreateDbContext();

            return await context.Users
                .FirstOrDefaultAsync((User u) => u.id == idUtente)
                ?? throw new Exception("Utente non trovato");
        }

        public async Task<List<Tweet>> GetUserTweets(string idUtente)
        {
            await using var context = _contextFactory.CreateDbContext();

            return await context.Tweets
                .Where((Tweet t) => t.idUtente == idUtente)
                .OrderByDescending((Tweet t) => t.dataCreazione)
                .Take(20)
                .ToListAsync();
        }

        public Task<List<TweetExtend>> GetLastPosts(DateTime time)
        {
            return _context.Tweets
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
        }

        public Task AggungiTweet(PostsUtenteForm postForm)
        {
            Tweet newTweet = new Tweet
            {
                dataCreazione = DateTime.UtcNow,
                testo = postForm.testo,
                idUtente = postForm.idUtente,
                immaginePost = postForm.immaginePost
            };
            _context.Tweets.Add(newTweet);
            return _context.SaveChangesAsync();
        }

        public async Task AggiornaTweet(int id, PostsUtenteForm postForm)
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
        }

        public async Task Elimiweet(int id)
        {
            Tweet? tweet = await _context.Tweets.FindAsync(id);

            if (tweet == null)
                throw new Exception("Tweet non trovato");

            _context.Tweets.Remove(tweet);
            await _context.SaveChangesAsync();
        }

        public async Task PostUtentiCron(string idUtente, string azione, SezioneCron sezione)
        {
            string sezioneString = sezione.GetNameEnum();

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"
                    INSERT INTO crono_schema.cron_utenti 
                    (""idUtente"", azione, sezione, created_at)
                    VALUES 
                    ({idUtente}, {azione}, {sezioneString}, {DateTime.UtcNow})
                "
            );
        }

        // -----------------------------------------------------------------------
        public Dictionary<string, string>? FormatSocial(JsonElement? socialElement)
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