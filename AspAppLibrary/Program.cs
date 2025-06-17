using AspAppLibrary;
using AspAppLibrary.DTO;
using AspAppLibrary.AuthLogic;
using AspAppLibrary.EF;
using AspAppLibrary.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddMySqlDataSource(builder.Configuration.GetConnectionString("Default")!);

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        new MySqlServerVersion(new Version(8, 0, 42))
    ));

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IRepositoryAsync<Book>, GenericRepository<Book>>();
builder.Services.AddScoped<IRepositoryAsync<Reader>, GenericRepository<Reader>>();
builder.Services.AddScoped<IRepositoryAsync<BorrowRecord>, GenericRepository<BorrowRecord>>();

ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = loggerFactory.CreateLogger<Program>();

builder.Services.AddMvc(options => options.EnableEndpointRouting = false);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Онлайн библиотека",
        Description = "Эта библиотека написана на ASP.NET Core WEB API!",
        Contact = new OpenApiContact
        {
            Name = "Дмитрий",
            Url = new Uri("https://localhost:7058/contact")
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    var libraryXmlPath = Path.Combine(AppContext.BaseDirectory, "AspAppLibrary.xml");
    options.IncludeXmlComments(libraryXmlPath);
});
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});

app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "/{controller=Home}/{action=Index}/{id?}");
});

app.UseResponseCaching();

app.MapPost("/login", (LoginModel loginModel) =>
{
    try
    {
        logger.LogInformation($"Запрос на вход в аккаунт {loginModel.Username}");

        var model = AuthRepository.GetLoginModel(loginModel.Username, loginModel.Password);

        if (model is null)
            return Results.Unauthorized();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.Username)
         };

        var jwt = new JwtSecurityToken(
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)),
        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
        SecurityAlgorithms.HmacSha256));

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        logger.LogInformation($"Был выполнен вход в аккаунт {loginModel.Username}!");
        return Results.Ok(new { Token = token });
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка аутентификации! Ошибка: {ex.Message}.");
        return Results.Problem($"Ошибка при аутентификации.");
    }
}).WithDescription("Аутентификация пользователя.")
.WithOpenApi();

app.Map("/checkdb", [Authorize] async () =>
{
    try
    {
        logger.LogInformation("Выполняется проверка подключения к БД.");

        using var connection = new MySqlConnection(builder.Configuration.GetConnectionString("Default"));
        await connection.OpenAsync();
        await connection.CloseAsync();

        logger.LogInformation("Подключение было выполнено.");

        return "Подключение к базе данных успешно.";
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка подключения к базе данных! Ошибка: {ex.Message}.");
        return $"Ошибка подключения к базе данных! Ошибка.";
    }
}).WithDescription("Проверка подключения к базе данных.")
.WithOpenApi();

app.Map("/contact", () =>
{
    return Results.Text(
        @"Имя: Дмитрий
Email: rodohlebov.dima@gmail.com
Телефон: +7 (951) 604-33-37",
"text/plain",
        System.Text.Encoding.UTF8
    );
}).WithDescription("Узнать контакт создателя.")
.WithOpenApi();

// GET
app.MapGet("/api/books", [Authorize] async ([FromServices] IRepositoryAsync<Book> bookRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        var books = await bookRepo.GetModelListAsync();
        var booksDTO = mapper.Map<List<BookDTO>>(books);
        logger.LogInformation("Был отправлен список книг.");
        
        return Results.Ok(booksDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при получении списка книг: {ex.Message}");
        return Results.Problem($"Ошибка при получении списка книг.");
    }
}).CacheOutput(p => p.Expire(TimeSpan.FromSeconds(30)))
.WithDescription("Получаем список книг.")
.WithOpenApi()
.Produces<List<BookDTO>>();

app.MapGet("/api/readers", [Authorize] async ([FromServices] IRepositoryAsync<Reader> readerRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        var readers = await readerRepo.GetModelListAsync();
        var readersDTO = mapper.Map<List<ReaderDTO>>(readers);
        logger.LogInformation("Был отправлен список читателей.");
        return Results.Ok(readersDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при получении списка читателей: {ex.Message}");
        return Results.Problem($"Ошибка при получении списка читателей.");
    }
}).CacheOutput(p => p.Expire(TimeSpan.FromSeconds(30)))
.WithDescription("Получаем список читателей.")
.WithOpenApi()
.Produces<List<ReaderDTO>>();

app.MapGet("/api/borrow_records", [Authorize] async ([FromServices] IRepositoryAsync<BorrowRecord> recordRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        var records = await recordRepo.GetModelListAsync();
        var recordsDTO = mapper.Map<List<BorrowRecordDTO>>(records);
        logger.LogInformation("Был отправлен список записей.");
        return Results.Ok(recordsDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при получении списка записей: {ex.Message}");
        return Results.Problem($"Ошибка при получении списка записей.");
    }
}).CacheOutput(p => p.Expire(TimeSpan.FromSeconds(30)))
.WithDescription("Получаем список записей.")
.WithOpenApi()
.Produces<List<BorrowRecordDTO>>();

app.MapGet("/api/books/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<Book> bookRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID должен быть положительным числом.");


        logger.LogInformation($"Выполняется запрос на получение книги под id: {id}.");

        Book? book = await bookRepo.GetModelByIdAsync(id);

        if (book != null)
        {
            BookDTO bookDTO = mapper.Map<BookDTO>(book);
            logger.LogInformation("Книга была получена.");
            return Results.Ok(bookDTO);
        }

        logger.LogWarning($"Книга под id: {id} не была найдена!");
        return Results.NotFound($"Данная книга под id: {id} не найдена!");
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при получении книги: {ex.Message}");
        return Results.Problem($"Ошибка при получении книги.");
    }
}).WithDescription("Получаем книгу по определённому id.")
.WithOpenApi()
.Produces<BookDTO>();

app.MapGet("/api/readers/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<Reader> readerRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID должен быть положительным числом.");


        logger.LogInformation($"Отправлен запрос на получение читателя под id: {id}.");

        Reader? reader = await readerRepo.GetModelByIdAsync(id);

        if (reader != null)
        {
            var readerDTO = mapper.Map<ReaderDTO>(reader);
            logger.LogInformation("Читатель был получен.");
            return Results.Ok(readerDTO);
        }

        logger.LogWarning($"Читатель под id: {id} не был найден!");
        return Results.NotFound($"Читатель под id: {id}. не найден!");
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при получении читателя: {ex.Message}");
        return Results.Problem($"Ошибка при получении читателя.");
    }
}).WithDescription("Получаем читателя по определённому id.")
.Produces<ReaderDTO>();

app.MapGet("/api/borrow_records/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<BorrowRecord> recordRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID должен быть положительным числом.");

        logger.LogInformation($"Отправлен запрос на получение записи под id: {id}.");

        BorrowRecord? borrowRecord = await recordRepo.GetModelByIdAsync(id);

        if (borrowRecord != null)
        {
            var borrowRecordDTO = mapper.Map<BorrowRecordDTO>(borrowRecord);
            logger.LogInformation("Запись была получена.");
            return Results.Ok(borrowRecordDTO);
        }

        logger.LogWarning($"Запись не была под id: {id} не была найдена!");
        return Results.NotFound($"Запись под id: {id}. не найдена!");
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при получении записи: {ex.Message}");
        return Results.Problem($"Ошибка при получении записи.");
    }
}).WithDescription("Получаем список записей.")
.WithOpenApi()
.Produces<BorrowRecordDTO>();

// POST
app.MapPost("/api/books", [Authorize] async ([FromBody] BookDTO bookDTO, [FromServices] IRepositoryAsync<Book> bookRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        logger.LogInformation("Отправлен запрос на добавление новой книги.");

        var book = mapper.Map<Book>(bookDTO);
        await bookRepo.CreateModelAsync(book);

        var createdBookDTO = mapper.Map<BookDTO>(book);

        logger.LogInformation("Книга добавлена.");
        return Results.Created($"/api/books/{book.Id}", createdBookDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при добавлении книги: {ex.Message}");
        return Results.Problem($"Ошибка при добавлении книги.");
    }
}).WithDescription("Запрос на добавление книги.")
.WithOpenApi()
.Produces<BookDTO>();

app.MapPost("/api/readers", [Authorize] async ([FromBody] Reader reader, [FromServices] IRepositoryAsync<Reader> readerRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        logger.LogInformation("Отправлен запрос на добавление нового читателя.");
        await readerRepo.CreateModelAsync(reader);

        var readerDTO = mapper.Map<ReaderDTO>(reader);

        logger.LogInformation("Читатель добавлен.");
        return Results.Created($"/api/reader/{readerDTO.Id}", readerDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при добавлении читателя: {ex.Message}");
        return Results.Problem($"Ошибка при добавлении читателя.");
    }
}).WithDescription("Запрос на добавление читателя.")
.WithOpenApi()
.Produces<ReaderDTO>();

app.MapPost("/api/borrow_records", [Authorize] async ([FromBody] BorrowRecord record, [FromServices] IRepositoryAsync<BorrowRecord> recordRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        logger.LogInformation("Отправлен запрос на добавление новой записи.");
        await recordRepo.CreateModelAsync(record);

        var recordDTO = mapper.Map<BorrowRecordDTO>(record);

        logger.LogInformation("Запись добавлена.");
        return Results.Created($"/api/borrow_record/{recordDTO.Id}", recordDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при добавлении записи: {ex.Message}");
        return Results.Problem($"Ошибка при добавлении записи.");
    }
}).WithDescription("Запрос на добавление записи.")
.WithOpenApi()
.Produces<BorrowRecordDTO>();

// PUT
app.MapPut("/api/books/{id:int}", [Authorize] async (int id, [FromBody] Book updatedBook, [FromServices] IRepositoryAsync<Book> bookRepository, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID должен быть положительным числом.");
        if (updatedBook == null)
            return Results.BadRequest("Данные книги не могут быть пустыми.");
        if (id != updatedBook.Id)
            return Results.BadRequest("ID в пути и ID книги не совпадают.");

        logger.LogInformation($"Отправлен запрос на изменение книги под id: {id}.");

        var existingBook = await bookRepository.GetModelByIdAsync(id);
        if (existingBook == null)
        {
            logger.LogWarning($"Книга под id: {id} не найдена!");
            return Results.NotFound($"Книга под id: {id} не найдена!");
        }

        existingBook.Title = updatedBook.Title;
        existingBook.Author = updatedBook.Author;
        existingBook.YearPublished = updatedBook.YearPublished;
        existingBook.Genre = updatedBook.Genre;
        existingBook.IsAvailable = updatedBook.IsAvailable;

        await bookRepository.UpdateModelByIdAsync(existingBook);

        var bookDTO = mapper.Map<BookDTO>(existingBook);
        logger.LogInformation($"Книга с id: {id} успешно обновлена");
        return Results.Ok(bookDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при обновлении книги: {ex.Message}");
        return Results.Problem($"Ошибка при обновлении книги.");
    }
}).WithDescription("Запрос на изменение книги по определённому id.")
.WithOpenApi()
.Produces<BookDTO>();

app.MapPut("/api/readers/{id:int}", [Authorize] async (int id, [FromBody] Reader updatedReader, [FromServices] IRepositoryAsync<Reader> readerRepository, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID должен быть положительным числом.");
        if (updatedReader == null)
            return Results.BadRequest("Данные читателя не могут быть пустыми.");
        if (id != updatedReader.Id)
            return Results.BadRequest("ID в пути и ID читателя не совпадают.");

        logger.LogInformation($"Отправлен запрос на изменение читателя под id: {id}.");

        var existingReader = await readerRepository.GetModelByIdAsync(id);

        if (existingReader == null)
        {
            logger.LogWarning($"Читатель под id: {id} не найден!");
            return Results.NotFound($"Читатель под id: {id} не найден!");
        }

        existingReader.Name = updatedReader.Name;
        existingReader.Email = updatedReader.Email;

        await readerRepository.UpdateModelByIdAsync(existingReader);

        var readerDTO = mapper.Map<ReaderDTO>(existingReader);
        logger.LogInformation($"Читатель под id: {id} успешно обновлён!");
        return Results.Ok(readerDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при обновлении читателя: {ex.Message}");
        return Results.Problem($"Ошибка при обновлении читателя.");
    }
}).WithDescription("Запрос на изменение читателя по определённому id.")
.WithOpenApi()
.Produces<ReaderDTO>();

app.MapPut("/api/borrow_records/{id:int}", [Authorize] async (int id, [FromBody] BorrowRecord updatedRecord, [FromServices] IRepositoryAsync<BorrowRecord> recordRepository, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID должен быть положительным числом.");

        logger.LogInformation($"Отправлен запрос на изменение записи под id: {id}.");

        BorrowRecord? record = await recordRepository.GetModelByIdAsync(id);

        if (record != null)
        {
            updatedRecord.Id = id;
            await recordRepository.UpdateModelByIdAsync(updatedRecord);

            var recordDTO = mapper.Map<BorrowRecordDTO>(updatedRecord);
            logger.LogInformation("Запись изменена.");
            return Results.Ok(recordDTO);
        }

        logger.LogWarning($"Запись под id: {id} не найдена!");
        return Results.NotFound($"Запись о выдаче под id: {id} не найдена!");
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при обновлении записи: {ex.Message}");
        return Results.Problem($"Ошибка при обновлении записи.");
    }
}).WithDescription("Запрос на изменение записи по определённому id.")
.WithOpenApi()
.Produces<BorrowRecordDTO>();

// DELETE
app.MapDelete("/api/books/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<Book> bookRepository) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID должен быть положительным числом.");

        logger.LogInformation($"Отправлен запрос на удаление книги под id: {id}.");

        await bookRepository.DeleteModelByIdAsync(id);

        logger.LogInformation("Книга удалена.");
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при удалении книги: {ex.Message}");
        return Results.Problem($"Ошибка при удалении книги.");
    }
}).WithDescription("Запрос на удаление книги по определённому id.")
.WithOpenApi();

app.MapDelete("/api/readers/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<Reader> readerRepository) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID должен быть положительным числом.");

        logger.LogInformation($"Отправлен запрос на удаление читателя под id: {id}.");

        await readerRepository.DeleteModelByIdAsync(id);

        logger.LogInformation("Читатель удалён.");
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при удалении читателя: {ex.Message}");
        return Results.Problem($"Ошибка при удалении читателя.");
    }
}).WithDescription("Запрос на удаление читателя под определённым id.")
.WithOpenApi();

app.MapDelete("/api/borrow_records/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<BorrowRecord> recordRepository) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID должен быть положительным числом.");

        logger.LogInformation($"Отправлен запрос на удаление записи под id: {id}.");

        await recordRepository.DeleteModelByIdAsync(id);

        logger.LogInformation("Запись удалена.");
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при удалении записи: {ex.Message}");
        return Results.Problem($"Ошибка при удалении записи.");
    }
}).WithDescription("Запрос на удаление записи под определённым id.")
.WithOpenApi();

app.Run();
