using L2SLedger.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.Services;

/// <summary>
/// Serviço para armazenamento de arquivos de exportação.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _baseDirectory;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _baseDirectory = Path.Combine(AppContext.BaseDirectory, "exports");
        _logger = logger;

        // Garantir que diretório existe
        if (!Directory.Exists(_baseDirectory))
        {
            Directory.CreateDirectory(_baseDirectory);
            _logger.LogInformation("Created exports directory: {Directory}", _baseDirectory);
        }
    }

    public async Task<string> SaveExportFileAsync(byte[] content, string fileName)
    {
        var filePath = Path.Combine(_baseDirectory, fileName);
        
        await File.WriteAllBytesAsync(filePath, content);
        
        _logger.LogInformation("Export file saved: {FilePath}, Size: {Size} bytes", filePath, content.Length);
        
        return filePath;
    }

    public async Task<byte[]> ReadExportFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Export file not found: {filePath}");
        }

        return await File.ReadAllBytesAsync(filePath);
    }

    public Task DeleteExportFileAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Export file deleted: {FilePath}", filePath);
        }

        return Task.CompletedTask;
    }

    public async Task<int> CleanupOldExportsAsync(DateTime olderThan)
    {
        var files = Directory.GetFiles(_baseDirectory);
        var deletedCount = 0;

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.CreationTimeUtc < olderThan)
            {
                await DeleteExportFileAsync(file);
                deletedCount++;
            }
        }

        if (deletedCount > 0)
        {
            _logger.LogInformation("Cleaned up {Count} old export files", deletedCount);
        }

        return deletedCount;
    }

    public long GetFileSizeBytes(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return 0;
        }

        var fileInfo = new FileInfo(filePath);
        return fileInfo.Length;
    }
}
