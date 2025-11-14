
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HRMCyberse.Data;
using HRMCyberse.Services;

namespace HRMCyberse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            
            // Configure response compression for better performance
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
                options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/json", "text/json" });
            });

            // Configure Entity Framework with performance optimizations
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); // Force UTC timestamps
            
            builder.Services.AddDbContext<CybersehrmContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(30); // 30 second timeout
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                });
                
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
                else
                {
                    // Production optimizations
                    options.EnableServiceProviderCaching();
                    options.EnableSensitiveDataLogging(false);
                }
            });

            // Register services with performance optimizations
            builder.Services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1000; // Limit cache entries
                options.CompactionPercentage = 0.25; // Compact when 75% full
                options.TrackStatistics = builder.Environment.IsDevelopment(); // Track stats in dev
            });
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IShiftService, ShiftService>();
            builder.Services.AddScoped<IAuditLogService, AuditLogService>();
            builder.Services.AddScoped<IAttendanceService, AttendanceService>();
            builder.Services.AddScoped<IRequestService, RequestService>();
            builder.Services.AddScoped<IPayrollService, PayrollService>();

            // Configure JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = Encoding.ASCII.GetBytes(secretKey!);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddControllers();
            
            // Configure Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "HRM Cyberse API",
                    Version = "v1",
                    Description = @"Complete HRM System API with 5 main features:
                    
1. **Authentication & User Management** - Login, Register, User CRUD
2. **Shift Management** - Create shifts, Assign shifts to employees
3. **Attendance Management** - Check-in/Check-out with GPS & Photos
4. **Request Management** - Leave requests, Shift change, Late arrival requests
5. **Payroll & Rewards** - Salary calculation, Rewards & Penalties",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "HRM Cyberse Team",
                        Email = "support@hrmcyberse.com"
                    }
                });

                // Group APIs by tags
                c.TagActionsBy(api =>
                {
                    if (api.GroupName != null)
                        return new[] { api.GroupName };

                    var controllerName = api.ActionDescriptor.RouteValues["controller"];
                    return controllerName switch
                    {
                        "Auth" => new[] { "1. Authentication" },
                        "Users" => new[] { "1. Authentication" },
                        "Shifts" => new[] { "2. Shift Management" },
                        "Attendance" => new[] { "3. Attendance Management" },
                        "Requests" or "ShiftRequests" or "LateRequests" => new[] { "4. Request Management" },
                        "Payroll" or "RewardPenalty" => new[] { "5. Payroll & Rewards" },
                        _ => new[] { controllerName ?? "Other" }
                    };
                });
                c.DocInclusionPredicate((name, api) => true);

                // Include XML documentation
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Configure JWT Bearer authentication in Swagger
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRM Cyberse API v1");
                    c.RoutePrefix = "swagger";
                    c.DocumentTitle = "HRM Cyberse API Documentation";
                });
            }

            app.UseHttpsRedirection();

            // Enable response compression
            app.UseResponseCompression();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
