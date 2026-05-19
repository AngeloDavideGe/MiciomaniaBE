namespace TaskOption
{
    public class SingleTaskOptions<T>
    {
        public required Func<Task<T>> Task { get; set; }
        public string ErrorMessage { get; set; } = "Errore interno del server";
    }

    public class MultiTaskOptions<T1, T2, TResult>
    {
        public required Func<Task<T1>> Task1 { get; set; }
        public required Func<Task<T2>> Task2 { get; set; }
        public required Func<T1, T2, TResult> ResultFactory { get; set; }
        public string ErrorMessage { get; set; } = "Errore interno del server";
    }

    public class SqlTaskOptions
    {
        public required Func<Task> Sql { get; set; }
        public string SuccessMessage { get; set; } = "Opeazione compleatata";
        public string ErrorMessage { get; set; } = "Errore interno del server";
    }

    public class CacheOptions<T>
    {
        public required Func<Task<T>> Task { get; set; }
        public required string NomeCache { get; set; }
        public required TimeSpan DurataCache { get; set; }
    }
}
