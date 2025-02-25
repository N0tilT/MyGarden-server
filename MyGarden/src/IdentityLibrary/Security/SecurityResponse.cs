namespace IdentityLibrary.Security
{
    public class SecurityResponse
    {
        public required User User { get; init; }

        public required string Token { get; init; }
    }
}
