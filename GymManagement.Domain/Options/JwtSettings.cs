namespace GymManagement.Domain.Options
{
    public class JwtSettings
    {
        public string SecretKey { get; init; } = string.Empty;
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public bool ValidateAudience { get; init; }
        public bool ValidateIssuer { get; init; }
        public bool ValidateLifetime { get; init; }
        public bool ValidateIssuerSigningKey { get; init; }
        public int AccessTokenExpirationTimeInMinutes { get; init; }
    }
}
