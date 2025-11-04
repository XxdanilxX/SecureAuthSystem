namespace SecureAuthSystem.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // ADMIN, W, A, R

        public override string ToString() => $"{Username} {Password} {Role}";
    }
}
