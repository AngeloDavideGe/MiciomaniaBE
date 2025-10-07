namespace UserViews
{
    public class UserParams
    {
        public string id { get; set; } = "";
        public string nome { get; set; } = "";
        public string? profilepic { get; set; } = "";
        public string ruolo { get; set; } = "";
    }

    // Caso di un GroupJoin
    // public class JoinUserAdmin
    // {
    //     public User u;
    //     public IEnumerable<Admin> admins;

    //     public JoinUserAdmin(User u, IEnumerable<Admin> admins)
    //     {
    //         this.u = u;
    //         this.admins = admins;
    //     }
    // }
}
