using System.IO;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.StorageServices
{
    public interface IStorageServices
    {
        /// <summary>
        /// Uploads a file from stream to Cloudflare R2 storage.
        /// </summary>
        /// <param name="fileStream">Stream of the file content</param>
        /// <param name="fileName">Target file name/path in the bucket</param>
        /// <param name="contentType">MIME content type of the file (e.g. image/jpeg, application/pdf)</param>
        /// <returns>Public access URL or file path in bucket</returns>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Downloads a file from Cloudflare R2 storage.
        /// </summary>
        /// <param name="fileName">File name/path in the bucket</param>
        /// <returns>Stream containing the file content</returns>
        Task<Stream> DownloadFileAsync(string fileName);

        /// <summary>
        /// Deletes a file from Cloudflare R2 storage.
        /// </summary>
        /// <param name="fileName">File name/path in the bucket to delete</param>
        Task DeleteFileAsync(string fileName);

        /// <summary>
        /// Generates a presigned URL to temporarily download/access a private file.
        /// </summary>
        /// <param name="fileName">File name/path in the bucket</param>
        /// <param name="expiryHours">Expiry duration in hours</param>
        /// <returns>Presigned download URL</returns>
        Task<string> GetPresignedUrlAsync(string fileName, double expiryHours = 24);
    }
}
