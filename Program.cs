using Hospital_Managemant_System.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<HospitalDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
    try
    {
        if (dbContext.Database.CanConnect())  // Checks database connectivity
        {
            Console.WriteLine("✅ Successfully connected to the database!");
        }
        else
        {
            Console.WriteLine("❌ Failed to connect to the database.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database connection error: {ex.Message}");
    }
}

app.Run();
