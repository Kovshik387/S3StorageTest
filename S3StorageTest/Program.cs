using Minio;

public static class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        var endpoint = "127.0.0.1:9000";
        var accessKey = "CQ4uKPZhmExjYQ5c8Vuc";
        var secretKey = "v9FJvA04SI09ZdNoOEBgEuNADfzJOtlHVyGyaypC";

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen().AddCors();

        builder.Services.AddMinio(configureClient => configureClient
                   .WithEndpoint(endpoint)
                   .WithCredentials(accessKey, secretKey)
                   .WithSSL(true)
                   );

        var app = builder.Build();

        // Configure the HTTP request pipeline.
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
