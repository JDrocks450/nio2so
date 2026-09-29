#define ERRORHANDLER // this can invoke a different error handler component to allow the user more clarity into why the server isn't starting.
//in debug scenarios though, it can get in the way of your ability to debug exceptions that arise, so it is disabled by default in these cases.

#if DEBUG
#undef ERRORHANDLER
#endif

using System.Reflection;

#if ERRORHANDLER
try
{
#endif
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

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
