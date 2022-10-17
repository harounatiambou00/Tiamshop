global using api.Models;

//Services
global using api.Services.EmailService;
global using api.Services.UserService;
global using api.Services.JwtService;
global using api.Services.UserTypeService;
global using api.Services.CityService;
global using api.Services.NeighborhoodService;

//DTOs
global using api.DTOs.UserDTOs;
global using api.DTOs.UserDTOs.Admins;
global using api.DTOs.UserDTOs.Clients;

//Data
global using api.Data.ServiceResponse;



using DbUp;

var builder = WebApplication.CreateBuilder(args);

//Adding cors
builder.Services.AddCors();


//This gets the database connection from the appsettings.json file and creates the database if it doesn't exist.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
EnsureDatabase.For.SqlDatabase(connectionString);

/**
 * Setting up dbup-sqlserver and upgrade
*/
var dbupUpgrader = DeployChanges.To.SqlDatabase(connectionString, null)
                                    .WithScriptsEmbeddedInAssembly(
                                        System.Reflection.Assembly.GetExecutingAssembly()
                                        )
                                    .WithTransaction()
                                    .Build();
// check whether there are any pending SQL Scripts, and using the PerformUpgrade method to do the actual migration.
if (dbupUpgrader.IsUpgradeRequired())
{
    dbupUpgrader.PerformUpgrade();
}


/**
    Adding services scopes.
 */
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserTypeService, UserTypeService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<INeighborhoodService, NeighborhoodService>();




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

//Configuring cors so that our client app can have access to the api
app.UseCors(options => options.WithOrigins(
                                    new[] { "http://localhost:3000",
                                        "https://localhost:3000",
                                        "http://localhost:3001",
                                        "https://localhost:3001",
                                        "http://localhost:3000/admin",
                                        "http://localhost:3000/account/1042"
                                    }
                                )
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials()
);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
