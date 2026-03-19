
using AnalisisVentas.Data.Persistence;
using Microsoft.OpenApi.Models;

namespace AnalisisVentas.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);

            // 🔹 Configuración
            builder.Configuration.AddJsonFile("appsettings.json", optional: false);

            // 🔹 Servicios
            builder.Services.AddSingleton<DapperContext>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // 🔹 Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}