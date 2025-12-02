
using EgyWonders.Data;
using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Models;
using EgyWonders.Repository;
using EgyWonders.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace EgyWonders
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // Lockout settings (important for login security)
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<TravelDbContext>()
.AddDefaultTokenProviders();

            // Add JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Policy: "Fixed": 5 requests every 10 seconds per IP
                options.AddFixedWindowLimiter("Fixed", opt =>
                {
                    opt.Window = TimeSpan.FromSeconds(10);
                    opt.PermitLimit = 5;
                    opt.QueueLimit = 2;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
            });
            builder.Services.AddAuthorization();
            // Add services to the container.
            builder.Services.AddDbContext<TravelDbContext>(options =>
                options.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

           
            builder.Services.AddScoped<IUnitOfWork, UnitOfWorkRepository>();
            builder.Services.AddScoped<IAmenityService, AmenityService>();
            builder.Services.AddScoped<IListingService, ListingService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<ITourService, TourService>();
            builder.Services.AddScoped<ITourBookingService, TourBookingService>(); 
            builder.Services.AddScoped<IHostDocumentService, HostDocumentService>();
            builder.Services.AddScoped<IGuideCertificationService, GuideCertificationService>(provider =>
              new GuideCertificationService(
          provider.GetRequiredService<IUnitOfWork>(),
          provider.GetRequiredService<IWebHostEnvironment>()
            )
            ); builder.Services.AddScoped<IReviewService,ReviewService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.Configure<EmailSettingsDTO>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();



            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // This prevents .NET from renaming your claims
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseRateLimiter();
            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
