using System.Text.Json;

namespace UserModels
{
    public class User
    {
        public string id { get; set; } = "";
        public string nome { get; set; } = "";
        public string email { get; set; } = "";
        public string password { get; set; } = "";
        public string? profilePic { get; set; } = "";
        public string? provincia { get; set; } = "";
        public string? bio { get; set; } = "";
        public DateTime? compleanno { get; set; }
        public JsonElement? social { get; set; }
    }
}
