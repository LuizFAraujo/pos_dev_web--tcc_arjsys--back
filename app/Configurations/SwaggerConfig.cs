namespace Api_ArjSys_Tcc.Configurations;

public static class SwaggerConfig
{
    public static WebApplication UseSwaggerConfig(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "ARJSYS API v1");
                options.RoutePrefix = "swagger";
            });
        }

        return app;
    }
}