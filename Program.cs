using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using ToDoApi;

var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ToDoDbContext>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "My ToDo API"        
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                      });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});

app.UseCors(MyAllowSpecificOrigins);

app.MapGet("/", () => "Hello!");

app.MapGet("/items",  (ToDoDbContext db) => db.Items.ToList());

app.MapPost("/items", async(ToDoDbContext db,Item item)=>{
    db.Add(item);
    await db.SaveChangesAsync();
    return item;
});

app.MapPut("/items/{id}", async(ToDoDbContext db, [FromBody]Item item, int id)=>{
    var existItem = await db.Items.FindAsync(id);
    if(existItem is null) return Results.NotFound();

    existItem.Name = item.Name;
    existItem.IsComplete = item.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/items/{id}",async (ToDoDbContext db, int id) =>
{
    var existItem = await db.Items.FindAsync(id);
    if (existItem is null) return Results.NotFound();

    db.Items.Remove(existItem);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();