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
        private const int DefaultDecompressionLevel = 5;
        private JsonZipSerializer()
        {
        }
        public static JsonZipSerializer Instance { get; } = new JsonZipSerializer();

        public async Task<T> DeserializeAsync<T>(Stream inputStream, bool keepOpen = false, CancellationToken cancellationToken = default)
        {
            T result = default;
            using (MemoryStream output = new MemoryStream())
            using (BrotliStream decompressor = new BrotliStream(inputStream, CompressionMode.Decompress))
            {
                await decompressor.CopyToAsync(output).ConfigureAwait(false);
                result = await JsonSerializer.DeserializeAsync<T>(output, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            if (!keepOpen)
            {
                inputStream.Close();
            }
            return result;
        }


        public async Task<T> DeserializeAsync<T>(string inputFilePath, CancellationToken cancellationToken = default)
        {
            T result = default;
            using (FileStream input = File.OpenRead(inputFilePath))
            {
                result = await DeserializeAsync<T>(input, keepOpen: true, cancellationToken);
            }
            return result;
        }

        public async Task SerializeAsync<T>(T obj, string outputFilePath, int compressionLevel = DefaultDecompressionLevel, CancellationToken cancellationToken = default)
        {
            using (Stream input = await SerializeAsync(obj, compressionLevel, cancellationToken).ConfigureAwait(false))
            using (FileStream output = File.Create(outputFilePath))
            {
                await input.CopyToAsync(output);
            }
        }

        public async Task<Stream> SerializeAsync<T>(T obj, int compressionLevel = DefaultDecompressionLevel, CancellationToken cancellationToken = default)
        {
            MemoryStream outputStream = new MemoryStream();
            using (MemoryStream inputStream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(inputStream, obj, cancellationToken: cancellationToken).ConfigureAwait(false);
                using (BrotliStream compressionStream = new BrotliStream(outputStream, (CompressionLevel)compressionLevel))
                {
                    await inputStream.CopyToAsync(compressionStream).ConfigureAwait(false);
                }
            }
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