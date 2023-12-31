using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using WebRozetka.Data;
using WebRozetka.Data.Entities.Identity;
using WebRozetka.FluentValidation.Categories;
using WebRozetka.Interfaces;
using WebRozetka.Mapper;
using WebRozetka.Models.Category;
using WebRozetka.Repository;
using WebRozetka.Services;

namespace WebRozetka
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ������ ������.
            builder.Services.AddDbContext<AppEFContext>(opt =>
                opt.UseNpgsql(builder.Configuration.GetConnectionString("WebRozetkaConnection")));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAutoMapper(typeof(AppMapProfile));
            builder.Services.AddCors();

            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();


            // IDENTITY SETTINGS
            builder.Services.AddIdentity<UserEntity, RoleEntity>(options =>
            {
                options.Stores.MaxLengthForKeys = 128;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.SignIn.RequireConfirmedEmail = true;
            })
              .AddEntityFrameworkStores<AppEFContext>()
              .AddDefaultTokenProviders();
            // -- IDENTITY SETTINGS


            // JWT SETTINGS
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSecretKey")));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config =>
            {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;
                config.TokenValidationParameters = new TokenValidationParameters()
                {

                    IssuerSigningKey = signinKey,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                };
            });
            // -- JWT SETTINGS


            var app = builder.Build();

            // ������������ ��������.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            app.UseSwagger();
            app.UseSwaggerUI();

            // �������� �����.
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "images");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(dir),
                RequestPath = "/images"
            });

            // ������������ CORS
            app.UseCors(options => options
                .WithOrigins(new[] { "http://localhost:5173" })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
            );
            // -- ������������ CORS

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.SeedData();

            app.Run();
        }
    }
}