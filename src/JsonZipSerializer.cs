using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Json.Zip
{
    public class JsonZipSerializer : IJsonZipSerializer
    {
        private const int DefaultDecompressionLevel = 6;
        private JsonZipSerializer()
        {
        }

        public static JsonZipSerializer Instance { get; } = new JsonZipSerializer();

        public async Task<T> DeserializeAsync<T>(Stream inputStream, bool keepOpen = false, Func<Stream, Stream> decompressFactory = null, CancellationToken cancellationToken = default)
        {
            decompressFactory = decompressFactory ?? ((iStream) => new BrotliStream(iStream, CompressionMode.Decompress));

            T result = default;
            using (MemoryStream output = new MemoryStream())
            using (Stream decompressor = decompressFactory(inputStream))
            {
                await decompressor.CopyToAsync(output).ConfigureAwait(false);
                output.Position = 0;
                result = await JsonSerializer.DeserializeAsync<T>(output, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            if (!keepOpen)
            {
                inputStream.Close();
            }
            return result;
        }


        public async Task<T> DeserializeAsync<T>(string inputFilePath, Func<Stream, Stream> decompressFactory = null, CancellationToken cancellationToken = default)
        {

            T result = default;
            using (FileStream input = File.OpenRead(inputFilePath))
            {
                result = await DeserializeAsync<T>(input, keepOpen: true, decompressFactory, cancellationToken);
            }
            return result;
        }

        public async Task SerializeAsync<T>(T obj, string outputFilePath, int compressionLevel = DefaultDecompressionLevel, Func<Stream, CompressionLevel, Stream> compressorFactory = null, CancellationToken cancellationToken = default)
        {
            using (Stream input = await SerializeAsync(obj, compressionLevel, compressorFactory: compressorFactory, cancellationToken: cancellationToken).ConfigureAwait(false))
            using (FileStream output = File.OpenWrite(outputFilePath))
            {
                await input.CopyToAsync(output).ConfigureAwait(false);
            }
        }

        public async Task<Stream> SerializeAsync<T>(T obj, int compressionLevel = DefaultDecompressionLevel, Func<Stream, CompressionLevel, Stream> compressorFactory = null, CancellationToken cancellationToken = default)
        {
            compressorFactory = compressorFactory ?? ((oStream, level) => new BrotliStream(oStream, level, leaveOpen: true));

            Stream outputStream = new MemoryStream();
            using (MemoryStream inputStream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(inputStream, obj, cancellationToken: cancellationToken).ConfigureAwait(false);
                inputStream.Position = 0;

                using (Stream compressionStream = compressorFactory(outputStream, (CompressionLevel)compressionLevel))
                {
                    await inputStream.CopyToAsync(compressionStream).ConfigureAwait(false);
                }
            }
            outputStream.Position = 0;
            return outputStream;
        }

        private static MemoryStream GetStream(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new System.ArgumentException($"'{nameof(input)}' cannot be null or empty.", nameof(input));
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(input));
        }


    }
}