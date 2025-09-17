using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddCustomOptions(builder.Configuration)
    .AddBearerAuthenticationScheme(builder.Configuration)
    .AddMongoDatabase(builder.Configuration)
    .AddInfrastructure()
    .AddApplicationServices();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

SeedData.Execute(app);
app.Run();


