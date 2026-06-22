using SmartAssistant.Application.Documents.Commands.UploadDocument;
using SmartAssistant.Infrastructure;
using SmartAssistant.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. اضافه کردن سرویس CORS برای اجازه دادن به درخواست‌های کلاینت
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175", "http://localhost:3000") // پورت‌های احتمالی React/Vite
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UploadDocumentHandler).Assembly));
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// 2. فعال‌سازی میدل‌ور CORS (حتما باید قبل از UseAuthorization و MapControllers باشد)
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();