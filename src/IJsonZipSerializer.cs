using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Json.Zip
{
    public interface IJsonZipSerializer
    {
        Task SerializeAsync<T>(T obj, string outputFilePath, int compressionLevel = 5, CancellationToken cancellationToken = default);
        Task<Stream> SerializeAsync<T>(T obj, int compressionLevel = 5, CancellationToken cancellationToken = default);

        Task<T> DeserializeAsync<T>(Stream inputStream, bool keepOpen = false, CancellationToken cancellationToken = default);
        Task<T> DeserializeAsync<T>(string inputFilePath, CancellationToken cancellationToken = default);
    }
}