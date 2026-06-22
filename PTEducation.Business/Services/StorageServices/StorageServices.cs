using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.StorageServices
{
    public class StorageServices : IStorageServices
    {
        private readonly CloudflareR2Settings _settings;
        private readonly IAmazonS3 _s3Client;

        public StorageServices(IOptions<CloudflareR2Settings> options)
        {
            _settings = options.Value;

            var config = new AmazonS3Config
            {
                ServiceURL = _settings.ServiceUrl,
                ForcePathStyle = true // Cloudflare R2 requires Path Style URLs
            };

            _s3Client = new AmazonS3Client(_settings.AccessKeyId, _settings.SecretAccessKey, config);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = fileName,
                BucketName = _settings.BucketName,
                ContentType = contentType,
                DisablePayloadSigning = true // R2 does not support payload signing for some regions/calls
            };

            await fileTransferUtility.UploadAsync(uploadRequest);

            if (!string.IsNullOrEmpty(_settings.PublicUrl))
            {
                return $"{_settings.PublicUrl.TrimEnd('/')}/{fileName.TrimStart('/')}";
            }
            return fileName;
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = fileName
            };

            var response = await _s3Client.GetObjectAsync(getRequest);
            return response.ResponseStream;
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = fileName
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }

        public async Task<string> GetPresignedUrlAsync(string fileName, double expiryHours = 24)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _settings.BucketName,
                Key = fileName,
                Expires = DateTime.UtcNow.AddHours(expiryHours)
            };

            return await Task.Run(() => _s3Client.GetPreSignedURL(request));
        }
    }
}
