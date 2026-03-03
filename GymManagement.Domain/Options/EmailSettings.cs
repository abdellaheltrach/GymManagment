namespace GymManagement.Domain.Options
{
    public class EmailSettings
    {
        public int Port { get; init; }
        public string Host { get; init; } = string.Empty;
        public string FromEmail { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
