using CyberBalance.CS.Core.Document.Spreadsheet;
using CyberBalance.CS.Core.Reports;
using Newtonsoft.Json.Linq;
using SpreadsheetLight;
using System;
using System.Data;
using System.IO;
using Xunit;

namespace CyberScope.Tests
{
    public class BlobExtractor_TESTS
    {
        /// <summary>
        /// Tests that BlobXlsxCombiner.CombineAllSheetsToOne correctly merges xlsx blobs
        /// and that SheetToJSON produces valid JSON with the expected row data.
        /// Uses an in-memory xlsx blob so no database connection is required.
        /// </summary>
        [Fact]
        public void xlsxExtractor_Extracts()
        {
            // Arrange - build an xlsx blob in memory with known headers and one data row
            byte[] xlsxBytes;
            using (var src = new SLDocument())
            {
                src.RenameWorksheet(src.GetCurrentWorksheetName(), "Sheet1");
                src.SetCellValue(1, 1, "ID");
                src.SetCellValue(1, 2, "Name");
                src.SetCellValue(2, 1, "1");
                src.SetCellValue(2, 2, "TestRow");
                using (var ms = new MemoryStream())
                {
                    src.SaveAs(ms);
                    xlsxBytes = ms.ToArray();
                }
            }

            // Prepend the 6-byte blob header that StripBlobHeader expects
            // bytes[4] = extLen (0), bytes[5] = nameLen (0) => skip = 6
            byte[] header = new byte[] { 0, 0, 0, 0, 0, 0 };
            byte[] blobData = new byte[header.Length + xlsxBytes.Length];
            Array.Copy(header, 0, blobData, 0, header.Length);
            Array.Copy(xlsxBytes, 0, blobData, header.Length, xlsxBytes.Length);

            // Build the DataTable with the columns CombineAllSheetsToOne reads
            var dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("FileName", typeof(string));
            dt.Columns.Add("FileData", typeof(byte[]));
            dt.Rows.Add(1, "test.xlsx", blobData);

            // Act
            SLDocument combined = BlobXlsxCombiner.CombineAllSheetsToOne(dt);

            // Assert - the combined sheet has at least the 2 rows and 2 columns we wrote
            SLWorksheetStatistics sts = combined.GetWorksheetStatistics();
            Assert.True(sts.NumberOfRows >= 2, $"Expected >= 2 rows, got {sts.NumberOfRows}");
            Assert.True(sts.NumberOfColumns >= 2, $"Expected >= 2 columns, got {sts.NumberOfColumns}");

            // Assert - SheetToJSON produces valid JSON containing our data row
            string json = combined.SheetToJSON(SheetName: combined.GetCurrentWorksheetName());
            Assert.NotNull(json);
            var rows = JArray.Parse(json);
            Assert.NotEmpty(rows);
            Assert.Equal("TestRow", rows[0]["Name"]?.ToString());
        }
    }
}
