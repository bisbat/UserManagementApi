using System.ComponentModel.DataAnnotations;
using UserManagementApi.Middleware;
using UserManagementApi.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();

var users = new List<User>
{
    new() { Id = 1, Name = "Alice", Email = "alice@example.com" },
    new() { Id = 2, Name = "Bob", Email = "bob@example.com" }
};
var nextId = 3;
var sync = new object();

static Dictionary<string, string[]>? Validate(User user)
{
    var results = new List<ValidationResult>();
    if (Validator.TryValidateObject(user, new ValidationContext(user), results, true))
        return null;

    return results
        .GroupBy(r => r.MemberNames.FirstOrDefault() ?? "user")
        .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? "Invalid").ToArray());
}

app.MapGet("/", () => Results.Ok(new { message = "User Management API is running" }));

// READ all
app.MapGet("/users", () =>
{
    lock (sync) { return Results.Ok(users.ToList()); }
});

// READ one
app.MapGet("/users/{id:int}", (int id) =>
{
    lock (sync)
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        return user is null ? Results.NotFound(new { error = "User not found" }) : Results.Ok(user);
    }
});

// CREATE
app.MapPost("/users", (User input) =>
{
    var errors = Validate(input);
    if (errors is not null) return Results.ValidationProblem(errors);

    lock (sync)
    {
        var user = new User { Id = nextId++, Name = input.Name.Trim(), Email = input.Email };
        users.Add(user);
        return Results.Created($"/users/{user.Id}", user);
    }
});

// UPDATE
app.MapPut("/users/{id:int}", (int id, User input) =>
{
    var errors = Validate(input);
    if (errors is not null) return Results.ValidationProblem(errors);

    lock (sync)
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null) return Results.NotFound(new { error = "User not found" });

        user.Name = input.Name.Trim();
        user.Email = input.Email;
        return Results.Ok(user);
    }
});

// DELETE
app.MapDelete("/users/{id:int}", (int id) =>
{
    lock (sync)
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null) return Results.NotFound(new { error = "User not found" });

        users.Remove(user);
        return Results.Ok(new { message = "User deleted", user });
    }
});

app.Run();