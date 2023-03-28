# BL-Web-Blazor-Todo

The solution is in three parts: 1. A class library to hold the Todo model 2. An set of API endpoint to CRUD Todo objects 3. A frontend to view and create Todo items.

## Introduction

To demonstrate the dotnet CLI is better to see it in use that it is to use it without knowing it. VS code lets us interact at the commandline level where as VS lets us use the CLI through a more abstracted means.

## Project sctructure

A root folder is needed to start with, this is the location for three more folders to hold: 1. a class library project, 2. API project and 3. Frontend project. The root folder is equivalent to a VS solution and it is the resource container that holds all the elements needed for the project, however the compiled result of all three projects can be distributed and run seperatley.

Using the folder structure:

BL-Web-Blazor-Todo ->
  BL-Web-Blazor-Todo
  Blazor-Demo
  Library-Demo
  
## Library project

In VS Code open the folder BL-Web-Blazor-Todo and open a terminal at this location, the type:

     dotnet new classlib Library-Demo

This will create a new folder 'Library-Demo', rename the CS place to TodoItem.cs and add the following code:
```
    using System.Text.Json.Serialization;

    namespace Library;
    public class TodoItem
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        [JsonPropertyName("isComplete")]
        public bool IsComplete { get; set; }
    }
```

This is the class object that describes out Todo items. Next, open a terminal at the library projects location and use the following command to build the project:

    dotnet build

## API project   

In VS Code open the folder BL-Web-Blazor-Todo and open a terminal at this location, the type:

     dotnet new web -o API-Demo --use-program-main

This will create a minimal web API with a main method insertion point. If you intend on using HTTPS then you'll need a dev cert:

    dotnet dev-certs https --trust

To use the class library model in the API project we need to open a terminal in the API-Demo folder and tell dotnet to reference the Library csproj file:

    dotnet add API-Demo/API-Demo.csproj reference ../Library-Demo/Library-Demo.csproj

Or add the following to the API-Demo.csproj file:

```
  <ItemGroup>
    <ProjectReference Include="..\Library-Demo\Library-Demo.csproj" />
  </ItemGroup>
```

To use the API and Library we could simple add a use statement to reference the Library DLL at the head of the program.cs:

```
using Library;
```

Then include a GET method to creating an enumerable of TodoItems and return them as JSON:

```
        app.MapGet("/todo", () =>
        {
            List<TodoItem> list = new List<TodoItem>() {
                new TodoItem() { Text = "This is a list of text.", IsComplete = true } ,
                new TodoItem() { Text = "New todo item.", IsComplete = false } ,
                new TodoItem() { Text = "Make toast.", IsComplete = false } ,
                new TodoItem() { Text = "Collect stuff.", IsComplete = true } ,
                new TodoItem() { Text = "Manage things.", IsComplete = true }
                };
            return list == null ? Results.NotFound() : Results.Ok(list);
        })
        .WithName("GetTodo")
        .WithOpenApi();
```
However, if a static set of objects isn't enough then we need to add a way to create and store new TodoItem objects. One of the fastest ways to do this is to use a single
DLL doctument store like Litedb. So, whilst we are in API-Demo folder open a terminal and register the dependancy:

    dotnet add package LiteDB

And then add the following the the program.cs in the Main method:    

```
        var connectionString = "temp.db";

        //add as a singleton - it's a single file with a single access point
        builder.Services.AddSingleton<ILiteDatabase, LiteDatabase>(
        x => new LiteDatabase(connectionString));
```        

And to add some TodoItem objects to Litedb we can create: 

```
        app.MapGet("/items", (ILiteDatabase db) =>
        {
            //create some items
            List<TodoItem> list = new List<TodoItem>() {
                new TodoItem() { Text = "This is a list of text.", IsComplete = true } ,
                new TodoItem() { Text = "New todo item.", IsComplete = false } ,
                new TodoItem() { Text = "Make toast.", IsComplete = false } ,
                new TodoItem() { Text = "Collect stuff.", IsComplete = true } ,
                new TodoItem() { Text = "Manage things.", IsComplete = true }
            };
            //group insert into database
            db.GetCollection<TodoItem>("TodoItems").InsertBulk(list);
            
            //find all items in db
            var items = db.GetCollection<TodoItem>("TodoItems").FindAll().ToList();
            //get results it items is set and not null
            return items == null ? Results.NotFound() : Results.Ok(items);
        })
        .WithName("GetItems")
        .WithOpenApi();
```

Finally, to insert a single TodoItem object into the database we need:

```       
        app.MapPost("/insert",([Microsoft.AspNetCore.Mvc.FromBody] TodoItem item, ILiteDatabase db) =>{
            var items = db.GetCollection<TodoItem>("TodoItems").Insert(item);
            return Results.Created($"/insert/{item._id}", item);
        })
        .WithName("PostItems")
        .WithOpenApi();
```

## Blazor project 

In VS Code open the folder BL-Web-Blazor-Todo then open a terminal at this location and type:

     dotnet new blazorserver -o Blazor-Demo

This is the front end project, it is the most important part of this whole solution and it based on server side page rendering using the Razor templating framework and
all html requests are server side rendered and delivered to the clients browser. Blazor is the over arching framework and is similar to React, Angular and other HTML
rendering frameworks.

As with all C# console applications the entry point is the Program.cs file and it is here were services, dependacies and initial endpoint are defined.

Lets add a Razor page to the Pages folder and use it to:

- initially populate the page with data from an API call
- create a new item, add it to the existing todo items and post it back to the API where it can be save into a database

Pages -> Items.razor

Directives are identified at the top of a razor file using the @ symbol

```
    @page "/items" //page route
    @using Library //using external DLL reference
    @inject IHttpClientFactory ClientFactory // Dependency Injection of a HTTP client
```

Next comes the body of the html and uses standard HTML tags mixed with C# expressions denoted by the @ :

```
    <div class="flex flex-col p-20 bg-blue-100">
    <div class="flex flex-col space-y-4 max-w-xl">
        <input class="rounded-lg" placeholder="Something todo" @bind-value="newTodo" />
        <button class="rounded-lg text-xl bg-blue-500 text-blue-100 hover:bg-blue-600 duration-300" @onclick="AddTodo">Add item</button>
    </div>
```

Finally, we have a code block:

```
    @code {
        private IEnumerable<TodoItem>? TodoItemList = Array.Empty<TodoItem>();
        public List<TodoItem>? displayTodo = new List<TodoItem>(); 
        private bool getTodoItemsError;
        private bool shouldRender;
        protected override bool ShouldRender() => shouldRender;
        public string? newTodo;
        public async Task AddTodo()
        {
            if (!string.IsNullOrWhiteSpace(newTodo))
            {
                TodoItem ins = new TodoItem { Text = newTodo, IsComplete = false };
                displayTodo?.Add(ins);
                newTodo = string.Empty;
                await InsertItem(ins);
            }
        }
    }
```

To initialise a page with data we use the OnInitializedAsync() method:

```
    protected override async Task OnInitializedAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5154/items");
        var client = ClientFactory.CreateClient();
        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            using var responseStream = await response.Content.ReadAsStreamAsync();
            TodoItemList = await System.Text.Json.JsonSerializer.DeserializeAsync<IEnumerable<TodoItem>>(responseStream);
        }
        else
        {
            getTodoItemsError = true;
        }
        displayTodo = TodoItemList?.ToList<TodoItem>() ?? new List<TodoItem>();
        shouldRender = true;
    }
```

To add an new item we create methods to update and insert:

```
    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(newTodo))
        {
            TodoItem ins = new TodoItem { Text = newTodo, IsComplete = false };
            displayTodo?.Add(ins);
            newTodo = string.Empty;
            await InsertItem(ins);
        }
    }

    public async Task InsertItem(TodoItem postBody){
        var client = ClientFactory.CreateClient();
        using var response = await client.PostAsJsonAsync("http://localhost:5154/insert", postBody);
        shouldRender = true;
    }
```

## Tailwind CSS

Tailwind is a Node.js project that standardised CSS. As this is a demo focused on Blazor we will simple add the Tailwind CLI to our environment path
using the dotnet CLI:

```
dotnet tool install --global tailwindcss.cli
```
After installing Tailwind we need a resource folder to hold config files, this tutorial discribes the process best https://gist.github.com/arkada38/74d2787b2fa092a723ba6892cc3b2ed1

Then we can update the HTML to include Tailwind classes:

```
    <div class="flex flex-col p-20 bg-blue-100">
    <div class="flex flex-col space-y-4 max-w-xl">
        <input class="rounded-lg" placeholder="Something todo" @bind-value="newTodo" />
        <button class="rounded-lg text-xl bg-blue-500 text-blue-100 hover:bg-blue-600 duration-300" @onclick="AddTodo">Add item</button>
    </div>
    <br/>
        <div class="flex flex-col space-y-4">
            @foreach (var Item in displayTodo ?? Enumerable.Empty<TodoItem>())
            {
                <div class="bg-white p-6 rounded-lg shadow-lg max-w-xl">
                    <p class="text-2xl font-bold mb-2 text-gray-800 break-words">@Item.Text</p>
                    <p class="text-gray-700">@Item.IsComplete</p>
                    <p class="text-gray-500">@Item._id</p>
                </div>
            }
        </div>
    </div>
```