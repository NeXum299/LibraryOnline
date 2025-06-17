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
        Title = "������ ����������",
        Description = "��� ���������� �������� �� ASP.NET Core WEB API!",
        Contact = new OpenApiContact
        {
            Name = "�������",
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
        logger.LogInformation($"������ �� ���� � ������� {loginModel.Username}");

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

        logger.LogInformation($"��� �������� ���� � ������� {loginModel.Username}!");
        return Results.Ok(new { Token = token });
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��������������! ������: {ex.Message}.");
        return Results.Problem($"������ ��� ��������������.");
    }
}).WithDescription("�������������� ������������.")
.WithOpenApi();

app.Map("/checkdb", [Authorize] async () =>
{
    try
    {
        logger.LogInformation("����������� �������� ����������� � ��.");

        using var connection = new MySqlConnection(builder.Configuration.GetConnectionString("Default"));
        await connection.OpenAsync();
        await connection.CloseAsync();

        logger.LogInformation("����������� ���� ���������.");

        return "����������� � ���� ������ �������.";
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ����������� � ���� ������! ������: {ex.Message}.");
        return $"������ ����������� � ���� ������! ������.";
    }
}).WithDescription("�������� ����������� � ���� ������.")
.WithOpenApi();

app.Map("/contact", () =>
{
    return Results.Text(
        @"���: �������
Email: rodohlebov.dima@gmail.com
�������: +7 (951) 604-33-37",
"text/plain",
        System.Text.Encoding.UTF8
    );
}).WithDescription("������ ������� ���������.")
.WithOpenApi();

// GET
app.MapGet("/api/books", [Authorize] async ([FromServices] IRepositoryAsync<Book> bookRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        var books = await bookRepo.GetModelListAsync();
        var booksDTO = mapper.Map<List<BookDTO>>(books);
        logger.LogInformation("��� ��������� ������ ����.");
        
        return Results.Ok(booksDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ��������� ������ ����: {ex.Message}");
        return Results.Problem($"������ ��� ��������� ������ ����.");
    }
}).CacheOutput(p => p.Expire(TimeSpan.FromSeconds(30)))
.WithDescription("�������� ������ ����.")
.WithOpenApi()
.Produces<List<BookDTO>>();

app.MapGet("/api/readers", [Authorize] async ([FromServices] IRepositoryAsync<Reader> readerRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        var readers = await readerRepo.GetModelListAsync();
        var readersDTO = mapper.Map<List<ReaderDTO>>(readers);
        logger.LogInformation("��� ��������� ������ ���������.");
        return Results.Ok(readersDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ��������� ������ ���������: {ex.Message}");
        return Results.Problem($"������ ��� ��������� ������ ���������.");
    }
}).CacheOutput(p => p.Expire(TimeSpan.FromSeconds(30)))
.WithDescription("�������� ������ ���������.")
.WithOpenApi()
.Produces<List<ReaderDTO>>();

app.MapGet("/api/borrow_records", [Authorize] async ([FromServices] IRepositoryAsync<BorrowRecord> recordRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        var records = await recordRepo.GetModelListAsync();
        var recordsDTO = mapper.Map<List<BorrowRecordDTO>>(records);
        logger.LogInformation("��� ��������� ������ �������.");
        return Results.Ok(recordsDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ��������� ������ �������: {ex.Message}");
        return Results.Problem($"������ ��� ��������� ������ �������.");
    }
}).CacheOutput(p => p.Expire(TimeSpan.FromSeconds(30)))
.WithDescription("�������� ������ �������.")
.WithOpenApi()
.Produces<List<BorrowRecordDTO>>();

app.MapGet("/api/books/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<Book> bookRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID ������ ���� ������������� ������.");


        logger.LogInformation($"����������� ������ �� ��������� ����� ��� id: {id}.");

        Book? book = await bookRepo.GetModelByIdAsync(id);

        if (book != null)
        {
            BookDTO bookDTO = mapper.Map<BookDTO>(book);
            logger.LogInformation("����� ���� ��������.");
            return Results.Ok(bookDTO);
        }

        logger.LogWarning($"����� ��� id: {id} �� ���� �������!");
        return Results.NotFound($"������ ����� ��� id: {id} �� �������!");
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ��������� �����: {ex.Message}");
        return Results.Problem($"������ ��� ��������� �����.");
    }
}).WithDescription("�������� ����� �� ������������ id.")
.WithOpenApi()
.Produces<BookDTO>();

app.MapGet("/api/readers/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<Reader> readerRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID ������ ���� ������������� ������.");


        logger.LogInformation($"��������� ������ �� ��������� �������� ��� id: {id}.");

        Reader? reader = await readerRepo.GetModelByIdAsync(id);

        if (reader != null)
        {
            var readerDTO = mapper.Map<ReaderDTO>(reader);
            logger.LogInformation("�������� ��� �������.");
            return Results.Ok(readerDTO);
        }

        logger.LogWarning($"�������� ��� id: {id} �� ��� ������!");
        return Results.NotFound($"�������� ��� id: {id}. �� ������!");
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ��������� ��������: {ex.Message}");
        return Results.Problem($"������ ��� ��������� ��������.");
    }
}).WithDescription("�������� �������� �� ������������ id.")
.Produces<ReaderDTO>();

app.MapGet("/api/borrow_records/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<BorrowRecord> recordRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID ������ ���� ������������� ������.");

        logger.LogInformation($"��������� ������ �� ��������� ������ ��� id: {id}.");

        BorrowRecord? borrowRecord = await recordRepo.GetModelByIdAsync(id);

        if (borrowRecord != null)
        {
            var borrowRecordDTO = mapper.Map<BorrowRecordDTO>(borrowRecord);
            logger.LogInformation("������ ���� ��������.");
            return Results.Ok(borrowRecordDTO);
        }

        logger.LogWarning($"������ �� ���� ��� id: {id} �� ���� �������!");
        return Results.NotFound($"������ ��� id: {id}. �� �������!");
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ��������� ������: {ex.Message}");
        return Results.Problem($"������ ��� ��������� ������.");
    }
}).WithDescription("�������� ������ �������.")
.WithOpenApi()
.Produces<BorrowRecordDTO>();

// POST
app.MapPost("/api/books", [Authorize] async ([FromBody] BookDTO bookDTO, [FromServices] IRepositoryAsync<Book> bookRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        logger.LogInformation("��������� ������ �� ���������� ����� �����.");

        var book = mapper.Map<Book>(bookDTO);
        await bookRepo.CreateModelAsync(book);

        var createdBookDTO = mapper.Map<BookDTO>(book);

        logger.LogInformation("����� ���������.");
        return Results.Created($"/api/books/{book.Id}", createdBookDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ���������� �����: {ex.Message}");
        return Results.Problem($"������ ��� ���������� �����.");
    }
}).WithDescription("������ �� ���������� �����.")
.WithOpenApi()
.Produces<BookDTO>();

app.MapPost("/api/readers", [Authorize] async ([FromBody] Reader reader, [FromServices] IRepositoryAsync<Reader> readerRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        logger.LogInformation("��������� ������ �� ���������� ������ ��������.");
        await readerRepo.CreateModelAsync(reader);

        var readerDTO = mapper.Map<ReaderDTO>(reader);

        logger.LogInformation("�������� ��������.");
        return Results.Created($"/api/reader/{readerDTO.Id}", readerDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ���������� ��������: {ex.Message}");
        return Results.Problem($"������ ��� ���������� ��������.");
    }
}).WithDescription("������ �� ���������� ��������.")
.WithOpenApi()
.Produces<ReaderDTO>();

app.MapPost("/api/borrow_records", [Authorize] async ([FromBody] BorrowRecord record, [FromServices] IRepositoryAsync<BorrowRecord> recordRepo, [FromServices] IMapper mapper) =>
{
    try
    {
        logger.LogInformation("��������� ������ �� ���������� ����� ������.");
        await recordRepo.CreateModelAsync(record);

        var recordDTO = mapper.Map<BorrowRecordDTO>(record);

        logger.LogInformation("������ ���������.");
        return Results.Created($"/api/borrow_record/{recordDTO.Id}", recordDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ���������� ������: {ex.Message}");
        return Results.Problem($"������ ��� ���������� ������.");
    }
}).WithDescription("������ �� ���������� ������.")
.WithOpenApi()
.Produces<BorrowRecordDTO>();

// PUT
app.MapPut("/api/books/{id:int}", [Authorize] async (int id, [FromBody] Book updatedBook, [FromServices] IRepositoryAsync<Book> bookRepository, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID ������ ���� ������������� ������.");
        if (updatedBook == null)
            return Results.BadRequest("������ ����� �� ����� ���� �������.");
        if (id != updatedBook.Id)
            return Results.BadRequest("ID � ���� � ID ����� �� ���������.");

        logger.LogInformation($"��������� ������ �� ��������� ����� ��� id: {id}.");

        var existingBook = await bookRepository.GetModelByIdAsync(id);
        if (existingBook == null)
        {
            logger.LogWarning($"����� ��� id: {id} �� �������!");
            return Results.NotFound($"����� ��� id: {id} �� �������!");
        }

        existingBook.Title = updatedBook.Title;
        existingBook.Author = updatedBook.Author;
        existingBook.YearPublished = updatedBook.YearPublished;
        existingBook.Genre = updatedBook.Genre;
        existingBook.IsAvailable = updatedBook.IsAvailable;

        await bookRepository.UpdateModelByIdAsync(existingBook);

        var bookDTO = mapper.Map<BookDTO>(existingBook);
        logger.LogInformation($"����� � id: {id} ������� ���������");
        return Results.Ok(bookDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ���������� �����: {ex.Message}");
        return Results.Problem($"������ ��� ���������� �����.");
    }
}).WithDescription("������ �� ��������� ����� �� ������������ id.")
.WithOpenApi()
.Produces<BookDTO>();

app.MapPut("/api/readers/{id:int}", [Authorize] async (int id, [FromBody] Reader updatedReader, [FromServices] IRepositoryAsync<Reader> readerRepository, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID ������ ���� ������������� ������.");
        if (updatedReader == null)
            return Results.BadRequest("������ �������� �� ����� ���� �������.");
        if (id != updatedReader.Id)
            return Results.BadRequest("ID � ���� � ID �������� �� ���������.");

        logger.LogInformation($"��������� ������ �� ��������� �������� ��� id: {id}.");

        var existingReader = await readerRepository.GetModelByIdAsync(id);

        if (existingReader == null)
        {
            logger.LogWarning($"�������� ��� id: {id} �� ������!");
            return Results.NotFound($"�������� ��� id: {id} �� ������!");
        }

        existingReader.Name = updatedReader.Name;
        existingReader.Email = updatedReader.Email;

        await readerRepository.UpdateModelByIdAsync(existingReader);

        var readerDTO = mapper.Map<ReaderDTO>(existingReader);
        logger.LogInformation($"�������� ��� id: {id} ������� �������!");
        return Results.Ok(readerDTO);
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ���������� ��������: {ex.Message}");
        return Results.Problem($"������ ��� ���������� ��������.");
    }
}).WithDescription("������ �� ��������� �������� �� ������������ id.")
.WithOpenApi()
.Produces<ReaderDTO>();

app.MapPut("/api/borrow_records/{id:int}", [Authorize] async (int id, [FromBody] BorrowRecord updatedRecord, [FromServices] IRepositoryAsync<BorrowRecord> recordRepository, [FromServices] IMapper mapper) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID ������ ���� ������������� ������.");

        logger.LogInformation($"��������� ������ �� ��������� ������ ��� id: {id}.");

        BorrowRecord? record = await recordRepository.GetModelByIdAsync(id);

        if (record != null)
        {
            updatedRecord.Id = id;
            await recordRepository.UpdateModelByIdAsync(updatedRecord);

            var recordDTO = mapper.Map<BorrowRecordDTO>(updatedRecord);
            logger.LogInformation("������ ��������.");
            return Results.Ok(recordDTO);
        }

        logger.LogWarning($"������ ��� id: {id} �� �������!");
        return Results.NotFound($"������ � ������ ��� id: {id} �� �������!");
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� ���������� ������: {ex.Message}");
        return Results.Problem($"������ ��� ���������� ������.");
    }
}).WithDescription("������ �� ��������� ������ �� ������������ id.")
.WithOpenApi()
.Produces<BorrowRecordDTO>();

// DELETE
app.MapDelete("/api/books/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<Book> bookRepository) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID ������ ���� ������������� ������.");

        logger.LogInformation($"��������� ������ �� �������� ����� ��� id: {id}.");

        await bookRepository.DeleteModelByIdAsync(id);

        logger.LogInformation("����� �������.");
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� �������� �����: {ex.Message}");
        return Results.Problem($"������ ��� �������� �����.");
    }
}).WithDescription("������ �� �������� ����� �� ������������ id.")
.WithOpenApi();

app.MapDelete("/api/readers/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<Reader> readerRepository) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID ������ ���� ������������� ������.");

        logger.LogInformation($"��������� ������ �� �������� �������� ��� id: {id}.");

        await readerRepository.DeleteModelByIdAsync(id);

        logger.LogInformation("�������� �����.");
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� �������� ��������: {ex.Message}");
        return Results.Problem($"������ ��� �������� ��������.");
    }
}).WithDescription("������ �� �������� �������� ��� ����������� id.")
.WithOpenApi();

app.MapDelete("/api/borrow_records/{id:int}", [Authorize] async (int id, [FromServices] IRepositoryAsync<BorrowRecord> recordRepository) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("ID ������ ���� ������������� ������.");

        logger.LogInformation($"��������� ������ �� �������� ������ ��� id: {id}.");

        await recordRepository.DeleteModelByIdAsync(id);

        logger.LogInformation("������ �������.");
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError($"������ ��� �������� ������: {ex.Message}");
        return Results.Problem($"������ ��� �������� ������.");
    }
}).WithDescription("������ �� �������� ������ ��� ����������� id.")
.WithOpenApi();

app.Run();
