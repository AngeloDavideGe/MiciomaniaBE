namespace BackGroundName
{
    public class BackGroundService(
    IServiceProvider serviceProvider,
    ILogger<BackGroundService> logger)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<BackGroundService> _logger = logger;


        public void FireAndForget(Func<IServiceProvider, Task> action)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    await action(scope.ServiceProvider);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore nell'operazione background");
                }
            });
        }
    }
}
