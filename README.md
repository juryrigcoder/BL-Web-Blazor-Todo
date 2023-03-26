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
    
