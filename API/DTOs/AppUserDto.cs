namespace API.DTOs
{
    public class AppUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? KnownAs { get; set; }
        public string? Gender { get; set; }
    }
}
