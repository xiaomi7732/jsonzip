using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Json.Zip.Tests
{
    public class SerializerTests
    {
        [Fact]
        public async Task ShouldRoundTripAsync()
        {
            Model model = new Model()
            {
                Id = 100,
            };

            Model deserialized = null;
            using (Stream outputStream = await JsonZipSerializer.Instance.SerializeAsync(model).ConfigureAwait(false))
            {
                deserialized = await JsonZipSerializer.Instance.DeserializeAsync<Model>(outputStream).ConfigureAwait(false);
            }

            Assert.NotNull(deserialized);
            Assert.Equal(model.Id, deserialized.Id);
        }

        [Fact]
        public async Task ShouldAllowSwapCompressor()
        {
            Model model = new Model()
            {
                Id = 100,
            };

            Model deserialized = null;
            using (Stream outputStream = await JsonZipSerializer.Instance.SerializeAsync(model, (int)CompressionLevel.Optimal, compressorFactory: (outStream, level) => new GZipStream(outStream, level, leaveOpen: true)).ConfigureAwait(false))
            {
                deserialized = await JsonZipSerializer.Instance.DeserializeAsync<Model>(outputStream, decompressFactory: (outStream) => new GZipStream(outStream, CompressionMode.Decompress), leaveOpen: false).ConfigureAwait(false);
            }

            Assert.NotNull(deserialized);
            Assert.Equal(model.Id, deserialized.Id);
        }

        [Fact]
        public async Task ShouldWriteFile()
        {
            List<Model> list = new List<Model>();
            for (var i = 0; i < 100; i++)
            {
                Model model = new Model()
                {
                    Id = 100,
                    DOB = DateTime.Now,
                    Name = "Alice" + i.ToString()
                };
                list.Add(model);
            }

            string fileName = "testoutput.json.compressed";
            await JsonZipSerializer.Instance.SerializeAsync(list, fileName).ConfigureAwait(false);

            Assert.True(File.Exists(fileName));

            TryDeleteFile(fileName);
        }

        [Fact]
        public async Task ShouldReadFile()
        {
            List<Model> list = new List<Model>();
            for (var i = 0; i < 100; i++)
            {
                Model model = new Model()
                {
                    Id = 100,
                    DOB = DateTime.Now,
                    Name = "Alice" + i.ToString()
                };
                list.Add(model);
            }

            string fileName = "testinput.json.compressed";
            await JsonZipSerializer.Instance.SerializeAsync(list, fileName).ConfigureAwait(false);

            try
            {
                list = null;
                list = await JsonZipSerializer.Instance.DeserializeAsync<List<Model>>(fileName).ConfigureAwait(false);
                Assert.NotNull(list);
            }
            finally
            {
                TryDeleteFile(fileName);
            }
        }

        [Fact]
        public async Task ShouldCompressTheContent()
        {
            List<Model> list = new List<Model>();
            for (var i = 0; i < 100; i++)
            {
                Model model = new Model()
                {
                    Id = 100,
                    DOB = DateTime.Now,
                    Name = "Alice" + i.ToString()
                };
                list.Add(model);
            }

            using (MemoryStream uncompressed = new MemoryStream())
            using (Stream compressed = await JsonZipSerializer.Instance.SerializeAsync(list).ConfigureAwait(false))
            using (Stream compressedGZip = await JsonZipSerializer.Instance.SerializeAsync(list, (int)CompressionLevel.Optimal, compressorFactory: (outStream, level) => new GZipStream(outStream, level, leaveOpen: true)))
            {
                await JsonSerializer.SerializeAsync(uncompressed, list).ConfigureAwait(false);

                Assert.True(compressed.Length < uncompressed.Length);
                Assert.True(compressedGZip.Length < uncompressed.Length);
            }
        }

        private void TryDeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex) when (ex is IOException)
            {
                System.Console.WriteLine("Can't delete file: " + path);
            }
        }
    }
}