var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// In-memory "database"
// Just a List, not a real DB. Satisfies the purpose of this demo.
var books = new List<Book>
{
    new Book(1, "The Count of Monte Cristo", "Alexander Dumas", 1846),
    new Book(2, "Dune", "Frank Herbert", 1965),
};
var nextId = 3;

// GET /api/books -- gets all books
app.MapGet("/api/books", () => Results.Ok(books));

// GET /api/books/{id} -- gets a single book by ID
app.MapGet("/api/books/{id}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    return book is not null ? Results.Ok(book) : Results.NotFound();
}).WithName("GetBook");

// POST /api/books -- create a book
app.MapPost("/api/books", (BookWriteDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest("Title is required.");
    
    var book = new Book(nextId++, dto.Title, dto.Author, dto.YearPublished);
    books.Add(book);
    return Results.CreatedAtRoute("GetBook", new { id = book.Id }, book);
});

// PUT /api/books/{id} -- full update
app.MapPut("/api/books/{id}", (int id, BookWriteDto dto) =>
{
    var index = books.FindIndex(b => b.Id == id);
    if (index == -1) return Results.NotFound();

    books[index] = new Book(id, dto.Title, dto.Author, dto.YearPublished);
    return Results.NoContent();
});

// DELETE /api/books/{id}
app.MapDelete("/api/books/{id}", (int id) =>
{
    var removed = books.RemoveAll(b => b.Id == id);
    return removed > 0 ? Results.NoContent() : Results.NotFound();
});

app.Run();


// Book is the full internal model: everything the server knows about a book
record Book(int Id, string Title, string Author, int YearPublished);

// BookWriteDto is what a client is allowed to send. Client is not allowed to 
// assign an ID to a book. The server handles that.
record BookWriteDto(string Title, string Author, int YearPublished);


