using Library;
using Microsoft.AspNetCore.OpenApi;
using LiteDB;

namespace API_Demo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //you could also get it from IConfiguration interface
        var connectionString = "temp.db";

        //add as a singleton - it's a single file with a single access point
        builder.Services.AddSingleton<ILiteDatabase, LiteDatabase>(
        x => new LiteDatabase(connectionString));

        // Add services to the container.
        builder.Services.AddAuthorization();

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

        app.MapGet("/todo", () =>
        {
            List<TodoItem> list = new List<TodoItem>() {
                new TodoItem() { Id = 1, Text = "This is a list of text.", IsComplete = true } ,
                new TodoItem() { Id = 2, Text = "New todo item.", IsComplete = false } ,
                new TodoItem() { Id = 3, Text = "Make toast.", IsComplete = false } ,
                new TodoItem() { Id = 4, Text = "Collect stuff.", IsComplete = true } ,
                new TodoItem() { Id = 5, Text = "Manage things.", IsComplete = true }
                };
            return list == null ? Results.NotFound() : Results.Ok(list);
        })
        .WithName("GetTodo")
        .WithOpenApi();

        app.MapGet("/items", (ILiteDatabase db) =>
        {
            var items = db.GetCollection<TodoItem>("TodoItems").FindAll().ToList();
            return items == null ? Results.NotFound() : Results.Ok(items);
        })
        .WithName("GetItems")
        .WithOpenApi();

        app.Run();
    }
}
