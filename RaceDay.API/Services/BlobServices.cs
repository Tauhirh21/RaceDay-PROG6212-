using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    public class BlobService
    {
        private readonly string _connectionString;

        public BlobService(IConfiguration config)
        {
            // We will add this to appsettings.json next
            _connectionString = config.GetConnectionString("AzureBlobStorage")!;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            // 1. Connect to Azure
            var blobServiceClient = new BlobServiceClient(_connectionString);

            // 2. Get or create the container (folder)
            var containerClient = blobServiceClient.GetBlobContainerClient("event-banners");
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // 3. Add a unique identifier to the filename so files don't overwrite each other
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            // 4. Upload the file
            await blobClient.UploadAsync(fileStream, true);

            // 5. Return the URL of the uploaded image
            return blobClient.Uri.ToString();
        }
    }
}
