using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System.Net.Mime;

namespace S3StorageTest.Controllersж;

public class ExampleController : ControllerBase 
{
    private readonly IMinioClient minioClient;
    private readonly ILogger logger;
    public ExampleController(IMinioClient minioClient, ILogger<ExampleController> logger)
    {
        this.minioClient = minioClient;
        this.logger = logger;
    }

    [HttpGet("check")]
    public async Task<IActionResult> CheckFile()
    {
        var getList = await minioClient.ListBucketsAsync();
        var beArgs = new BucketExistsArgs().WithBucket("test");
        bool found = await minioClient.BucketExistsAsync(beArgs);

        logger.LogInformation($"found {found}");

        if (!found || getList.Buckets is null) { return BadRequest(); }

        foreach(var item in  getList.Buckets) { logger.LogDebug($"{item.Name} {item.CreationDateDateTime}"); }

        return Ok(getList.Buckets);
    }

    [HttpGet("test")]
    public async Task<IActionResult> GetUrl(string name)
    {
        logger.LogInformation(name);

        //var objecturl = await minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs().
        //    WithBucket("test").
        //    WithObject(name).
        //    WithExpiry(60 * 60 * 20)
        //    );
        //logger.LogInformation("url:" + objecturl);

        var file = new MemoryStream();

        var object_file = await minioClient.GetObjectAsync(new 
            GetObjectArgs()
            .WithObject(name)
            .WithBucket("test")
            .WithCallbackStream((stream) => { stream.CopyTo(file); })
            );

        //return Ok(objecturl);
        //return File(objecturl,MediaTypeNames.Image.Jpeg);
        return File(file,object_file.ContentType);
    }

    [HttpPut("put")]
    public async Task<IActionResult> PutFile(IFormFile file)
    {
        var bucketName = "test";
        var contentType = "application/jpg";

        try
        {
            // Make a bucket on the server, if not already present.
            var beArgs = new BucketExistsArgs().WithBucket(bucketName);
            
            bool found = await minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(bucketName);
                await minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
            }
            
            var objName = Guid.NewGuid().ToString() + ".jpg";
            
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objName)
                .WithStreamData(file.OpenReadStream())
                .WithObjectSize(file.Length)
                .WithContentType(contentType);

            await minioClient.PutObjectAsync(putObjectArgs);
            
            var info = await minioClient.StatObjectAsync(new StatObjectArgs().WithBucket("test").WithObject(objName));
            
            logger.LogInformation($"name: {info.ObjectName} | Etag {info.ETag} | {info.ContentType}");
        }
        catch (MinioException e)
        {
            Console.WriteLine("File Upload Error: {0}", e.Message);
            return BadRequest();
        }
        return Ok();
    }

}
