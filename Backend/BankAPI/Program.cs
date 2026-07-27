using BankAPI.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => 
{
    options.AddPolicy("AngularProject",policy => {
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .WithOrigins("http://localhost:4200")
              .AllowCredentials();  
    });
});
// --- SERVİSLERİ KAYDETME BÖLÜMÜ ---
builder.Services.AddScoped<BankAPI.Services.MusteriService>();
builder.Services.AddScoped<BankAPI.Services.AdresService>();
builder.Services.AddScoped<BankAPI.Services.HesapService>();
builder.Services.AddScoped<BankAPI.Services.HesapHareketService>();
builder.Services.AddScoped<BankAPI.Services.DashboardService>();
builder.Services.AddScoped<BankAPI.Services.AdminService>();

// YENİ: Controller (MusteriController vb.) sınıflarını okuma yeteneğini açıyoruz
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // [ApiController] model doğrulama hatalarını varsayılan olarak ProblemDetails
        // formatında döndürüyor; Angular tarafı ise her hatada "message" alanını
        // okuduğu için diğer uçlarla aynı { message } biçimine çeviriyoruz.
        options.InvalidModelStateResponseFactory = context =>
        {
            var mesaj = string.Join(" ", context.ModelState.Values
                .SelectMany(deger => deger.Errors)
                .Select(hata => hata.ErrorMessage));

            return new BadRequestObjectResult(new { message = mesaj });
        };
    });

// YENİ: Swagger görsel arayüzünü oluşturacak sistemi ekliyoruz
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AngularProject");

// --- HTTP AYARLARI BÖLÜMÜ ---
if (app.Environment.IsDevelopment())
{
    // YENİ: Uygulama çalışırken o güzel arayüzü yayına al
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// YENİ: Gelen istekleri Controller'daki metodlarımıza yönlendir
app.MapControllers(); 

app.Run();
