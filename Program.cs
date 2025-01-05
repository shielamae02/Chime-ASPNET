using System.Text;
using Newtonsoft.Json;
using Chime_ASPNET.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Chime_ASPNET.Services.Auth;
using Chime_ASPNET.Models.Config;
using Chime_ASPNET.Services.Email;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Chime_ASPNET.Repositories.Auth;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Automapper Configuration
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
#endregion


ConfigureServices(builder.Services, builder.Configuration, builder.Environment);
var app = builder.Build();


#region Automatic Database Migration
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    context.Database.Migrate();
}
#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
{
    #region API Versioning
    services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
    });
    #endregion

    #region SQL Server Configuration 
    services.AddDbContext<DataContext>(options =>
    {
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
    });
    #endregion

    #region Authentication Configuration 
    var isDevelopment = environment.IsDevelopment();
    var jwt = configuration.GetSection("JWT");
    var key = jwt["Key"];

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !isDevelopment,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = !isDevelopment,
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
    });
    #endregion

    #region CORs Configuration 
    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });
    #endregion

    #region Swagger Documentation 
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Chime ASP.NET API",
            Version = "v1",
            Description = "Chime is a simple API for sharing updates and connecting with others.",
            Contact = new OpenApiContact
            {
                Name = "Shiela Mae Lepon",
                Email = "shiela.mlepon@gmail.com"
            }
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header using the Bearer scheme.",
            Scheme = "Bearer",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }, []
            }
        });
    });
    #endregion

    #region Validation Configuration    
    services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
    #endregion

    #region JWT Data Binding 
    services.Configure<JWTSettings>(configuration.GetSection("JWT"));
    services.AddSingleton(resolver =>
        resolver.GetRequiredService<IOptions<JWTSettings>>().Value);
    #endregion

    #region Applicaton Data Binding 
    services.Configure<AppSettings>(configuration.GetSection("Application"));
    services.AddSingleton(resolver =>
        resolver.GetRequiredService<IOptions<AppSettings>>().Value);
    #endregion

    #region SMTP Data Binding 
    services.Configure<SMTPSettings>(configuration.GetSection("SMTP"));
    services.AddSingleton(resolver =>
        resolver.GetRequiredService<IOptions<SMTPSettings>>().Value);
    #endregion

    #region Logging Configuration 
    services.AddLogging();
    #endregion

    #region Background Services Configuration 
    services.AddHostedService<AuthBackgroundService>();
    services.AddHostedService<EmailBackgroundService>();

    services.AddSingleton<EmailQueue>();
    #endregion

    #region Services Configuration
    services.AddScoped<IAuthService, AuthService>();

    services.AddSingleton<IEmailService, EmailService>();
    #endregion

    #region Repositories Configuration
    services.AddScoped<IAuthRepository, AuthRepository>();
    #endregion
}