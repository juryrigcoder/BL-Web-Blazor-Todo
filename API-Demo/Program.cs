using Library;
using Microsoft.AspNetCore.OpenApi;

namespace API_Demo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        app.MapGet("/todo", (HttpContext httpContext) =>
        {
            List<TodoItem> list = new List<TodoItem>() { 
                new TodoItem() { Id = 1, Text = "This is a list of text.", IsComplete = true } ,
                new TodoItem() { Id = 2, Text = "New todo item.", IsComplete = false } ,
                new TodoItem() { Id = 3, Text = "Make toast.", IsComplete = false } ,
                new TodoItem() { Id = 4, Text = "Collect stuff.", IsComplete = true } ,
                new TodoItem() { Id = 5, Text = "Manage things.", IsComplete = true } 
                };
            return list;

        })
        .WithName("GetTodo")
        .WithOpenApi();

        app.Run();
    }
}
