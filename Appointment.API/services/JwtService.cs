using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Appointment.API.Services
{
    public class JwtService
    {
        private readonly string secretKey = "THIS_IS_MY_SECRET_KEY_12345_FOR_HS256";

        public string GenerateToken(string email, string? role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, string.IsNullOrWhiteSpace(role) ? "User" : role)
            };

            var token = new JwtSecurityToken(
                issuer: "AppointmentAPI",
                audience: "AppointmentAPI",
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}