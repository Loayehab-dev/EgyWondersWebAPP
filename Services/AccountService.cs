using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static EgyWonders.DTO.RegisterDTO;
using Google.Apis.Auth;

namespace EgyWonders.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService; 
        public AccountService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork uow,
            IConfiguration config,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _uow = uow;
            _config = config;
            _emailService = emailService;
        }


        public async Task<AuthResponseDto> RegisterAsync(RegisterDTO dto)
        {
            // 1. Validation
            var allowedRoles = new List<string> { "Host", "Guest", "TourGuide" };

            if (!allowedRoles.Contains(dto.Role))
            {
                throw new Exception($"Invalid Role. Allowed roles are: {string.Join(", ", allowedRoles)}");
            }

            if (dto.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Admin registration is restricted.");
            }

            // 1. Validation: Check if email exists
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                throw new Exception("Email is already registered.");
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                throw new Exception("Email already registered.");
            if (await _userManager.FindByNameAsync(dto.Username) != null)
                throw new Exception("Username already taken.");

            // 2. Create Identity User
            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Nationality = dto.Nationality,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            // 3. Assign Role
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);

            // 4. Create Business Profile
            var businessUser = new User
            {
                AspNetUserId = user.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Nationality = dto.Nationality,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            try
            {
                _uow.Repository<User>().Add(businessUser);
                await _uow.CompleteAsync();
            }
            catch
            {
                // Rollback if business profile fails
                await _userManager.DeleteAsync(user);
                throw new Exception("Failed to create business profile.");
            }

            // 5.  SEND CONFIRMATION EMAIL 
            //
            //  i didnNOT use try/catch here so you can see the error in Postman if it fails.
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = System.Web.HttpUtility.UrlEncode(token);
            var baseUrl = _config["AppUrl"] ?? "https://localhost:7151";

            var confirmationLink = $"{baseUrl}/api/Auth/confirm-email?userId={user.Id}&token={encodedToken}";

            string emailBody = $@"
                <h3>Welcome to EgyWonders, {dto.FirstName}!</h3>
                 <h4>One step left to start your adventure.</h4>

                <p> Please confirm your account by clicking the link below:</p>
                <a href='{confirmationLink}'>Confirm Email</a>";

            await _emailService.SendEmailAsync(dto.Email, "Confirm your email", emailBody);

            // 6. Return Token
            return await GenerateJwtToken(user, businessUser.UserId);
        }
        public async Task<AuthResponseDto> LoginAsync(LoginDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.UsernameOrEmail)
                       ?? await _userManager.FindByNameAsync(dto.UsernameOrEmail);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new Exception("Invalid credentials.");

            //  Check Email Confirmation
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new Exception("Email not confirmed. Please check your inbox.");
            }

            // Check Status
            if (user.IsActive == false) throw new Exception("Account is inactive.");

            // Check Lockout
            if (await _userManager.IsLockedOutAsync(user))
                throw new Exception("Account locked. Please try again later.");

            // Success logic...
            await _userManager.ResetAccessFailedCountAsync(user);
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var businessUser = (await _uow.Repository<User>().GetAllAsync(u => u.AspNetUserId == user.Id)).FirstOrDefault();
            return await GenerateJwtToken(user, businessUser?.UserId ?? 0);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) throw new Exception("Invalid token.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsActive == false) throw new Exception("User not active.");

            var businessUser = (await _uow.Repository<User>().GetAllAsync(u => u.AspNetUserId == user.Id)).FirstOrDefault();
            return await GenerateJwtToken(user, businessUser?.UserId ?? 0);
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        // --- PROFILE ---

        public async Task<UserProfileDTO> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            var businessUser = (await _uow.Repository<User>().GetAllAsync(u => u.AspNetUserId == userId)).FirstOrDefault();
            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileDTO
            {
                Id = user.Id,
                BusinessUserId = businessUser?.UserId ?? 0,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Nationality = user.Nationality,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Roles = roles,
                IsActive = user.IsActive ?? true
            };
        }

        public async Task UpdateProfileAsync(string userId, UpdateProfileDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var businessUser = (await _uow.Repository<User>().GetAllAsync(u => u.AspNetUserId == userId)).FirstOrDefault();

            if (user == null || businessUser == null) throw new Exception("User not found.");

            // Identity Update
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Phone = dto.Phone;
            user.Nationality = dto.Nationality;
            user.Gender = dto.Gender;
            user.DateOfBirth = dto.DateOfBirth;
            await _userManager.UpdateAsync(user);

            // Business Update
            businessUser.FirstName = dto.FirstName;
            businessUser.LastName = dto.LastName;
            businessUser.Phone = dto.Phone;
            businessUser.Nationality = dto.Nationality;
            businessUser.Gender = dto.Gender;
            businessUser.DateOfBirth = dto.DateOfBirth;

            _uow.Repository<User>().Update(businessUser);
            await _uow.CompleteAsync();
        }


        public async Task DeleteAccountAsync(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            if (!await _userManager.CheckPasswordAsync(user, password))
                throw new Exception("Invalid password.");

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            var businessUser = (await _uow.Repository<User>().GetAllAsync(u => u.AspNetUserId == userId)).FirstOrDefault();
            if (businessUser != null)
            {
                businessUser.IsActive = false;
                _uow.Repository<User>().Update(businessUser);
                await _uow.CompleteAsync();
            }
        }

        // --- ADMIN ---

        public async Task AssignRoleAsync(AssignRoleDTO dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null) throw new Exception("User not found.");

            if (!await _roleManager.RoleExistsAsync(dto.RoleName))
                throw new Exception("Role does not exist.");

            await _userManager.AddToRoleAsync(user, dto.RoleName);
        }

        public async Task RemoveRoleAsync(AssignRoleDTO dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null) throw new Exception("User not found.");

            await _userManager.RemoveFromRoleAsync(user, dto.RoleName);
        }

        public async Task ToggleUserStatusAsync(int businessId, bool isActive)
        {
            var businessUser = await _uow.Repository<User>().GetByIdAsync(businessId);
            if (businessUser == null) throw new Exception("User not found.");

            businessUser.IsActive = isActive;
            _uow.Repository<User>().Update(businessUser);
            await _uow.CompleteAsync();

            if (!string.IsNullOrEmpty(businessUser.AspNetUserId))
            {
                var user = await _userManager.FindByIdAsync(businessUser.AspNetUserId);
                if (user != null)
                {
                    user.IsActive = isActive;
                    await _userManager.UpdateAsync(user);
                }
            }
        }

        public async Task<IEnumerable<UserProfileDTO>> GetAllUsersAsync()
        {
            var users = await _uow.Repository<User>().GetAllAsync();
            return users.Select(u => new UserProfileDTO
            {
                BusinessUserId = u.UserId,
                Id = u.AspNetUserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                IsActive = u.IsActive ?? true
            });
        }

        public async Task<UserProfileDTO> GetUserByIdAsync(int businessId)
        {
            var u = await _uow.Repository<User>().GetByIdAsync(businessId);
            if (u == null) return null;

            var identityUser = await _userManager.FindByIdAsync(u.AspNetUserId);
            var roles = identityUser != null ? await _userManager.GetRolesAsync(identityUser) : new List<string>();

            return new UserProfileDTO
            {
                BusinessUserId = u.UserId,
                Id = u.AspNetUserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Roles = roles,
                IsActive = u.IsActive ?? true
            };
        }

        // HELPERS 
        //  JWT GENERATION 
        private async Task<AuthResponseDto> GenerateJwtToken(ApplicationUser user, int businessId)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("BusinessId", businessId.ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            double minutes = Convert.ToDouble(_config["JwtSettings:AccessTokenExpirationMinutes"] ?? "60");
            var expiry = DateTime.UtcNow.AddMinutes(minutes);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new AuthResponseDto
            {
                Email = user.Email ?? "",
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Roles = roles.ToList(),
                BusinessUserId = businessId,
                ExpiresAt = expiry
            };
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["JwtSettings:Issuer"],
                ValidAudience = _config["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        public async Task<string> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) throw new Exception("Invalid or expired token");

            return "Email confirmed successfully!";
        }

        public async Task<string> ForgotPasswordAsync(string email)
{
    var user = await _userManager.FindByEmailAsync(email);
    if (user == null) return "If email exists, reset link sent.";

    // 1. Generate Token
    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

    // 2. Encode Token (Critical! Tokens contain symbols like '+' that break URLs)
    var encodedToken = System.Web.HttpUtility.UrlEncode(token);
    var encodedEmail = System.Web.HttpUtility.UrlEncode(email);

    // 3. Build Link
    // In production, this points to: "https://your-website.com/reset-password"
    // For now, let's assume your frontend runs on localhost:3000
    var frontendUrl = "http://localhost:3000/reset-password";
    
    var resetLink = $"{frontendUrl}?token={encodedToken}&email={encodedEmail}";

    // 4. Create HTML Body
    string emailBody = $@"
        <h3>Reset Your Password</h3>
        <p>Click the link below to set a new password:</p>
        <a href='{resetLink}'>Reset Password</a>
        <br/>
        <p>Or copy this token if you are testing in Postman:</p>
        <p>{token}</p>";

    // 5. Send
    await _emailService.SendEmailAsync(email, "Reset Password", emailBody);

    return "Password reset link sent to email.";
}

        public async Task<string> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Invalid request.");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return "Password has been reset successfully.";
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(string googleIdToken)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { _config["GoogleAuthSettings:ClientId"] }
                };

                // 1. Verify Token with Google servers
                payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, settings);
            }
            catch
            {
                throw new Exception("Invalid Google Token or ID.");
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                // Google does NOT provide Gender, Nationality, or Phone.
                // We set them to NULL so the User Profile shows them as empty/editable later.

                var registerDto = new RegisterDTO
                {
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "Google User",
                    LastName = payload.FamilyName ?? "Google User",
                    Username = payload.Email.Split('@')[0] + new Random().Next(100, 999),
                    Password = "Google_" + Guid.NewGuid(),
                    Role = "Guest",

                    //  set these to NULL because google never provides them
                    Nationality = null,
                    Gender = null,
                    Phone = null,
                    DateOfBirth = null
                };

                // Note: RegisterAsync will send an email. 
                // For Google Users, we usually want to auto-confirm their email.
                // Let's call a modified register logic or manually confirm them here.

                // A. Call Register
                var response = await RegisterAsync(registerDto);

                // B. AUTO-CONFIRM EMAIL (Since Google verified it already)
                var newUser = await _userManager.FindByEmailAsync(payload.Email);
                if (newUser != null)
                {
                    newUser.EmailConfirmed = true;
                    await _userManager.UpdateAsync(newUser);
                }

                return response;
            }

            // Existing user logic...
            var businessUser = (await _uow.Repository<User>().GetAllAsync(u => u.AspNetUserId == user.Id)).FirstOrDefault();
            return await GenerateJwtToken(user, businessUser?.UserId ?? 0);
        }




    }
}