using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using BulkyBook.Utility;
using Stripe;
using BulkyBook.DataAccess.DbInit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
// Add DbContxt to container
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
//Add Strike(ตัวจัดการเงิน) โดยจะ bind value จาก Class StripeSettings กับ appsetting.json ให้อัตโนมัติ 
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

//Custom Identity การระบุตัวตน , Role
builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

//Scope
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDbinit, Dbinit>();


//Email
builder.Services.AddSingleton<IEmailSender, EmailSender>();

//ถ้าไม่ได้ Login แต่ไปกด add to cart จะ route path ผิด ต้อง config ใหม่
builder.Services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = $"/Identity/Account/Login";
    option.LogoutPath = $"/Identity/Account/Logout";
    option.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

//ถ้าจะใช้ cookies หรือ Session ต้อง add code นี้เข้าไป
// Session เก็บได้เฉพาะ Integer และ String
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//facebook login
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "725315725591861";
    options.AppSecret = "8a516ef9f51abbee9fccda8441391b84";
});


var app = builder.Build();


#region pipline

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

//setting stripe
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

//Init Database
SeedDatabase();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

// ใช้ใน LoginPartrial
app.MapRazorPages();

//ใช้ Cookies , Session
app.UseSession();

#endregion

app.Run();


void SeedDatabase()
{
    using(var scope = app.Services.CreateScope())
    {
        //เรียกใช้ Scope
        var dbInit = scope.ServiceProvider.GetRequiredService<IDbinit>();
        dbInit.Init();
    }
}