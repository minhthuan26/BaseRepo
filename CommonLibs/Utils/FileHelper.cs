using ClosedXML.Excel;
using CommonLibs.DTOs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.Data;
using System.Formats.Asn1;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Xml.Serialization;

namespace CommonLibs.Utils
{
    public static class FileHelper
    {
        public static async Task<List<T>> ReadFile<T>(
            Stream stream,
            string fileName,
            Dictionary<string, string>? headers)
            where T : class, new()
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            stream.Position = 0;

            switch (extension)
            {
                case ".csv":
                    ValidateCsvHeader(stream, headers);
                    stream.Position = 0;
                    return await ReadCsv<T>(stream);

                case ".xlsx":
                case ".xls":
                    ValidateExcelHeader(stream, headers);
                    stream.Position = 0;
                    return await ReadExcel<T>(stream);

                default:
                    throw new AppException(
                        $"Unsupported file type '{extension}'.",
                        HttpStatusCode.BadRequest);
            }
        }

        private static void ValidateExcelHeader(
            Stream stream,
            Dictionary<string, string>? expectedHeaders)
        {
            if (expectedHeaders == null || expectedHeaders.Count == 0)
                return;

            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheet(1);

            var actualHeaders = worksheet.FirstRowUsed()
                .CellsUsed()
                .Select(x => x.GetString().Trim())
                .ToArray();

            ValidateHeaders(actualHeaders, expectedHeaders.Keys);
        }

        private static void ValidateCsvHeader(
            Stream stream,
            Dictionary<string, string>? expectedHeaders)
        {
            if (expectedHeaders == null || expectedHeaders.Count == 0)
                return;

            using var reader = new StreamReader(stream, leaveOpen: true);

            var firstLine = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(firstLine))
            {
                throw new AppException(
                    "File is empty.",
                    HttpStatusCode.BadRequest);
            }

            var actualHeaders = firstLine
                .Split(',')
                .Select(x => x.Trim())
                .ToArray();

            ValidateHeaders(actualHeaders, expectedHeaders.Keys);
        }

        private static void ValidateHeaders(
            IEnumerable<string> actual,
            IEnumerable<string> expected)
        {
            var missing = expected
                .Except(actual, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (missing.Any())
            {
                throw new AppException(
                    $"Invalid template. Missing columns: {string.Join(", ", missing)}.",
                    HttpStatusCode.BadRequest);
            }
        }

        private static async Task<List<T>> ReadCsv<T>(Stream stream) where T : class, new()
        {
            using var reader = new StreamReader(stream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var csv = new CsvReader(reader, config);

            return await Task.FromResult(csv.GetRecords<T>().ToList());
        }

        private static Task<List<T>> ReadExcel<T>(Stream stream) where T : class, new()
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            var headerRow = worksheet.FirstRowUsed();

            if (headerRow == null)
                return Task.FromResult(new List<T>());

            // Header trong Excel
            var excelHeaders = headerRow.CellsUsed()
                .Select((c, i) => new
                {
                    Index = i + 1,
                    Name = c.GetString().Trim()
                })
                .ToList();

            // Property của DTO
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            // Lấy HeaderMappings nếu DTO kế thừa BaseInputDTO
            Dictionary<string, string>? headerMappings = null;

            if (Activator.CreateInstance<T>() is BaseInputDTO dto)
            {
                headerMappings = dto.HeaderMappings;
            }

            List<T> result = new();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                T item = new();

                foreach (var header in excelHeaders)
                {
                    string propertyName = header.Name;

                    if (headerMappings != null &&
                        headerMappings.TryGetValue(header.Name, out var mappedProperty))
                    {
                        propertyName = mappedProperty;
                    }

                    if (!properties.TryGetValue(propertyName, out var property))
                        continue;

                    var value = row.Cell(header.Index).GetFormattedString().Trim();

                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    try
                    {
                        property.SetValue(
                            item,
                            ConvertValue(value, property.PropertyType));
                    }
                    catch (Exception ex)
                    {
                        throw new AppException(
                            $"Invalid value '{value}' for column '{header.Name}' at row {row.RowNumber()}. {ex.Message}",
                            HttpStatusCode.BadRequest);
                    }
                }

                result.Add(item);
            }

            return Task.FromResult(result);
        }

        private static object? ConvertValue(string value, Type propertyType)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            Type targetType =
                Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (targetType == typeof(string))
                return value;

            if (targetType.IsEnum)
            {
                if (int.TryParse(value, out var enumInt))
                    return Enum.ToObject(targetType, enumInt);

                if (Enum.TryParse(targetType, value, true, out var enumValue))
                    return enumValue;

                throw new Exception($"'{value}' is not a valid {targetType.Name}.");
            }

            if (targetType == typeof(Guid))
                return Guid.Parse(value);

            if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(value, out var date))
                    return date;

                throw new Exception($"'{value}' is not a valid DateTime.");
            }

            if (targetType == typeof(bool))
            {
                if (bool.TryParse(value, out var b))
                    return b;

                if (value == "1")
                    return true;

                if (value == "0")
                    return false;

                throw new Exception($"'{value}' is not a valid Boolean.");
            }

            if (targetType == typeof(int))
                return int.Parse(value, CultureInfo.InvariantCulture);

            if (targetType == typeof(long))
                return long.Parse(value, CultureInfo.InvariantCulture);

            if (targetType == typeof(short))
                return short.Parse(value, CultureInfo.InvariantCulture);

            if (targetType == typeof(decimal))
                return decimal.Parse(value, CultureInfo.InvariantCulture);

            if (targetType == typeof(double))
                return double.Parse(value, CultureInfo.InvariantCulture);

            if (targetType == typeof(float))
                return float.Parse(value, CultureInfo.InvariantCulture);

            if (targetType == typeof(byte))
                return byte.Parse(value, CultureInfo.InvariantCulture);

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Writes raw string content to a file. If the directory doesn't exist, it will be created.
        /// </summary>
        public static async Task<bool> WriteToFile(string filename, string content)
        {
            try
            {
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filename);

                // Ensure target directory exists
                var directory = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(absolutePath, content);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Reads a JSON file and deserializes it into a strongly-typed object.
        /// </summary>
        public static async Task<T?> ReadJSON<T>(string filename, JsonSerializerOptions? options = null)
        {
            try
            {
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filename);
                if (!File.Exists(absolutePath))
                {
                    throw new AppException("JSON file not found.", HttpStatusCode.NotFound);
                }

                using var stream = File.OpenRead(absolutePath);
                return await JsonSerializer.DeserializeAsync<T>(stream, options);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Reads an XML file and deserializes it into a strongly-typed object.
        /// </summary>
        public static async Task<T?> ReadXML<T>(string filename)
        {
            try
            {
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filename);
                if (!File.Exists(absolutePath))
                {
                    throw new AppException("XML file not found.", HttpStatusCode.NotFound);
                }

                // XmlSerializer does not have native async stream reading methods, 
                // so we read the text asynchronously first.
                string xmlContent = await File.ReadAllTextAsync(absolutePath);

                using var reader = new StringReader(xmlContent);
                var serializer = new XmlSerializer(typeof(T));
                return (T?)serializer.Deserialize(reader);
            }
            catch (Exception)
            {
                throw;
            }
        }

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
