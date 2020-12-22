using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Json.Zip
{
    public interface IJsonZipSerializer
    {
        Task SerializeAsync<T>(T obj, string outputFilePath, int compressionLevel = 5, Func<Stream, CompressionLevel, Stream> compressorFactory = null, CancellationToken cancellationToken = default);
        Task<Stream> SerializeAsync<T>(T obj, int compressionLevel = 5, Func<Stream, CompressionLevel, Stream> compressorFactory = null, CancellationToken cancellationToken = default);

        Task<T> DeserializeAsync<T>(Stream inputStream, bool leaveOpen = false, Func<Stream, Stream> decompressFactory = null, CancellationToken cancellationToken = default);
        Task<T> DeserializeAsync<T>(string inputFilePath, Func<Stream, Stream> decompressFactory = null, CancellationToken cancellationToken = default);
    }
}