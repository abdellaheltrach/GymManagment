namespace GymManagement.Domain.Options
{
    public class EmailSettings
    {
        public string Host { get; init; } = string.Empty;
        public int Port { get; init; } = 587;
        public bool UseSsl { get; init; } = true;
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string SenderName { get; init; } = "GymManagement";
        public string SenderEmail { get; init; } = string.Empty;
    }
}
