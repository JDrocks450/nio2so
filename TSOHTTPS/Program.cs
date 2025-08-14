using nio2so.TSOHTTPS.Protocol.Controllers;
using nio2so.TSOHTTPS.Protocol.Services;
using System.Net;

namespace TSOHTTPS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Net.ServicePointManager.SecurityProtocol =
               System.Net.SecurityProtocolType.Tls12 | 
               SecurityProtocolType.Tls11 | 
               SecurityProtocolType.Tls;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpClient<nio2soMVCDataServiceClient>();
            builder.Services.AddMvc().AddApplicationPart(typeof(AuthLoginController).Assembly);
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }           
            
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}