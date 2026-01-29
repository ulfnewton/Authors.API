using AuthorsAPI;
using AuthorsAPI.Endpoints;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthorDbContext>(
    options => options.UseInMemoryDatabase("authors")
    );

// Add services to the container.
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

app.Urls.Add("https://localhost:5001");

app.UseHttpsRedirection();

app.MapPost("/authors", AuthorEndpoints.CreateAuthor);
app.MapGet("/authors", AuthorEndpoints.GetAuthors);
app.MapGet("/authors/{id:guid}", AuthorEndpoints.GetAuthor);
app.MapPost("/authors/{id:guid}/books", AuthorEndpoints.CreateBook);

app.Run();
