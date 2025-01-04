using Chime_ASPNET.Data;
using Chime_ASPNET.Models.Config;
using Chime_ASPNET.Services.Auth;
using Chime_ASPNET.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
}
