using Azure.Storage.Blobs;
using CyberBalance.CS.Core.Reports;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CyberScope.Tests.Automator.Tests
{
    public class ArchiveTests
    {
        /// <summary>
        /// Tests that BlobJsonExtractor can extract JSON content from a blob byte array.
        /// Uses mocked data instead of a real database connection.
        /// </summary>
        [Fact]
        public void BlobJsonExtractor_ExtractsJsonFromBlob()
        {
            // Arrange - Create a mock blob with header + JSON content
            // Header format: 6 bytes where byte[4] = extension length, byte[5] = name length
            string expectedJson = @"{""MetaData"":{""Description"":""Test Metadata""},""Data"":""test""}";
            byte[] jsonBytes = Encoding.UTF8.GetBytes(expectedJson);

            // Create header: 6 bytes with extLen=0, nameLen=0 (no extension or name to skip)
            byte[] header = new byte[] { 0, 0, 0, 0, 0, 0 };

            // Combine header + JSON content
            byte[] blobData = new byte[header.Length + jsonBytes.Length];
            Array.Copy(header, 0, blobData, 0, header.Length);
            Array.Copy(jsonBytes, 0, blobData, header.Length, jsonBytes.Length);

            // Act - Extract JSON from the blob
            string jsonContent = BlobJsonExtractor.ExtractJsonFromBlob(blobData);

            // Assert
            Assert.NotNull(jsonContent);
            Assert.Contains("MetaData", jsonContent);

            // Parse and verify JSON structure
            JObject jsonObject = JObject.Parse(jsonContent);
            string description = jsonObject["MetaData"]["Description"].ToString();
            Assert.Equal("Test Metadata", description);
        }

  

        [Fact]
        public void BlobJsonExtractor_ExtractsJsonWithHeaderData()
        {
            // Arrange - Create blob with extension and name in header
            string expectedJson = @"{""key"":""value""}";
            byte[] jsonBytes = Encoding.UTF8.GetBytes(expectedJson);

            // Header: extLen=4 (.zip), nameLen=8 (test.zip)
            byte extLen = 4;
            byte nameLen = 8;
            byte[] header = new byte[] { 0, 0, 0, 0, extLen, nameLen };
            byte[] extBytes = Encoding.UTF8.GetBytes(".zip");
            byte[] nameBytes = Encoding.UTF8.GetBytes("test.zip");

            // Combine all parts
            byte[] blobData = new byte[header.Length + extLen + nameLen + jsonBytes.Length];
            int offset = 0;
            Array.Copy(header, 0, blobData, offset, header.Length);
            offset += header.Length;
            Array.Copy(extBytes, 0, blobData, offset, extLen);
            offset += extLen;
            Array.Copy(nameBytes, 0, blobData, offset, nameLen);
            offset += nameLen;
            Array.Copy(jsonBytes, 0, blobData, offset, jsonBytes.Length);

            // Act
            string jsonContent = BlobJsonExtractor.ExtractJsonFromBlob(blobData);

            // Assert
            Assert.NotNull(jsonContent);
            JObject jsonObject = JObject.Parse(jsonContent);
            string value = jsonObject["key"].ToString();
            Assert.Equal("value", value);
        }

        /// <summary>
        /// Tests that BlobJsonExtractor throws when given null input.
        /// </summary>
        [Fact]
        public void BlobJsonExtractor_ThrowsOnNullInput()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => BlobJsonExtractor.ExtractJsonFromBlob(null));
        }

        // Helper method for Azure Blob upload - kept for reference but not tested against real Azure
        public static async Task UploadFileToBlobAsync(string connectionString, string containerName, string localFilePath)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            string fileName = Path.GetFileName(localFilePath);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            using (FileStream uploadFileStream = File.OpenRead(localFilePath))
            {
                await blobClient.UploadAsync(uploadFileStream, true);
            }

            Console.WriteLine($"File '{fileName}' uploaded to Azure Blob Storage in container '{containerName}'.");
        }
    }
}
