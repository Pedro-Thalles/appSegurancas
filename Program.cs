using Microsoft.EntityFrameworkCore;
using appSegurancas.Data;
using Microsoft.AspNetCore.Identity;
using appSegurancas.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<segDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IPasswordHasher<Seguranca>, PasswordHasher<Seguranca>>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Seguranca/Login"; // Rota para a página de login
        options.AccessDeniedPath = "/Seguranca/AcessoNegado"; // Rota para acesso negado
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Tempo de expiração do cookie
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();


//isso é a rota inicial
app.MapControllerRoute(
    name: "default",
    //ele procura um CONTROLLER. Se não encontrar, ele procura o HomeController.
    //Depois ele procura a ACTION. Se não encontrar, ele procura a Index.
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
