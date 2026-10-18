using PricingService.Domain;
using PricingService.Infrastructure;
using Steeltoe.Discovery.Client;

var builder = WebApplication.CreateBuilder(args);

// Controllers (dùng Newtonsoft vì QuestionAnswer có JsonConverter của Newtonsoft)
builder.Services
    .AddControllers(options => options.Filters.Add<ValidationExceptionFilter>())
    .AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDataStore, InMemoryDataStore>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Task #2 (Lab 5): đăng ký PricingService vào Eureka
builder.Services.AddDiscoveryClient(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Cho phép project IntegrationTest truy cập Program
public partial class Program { }
