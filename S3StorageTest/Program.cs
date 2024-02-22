using Minio;

public static class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        var endpoint = "172.19.0.3:9000"; //работает

        //var accessKey = "prsIiezvuWkHxIAhFid2";
        //var secretKey = "RujN8ZOadiXDuYaS3oJEqcRQSIFqAgeciabGehYI";

        var accessKey = "heOkcd71mpwvzCIuRkkO";
        var secretKey = "EqrMmyG6KRai6J81s1fJSHmuYB4UQ24ftzRhgyJ8";

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen().AddCors();

        builder.Services.AddMinio(configureClient => configureClient
                   .WithEndpoint(endpoint)
                   .WithCredentials(accessKey, secretKey)
                   .WithSSL(false)
                    );

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}
