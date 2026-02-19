using Scalar.AspNetCore;

namespace Api_ArjSys_Tcc.Configurations;

public static class ScalarConfig
{
    public static WebApplication UseScalarConfig(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("ARJSYS API");
                options.WithTheme(ScalarTheme.BluePlanet);
            });
        }

        return app;
    }
}