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

        [HttpPost]
        public IActionResult PostImage(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest("No file given");
            }
            var fileExtension = Path.GetExtension(file.FileName);

            if (!_options.FileExtensions.Contains(fileExtension) || file.Length > (1024 * 1024) * _options.MaxImageSize)
            {
                return BadRequest(
                    $"File does not meet the Requirements. Maximum size {_options.MaxImageSize}MB, Allowed Extensions [{string.Join(", ", _options.FileExtensions)}]");
            }

            var fileName = Path.ChangeExtension(Guid.NewGuid().ToString("D"), fileExtension);
            var filePath = Path.Combine(_options.LocalImageBasePath, fileName);
            Directory.CreateDirectory(_options.LocalImageBasePath);

            using (var fs = System.IO.File.Create(filePath))
            {
                file.CopyTo(fs);
            }

            return LocalRedirect($"{_options.WebImageBasePath}/{fileName}");
        }
    }
}
