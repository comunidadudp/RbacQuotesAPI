using MongoDB.Driver;
using RbacApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var mongoConn = builder.Configuration.GetConnectionString("QuotesDB") ?? "mongo:db//localhost:27017";
var mongoClient = new MongoClient(mongoConn);
var mongoDb = mongoClient.GetDatabase(builder.Configuration.GetValue<string>("MongoDbName") ?? "quotes");
builder.Services.AddSingleton(mongoDb);

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

SeedData.Execute(app);
app.Run();


