using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.Net;

namespace CommonLibs.Utils
{
    public static class FileHelper
    {
        public static async Task<bool> DeleteFile(string filename)
        {
            try
            {
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filename);
                if (!File.Exists(absolutePath))
                {
                    throw new AppException("File not founded.", HttpStatusCode.NotFound);
                }
                File.Delete(absolutePath);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<FileDownload> GetFile(string filename)
        {
            try
            {
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filename);

                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(absolutePath, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                var bytes = await File.ReadAllBytesAsync(absolutePath);
                return new FileDownload
                {
                    Bytes = bytes,
                    ContentType = contentType,
                    Filename = filename
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<List<FileDownload>> GetFiles(string[] filenames)
        {
            try
            {
                List<FileDownload> result = new();
                foreach (var filename in filenames)
                {
                    var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filename);

                    var provider = new FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(absolutePath, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }

                    var bytes = await File.ReadAllBytesAsync(absolutePath);
                    var file = new FileDownload
                    {
                        Bytes = bytes,
                        ContentType = contentType,
                        Filename = filename
                    };
                    result.Add(file);
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<List<FileDownload>> GetFilesInDirectory(string directory)
        {
            try
            {
                List<FileDownload> result = new();
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), directory);
                var filenames = Directory.GetFiles(absolutePath);

                foreach (var filename in filenames)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), filename);

                    var provider = new FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(path, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }

                    var bytes = await File.ReadAllBytesAsync(path);

                    result.Add(new FileDownload
                    {
                        Bytes = bytes,
                        ContentType = contentType,
                        Filename = filename
                    });
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<bool> DeleteFiles(string directory)
        {
            try
            {
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), directory);

                if (Directory.Exists(absolutePath))
                {
                    var files = Directory.GetFiles(absolutePath);
                    foreach (var file in files)
                    {
                        var path = Path.Combine(Directory.GetCurrentDirectory(), file);
                        File.Delete(path);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<bool> DeleteDirectory(string directory)
        {
            try
            {
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), directory);

                if (Directory.Exists(absolutePath))
                {
                    var files = Directory.GetFiles(absolutePath);
                    foreach (var file in files)
                    {
                        var path = Path.Combine(Directory.GetCurrentDirectory(), file);
                        File.Delete(path);
                    }
                    Directory.Delete(absolutePath);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<bool> CreateDirectory(string directory)
        {
            try
            {
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), directory);

                if (!Directory.Exists(absolutePath))
                {
                    Directory.CreateDirectory(absolutePath);
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<bool> UploadFile(IFormFile file, string filepath)
        {
            try
            {
                var fileInfo = new FileInfo(file.FileName);
                string filenameWithoutExtnsion = file.FileName[..file.FileName.LastIndexOf(".")];
                string filename = filenameWithoutExtnsion + "_" + DateTime.Now.Ticks.ToString() + fileInfo.Extension;

                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filepath);

                if (!Directory.Exists(absolutePath))
                {
                    Directory.CreateDirectory(absolutePath);
                }

                var exactPath = Path.Combine(Directory.GetCurrentDirectory(), filepath, filename);
                using var stream = new FileStream(exactPath, FileMode.Create);
                await file.CopyToAsync(stream);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<bool> UploadFiles(List<IFormFile> files, string filepath)
        {
            try
            {
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file.FileName);
                    string filenameWithoutExtnsion = file.FileName[..file.FileName.LastIndexOf(".")];
                    string filename = filenameWithoutExtnsion + "_" + DateTime.Now.Ticks.ToString() + fileInfo.Extension;

                    var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filepath);

                    if (!Directory.Exists(absolutePath))
                    {
                        Directory.CreateDirectory(absolutePath);
                    }

                    var exactPath = Path.Combine(Directory.GetCurrentDirectory(), filepath, filename);
                    using var stream = new FileStream(exactPath, FileMode.Create);
                    await file.CopyToAsync(stream);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class FileDownload
    {
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; } = "application/octet-stream";
        public string Filename { get; set; }
    }
}
