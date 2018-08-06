using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShareXAPI.Models;
using ShareXAPI.Options;

namespace ShareXAPI.Controllers
{
    
    public class ImageController : Controller
    {
        private readonly ApiOptions _options;

        public ImageController(IOptions<ApiOptions> options)
        {
            _options = options.Value;
        }

        [HttpPost("/{uploadName}")]
        public IActionResult Post([FromRoute]string uploadName, [FromForm]PostFileModel model)
        {
            var file = model.File;
            var apiKey = model.ApiKey;
            if (file == null)
            {
                return BadRequest("No file given");
            }

            var uploaders =
                _options.Uploader.Where(
                    s => s.WebBasePath.Equals(uploadName, StringComparison.OrdinalIgnoreCase));
            if (uploaders == null)
            {
                return BadRequest(
                    $"Uploader not Found - Wrong path?");
            } 
            if (!uploaders.Any(s => s.ApiKey == apiKey && !string.IsNullOrEmpty(s.ApiKey)))
            {
                return new UnauthorizedResult();
            }

            var uploader =
                _options.Uploader.FirstOrDefault(
                    s => s.WebBasePath.Equals(uploadName, StringComparison.OrdinalIgnoreCase) && s.ApiKey == apiKey);

            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!uploader.FileExtensions.Contains(fileExtension) && !uploader.FileExtensions.Contains("*") || file.Length > (1024 * 1024) * uploader.MaxFileSize)
            {
                return BadRequest(
                    $"File does not meet the Requirements. Maximum size {uploader.MaxFileSize}MB, Allowed Extensions [{string.Join(", ", uploader.FileExtensions)}]");
            }

            Directory.CreateDirectory(uploader.LocalBasePath);

            var fileName = GetRandomFileName(fileExtension);
            var filePath = Path.Combine(uploader.LocalBasePath, fileName);
            var fileSize = file.Length;

            if (uploader.MaxFolderSize > 0)
            {
                if (fileSize > uploader.MaxFolderSize*1024*1024)
                {
                    return BadRequest("File bigger than max foldersize");
                }

                while (GetDirectorySize(uploader.LocalBasePath) + fileSize > uploader.MaxFolderSize*1024*1024)
                {
                    DeleteOldestFile(uploader.LocalBasePath);
                }
            }
            
            while (System.IO.File.Exists(filePath))
            {
                fileName = GetRandomFileName(fileExtension);
                filePath = Path.Combine(uploader.LocalBasePath, fileName);
            }

            using (var fs = System.IO.File.Create(filePath))
            {
                file.CopyTo(fs);
            }

            return LocalRedirect($"/{uploader.WebBasePath}/{fileName}");
        }

        private string GetRandomFileName(string extension) =>
            Path.ChangeExtension(Guid.NewGuid().ToString("N").Substring(0, 10), extension);

        private long GetDirectorySize(string directoryPath) =>
            Directory.GetFiles(directoryPath, "*.*").Select(name => new FileInfo(name))
                .Select(currentFile => currentFile.Length).Sum();

        private void DeleteOldestFile(string directoryPath) =>
            System.IO.File.Delete(Path.Combine(directoryPath,
                Directory.GetFiles(directoryPath).Select(name => new FileInfo(name))
                    .OrderBy(currentFile => currentFile.CreationTime).FirstOrDefault()?.Name));
    }
}