#define ERRORHANDLER // this can invoke a different error handler component to allow the user more clarity into why the server isn't starting.
//in debug scenarios though, it can get in the way of your ability to debug exceptions that arise, so it is disabled by default in these cases.

#if DEBUG
#undef ERRORHANDLER
#endif

using nio2so.TSOHTTPS.Protocol.Controllers;
using nio2so.TSOHTTPS.Protocol.Services;
using System.Net;
using System.Reflection;

namespace nio2so.TSOHTTPS
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if ERRORHANDLER
            try
            {
#endif
                ServicePointManager.SecurityProtocol =
               SecurityProtocolType.Tls12 |
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
#if ERRORHANDLER
            }
            catch (Exception e)
            {
                nio2so.CrashHandler.ErrorWindow.InvokeErrorHandler(e, new nio2so.CrashHandler.ErrorHandlerArgs("Is your server already running?", "Is the server certifcate trusted?")
                {
                    SourceProgramName = Assembly.GetEntryAssembly().GetName().Name
                });
            }
#endif
        }
    }
}