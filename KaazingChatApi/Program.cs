using KaazingChatApi;
using KaazingChatApi.JWTAuthentication.Authentication;
using KaazingTestWebApplication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen.ConventionalRouting;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

Startup.Configure(builder.Environment);

var AllowSpecificOrigin = Startup.AppSettingManager.GetSection("appSettings:AllowSpecificOrigin").Get<List<String>>();

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins(AllowSpecificOrigin.ToArray<string>())
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
});

builder.Services.AddMvc(config =>
{
    config.Filters.Add(new WebApiExceptionFilter());
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
    options.SerializerSettings.Converters.Add(new StringEnumConverter
    {
        NamingStrategy = new CamelCaseNamingStrategy
        {
            OverrideSpecifiedNames = true
        }
    });
}).AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KaazingChatApi",
        Version = "v1",
        Description = "This is ASP.NET Core RESTful KaazingChat WebAPI.",
        Contact = new OpenApiContact
        {
            Name = "Leon Li",
        }
    }
    );
    var filePath = Path.Combine(AppContext.BaseDirectory, "KaazingChatApi.xml");    //專案>建置>輸出>文件檔案:產出包含專案公用API文件的檔案的預設路徑位置=AppContext.BaseDirectory
    c.IncludeXmlComments(filePath, includeControllerXmlComments: true);
    //c.EnableAnnotations();  //Controller檔中使用 SwaggerResponse 方式時,需此行程式碼
    //add the ENUM types to the Swagger Document
    c.SchemaFilter<KaazingChatApi.Utility.EnumTypesSchemaFilter>(filePath);
});

//使用傳統路由方式時,需加入Package "Swashbuckle.AspNetCore.SwaggerGen.ConventionalRouting",
//並使用下列程式碼讓swagger產生器使用傳統路由
builder.Services.AddSwaggerGenWithConventionalRoutes(options =>
{
    options.IgnoreTemplateFunc = (template) => template.StartsWith("api/");
    options.SkipDefaults = true;
});

// For Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Startup.AppSettingManager.GetConnectionString("default")));

// For Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

// Adding Jwt Bearer
.AddJwtBearer(options =>
             {
                 options.SaveToken = true;
                 options.RequireHttpsMetadata = false;
                 options.TokenValidationParameters = new TokenValidationParameters()
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     ClockSkew = TimeSpan.Zero,

                     ValidAudience = Startup.AppSettingManager.GetSection("JWT:ValidAudience").Value,
                     ValidIssuer = Startup.AppSettingManager.GetSection("JWT:ValidIssuer").Value,
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Startup.AppSettingManager.GetSection("JWT:Secret").Value))
                 };
             });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSession();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

//下面使用傳統路由方式時,Controller程式檔需拿掉Route屬性程式的引用，
//並配合使用Package "Swashbuckle.AspNetCore.SwaggerGen.ConventionalRouting"
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "KaazingChatWebApi/api/{controller}/{action}/{id?}");

    // Pass the conventional routes to the generator
    ConventionalRoutingSwaggerGen.UseRoutes(endpoints);
});

app.MapControllers();

app.Run();
