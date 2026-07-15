namespace UserForms
{
    public class UserPostForm
    {
        public string nome { get; set; } = "";
        public string username { get; set; } = "";
        public string email { get; set; } = "";
        public string password { get; set; } = "";
    }

    public class UserRuoloUpdateForm
    {
        public string ruolo { get; set; } = "";
        public List<string> permessi { get; set; } = new List<string>();
    }
}
