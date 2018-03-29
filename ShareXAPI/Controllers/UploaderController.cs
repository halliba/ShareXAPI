using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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

        [HttpPost("/{someName}")]
        public IActionResult Post([FromRoute]string someName, [FromForm]Model model)
        {
            var file = model.File;
            var apiKey = model.ApiKey;
            if (file == null)
            {
                return BadRequest("No file given");
            }
            var uploader =
                _options.Uploader.FirstOrDefault(
                    s => s.WebBasePath.Equals(someName, StringComparison.OrdinalIgnoreCase));
            if (uploader == null)
            {
                return NotFound();
            }
            if (uploader.ApiKey != apiKey && !string.IsNullOrEmpty(uploader.ApiKey))
            {
                return new UnauthorizedResult();
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!uploader.FileExtensions.Contains(fileExtension) && !uploader.FileExtensions.Contains("*") || file.Length > (1024 * 1024) * uploader.MaxFileSize)
            {
                return BadRequest(
                    $"File does not meet the Requirements. Maximum size {uploader.MaxFileSize}MB, Allowed Extensions [{string.Join(", ", uploader.FileExtensions)}]");
            }

            Directory.CreateDirectory(uploader.LocalBasePath);

            var fileName = GetRandomFileName(fileExtension);
            var filePath = Path.Combine(uploader.LocalBasePath, fileName);
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
    }

    public class Model
    {
        public IFormFile File { get; set; }

        public string ApiKey { get; set; }
    }
}
