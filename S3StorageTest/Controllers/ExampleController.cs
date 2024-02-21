using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

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
        var getList = await minioClient.ListBucketsAsync().ConfigureAwait(false);
        var beArgs = new BucketExistsArgs().WithBucket("test");
        bool found = await minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);

        if (found) { Ok(); }

        logger.LogCritical(getList.Owner);

        foreach(var item in  getList.Buckets) { logger.LogDebug($"{item.Name} {item.CreationDateDateTime}"); }

        return Ok(getList.Buckets);
    }

    [HttpGet("test")]
    public async Task<string> GetUrl()
    {
        // Create an async task for listing buckets.
        var getListBucketsTask = await minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs().
            WithBucket("test").
            WithObject("golden-oldies.jpg").
            WithExpiry(60*60)
            );
        return getListBucketsTask;
        //return Ok(await minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs().WithBucket("test-kovshik")));
    }

    [HttpPut("put")]
    public async Task<IActionResult> PutFile([FromBody]IFormFile file)
    {
        var bucketName = "test";
        var objectName = "golden-oldies.jpg";
        var contentType = "application/jpg";

        try
        {
            // Make a bucket on the server, if not already present.
            var beArgs = new BucketExistsArgs()
                .WithBucket(bucketName);
            bool found = await minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(bucketName);
                await minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
            }
            // Upload a file to bucket.
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithStreamData(file.OpenReadStream())
                .WithFileName(file.FileName)
                .WithContentType(contentType);
            await minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
            Console.WriteLine("Successfully uploaded " + objectName);
        }
        catch (MinioException e)
        {
            Console.WriteLine("File Upload Error: {0}", e.Message);
            return BadRequest();
        }
        return Ok();
    }

}
