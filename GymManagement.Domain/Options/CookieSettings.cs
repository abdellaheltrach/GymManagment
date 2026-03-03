namespace GymManagement.Domain.Options
{
    public class CookieSettings
    {
        public bool HttpOnly { get; init; }
        public bool Secure { get; init; }
        public string SameSite { get; init; } = string.Empty;
        public int RefreshTokenExpirationTimeInDays { get; init; }
    }
}
