using BLL.Interfaces;
using DAL;
using DTOs.Constants;
using DTOs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User> RegisterAsync(string fullName, string email, string password, string role)
        {

            if (!UserRoles.IsValid(role))
            {
                throw new Exception($"Invalid role. Allowed roles are: {UserRoles.Annotator}, {UserRoles.Reviewer}, {UserRoles.Manager}, {UserRoles.Admin}");
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                throw new Exception("Email already exists.");
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                Role = role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                PaymentInfo = new PaymentInfo()
            };

            user.PaymentInfo.UserId = user.Id;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isValidPassword) return null;

            return GenerateJwtToken(user);
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _context.Users
                .Include(u => u.PaymentInfo)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task UpdatePaymentInfoAsync(string userId, string bankName, string bankAccount, string taxCode)
        {
            var user = await _context.Users
                .Include(u => u.PaymentInfo)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) throw new Exception("User not found");

            if (user.PaymentInfo == null)
            {
                user.PaymentInfo = new PaymentInfo { UserId = userId };
            }

            user.PaymentInfo.BankName = bankName;
            user.PaymentInfo.BankAccountNumber = bankAccount;
            user.PaymentInfo.TaxCode = taxCode;

            await _context.SaveChangesAsync();
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}