using FileTransferService.Core.Interfaces;
using FileTransferService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.AddDebug();
    configure.SetMinimumLevel(LogLevel.Information);
});

// Register core services
builder.Services.AddScoped<IFileScanner, FileScanner>();
builder.Services.AddScoped<IFileValidator, FileValidator>();
builder.Services.AddScoped<IFileTransfer, FileTransfer>();
builder.Services.AddScoped<IDestinationResolver, DestinationResolver>();
builder.Services.AddScoped<IFileWatcher, FileWatcher>();
builder.Services.AddScoped<ITransferOrchestrator, TransferOrchestrator>();

// Add controllers
builder.Services.AddControllers();

// Add API exploration and swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
