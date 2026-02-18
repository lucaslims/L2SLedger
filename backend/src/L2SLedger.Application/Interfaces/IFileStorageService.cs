namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Interface para serviço de armazenamento e gerenciamento de arquivos de exportação.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Salva o arquivo de exportação e retorna o caminho onde foi salvo.
    /// </summary>
    /// <param name="content">Conteúdo do arquivo a ser salvo.</param>
    /// <param name="fileName">Nome do arquivo a ser salvo.</param>
    /// <returns>Caminho onde o arquivo foi salvo.</returns>
    Task<string> SaveExportFileAsync(byte[] content, string fileName);

    /// <summary>
    /// Lê o conteúdo do arquivo de exportação a partir do caminho fornecido.
    /// </summary>
    /// <param name="filePath">Caminho do arquivo a ser lido.</param>
    /// <returns>Conteúdo do arquivo.</returns>
    Task<byte[]> ReadExportFileAsync(string filePath);

    /// <summary>
    /// Deleta o arquivo de exportação no caminho fornecido.
    /// </summary>
    /// <param name="filePath">Caminho do arquivo a ser deletado.</param>
    /// <returns></returns>
    Task DeleteExportFileAsync(string filePath);

    /// <summary>
    /// Remove arquivos de exportação mais antigos que a data fornecida.
    /// </summary>
    /// <param name="olderThan">Data limite para remoção de arquivos.</param>
    /// <returns>Número de arquivos removidos.</returns>
    Task<int> CleanupOldExportsAsync(DateTime olderThan);

    /// <summary>
    /// Obtém o tamanho do arquivo em bytes.
    /// </summary>
    /// <param name="filePath">Caminho do arquivo.</param>
    /// <returns>Tamanho do arquivo em bytes.</returns>
    long GetFileSizeBytes(string filePath);
}
