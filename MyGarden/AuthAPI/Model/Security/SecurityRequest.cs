﻿namespace AuthAPI.Model.Security
{
    public class SecurityRequest
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}
