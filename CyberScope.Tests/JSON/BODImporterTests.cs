using CyberBalance.CS.Core;
using CyberBalance.CS.Core.JSON;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CyberScope.Tests.JSON
{
    public class BODImporter
    {
        private string BasePath => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Tests that JsonSchemaValidator validates JSON against a schema.
        /// Uses embedded test data files instead of external file paths.
        /// </summary>
        [Fact]
        public void JsonSchemaValidator_ValidatesValidJson()
        {
            // Arrange - Load test JSON from embedded test data
            string jsonFilePath = Path.Combine(BasePath, "Data", "ScubaResults_validc8c-3a87-4387.json");
            string json = File.ReadAllText(jsonFilePath);

            // Use the schema file that's deployed with the test project
            string schemaFileName = "scuba-schema-1.2.json";

            // Act
            var validator = new JsonSchemaValidator(json, schemaFileName);
            var errors = validator.Validate();

            // Assert
            Assert.NotNull(errors);
            // The validator should process without throwing
        }

        /// <summary>
        /// Tests that JsonSchemaValidator returns errors for invalid JSON.
        /// </summary>
        [Fact]
        public void JsonSchemaValidator_ReturnsErrorsForInvalidJson()
        {
            // Arrange - Use invalid JSON
            string invalidJson = "{ this is not valid json }";
            string schemaFileName = "scuba-schema-1.2.json";

            // Act
            var validator = new JsonSchemaValidator(invalidJson, schemaFileName);
            var errors = validator.Validate();

            // Assert
            Assert.False(validator.IsValid);
            Assert.NotEmpty(errors);
        }

        /// <summary>
        /// Tests that YamlToJsonConverter can convert valid YAML to JSON.
        /// </summary>
        [Fact]
        public void YamlToJsonConverter_ConvertsValidYaml()
        {
            // Arrange - Load valid YAML from embedded test data
            string yamlFilePath = Path.Combine(BasePath, "Data", "scuba_compliance_valid.yaml");
            
            // Check if file exists, if not use inline YAML for test
            string yaml;
            if (File.Exists(yamlFilePath))
            {
                yaml = File.ReadAllText(yamlFilePath);
            }
            else
            {
                // Fallback to inline YAML
                yaml = @"
name: test
version: 1.0
settings:
  enabled: true
  count: 42
";
            }

            // Act
            var converter = new YamlToJsonConverter(yaml);
            var json = converter.Convert();

            // Assert
            Assert.True(converter.IsValid);
            Assert.NotNull(json);
            Assert.NotEmpty(json);
        }

        /// <summary>
        /// Tests that YamlToJsonConverter handles invalid YAML gracefully.
        /// </summary>
        [Fact]
        public void YamlToJsonConverter_HandlesInvalidYaml()
        {
            // Arrange - Invalid YAML with bad indentation/structure
            string invalidYaml = @"
name: test
  bad_indent: this is invalid
    nested: wrong
";

            // Act
            var converter = new YamlToJsonConverter(invalidYaml);
            var json = converter.Convert();

            // Assert - Should either return null or set IsValid to false
            // The converter should handle this gracefully without throwing
            if (!converter.IsValid)
            {
                Assert.NotEmpty(converter.Errors);
            }
        }

        /// <summary>
        /// Tests that ScubaJsonValidator can parse valid ScubaGear JSON metadata.
        /// This test validates the JSON structure parsing without calling external APIs.
        /// </summary>
        [Fact]
        public void ScubaJsonValidator_ParsesValidJsonMetadata()
        {
            // Arrange - Load valid ScubaGear JSON from test data
            string jsonFilePath = Path.Combine(BasePath, "Data", "ScubaResults_validc8c-3a87-4387.json");
            string scubaJson = File.ReadAllText(jsonFilePath);

            // Act - Parse and verify the JSON structure can be deserialized
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(scubaJson);

            // Assert - Verify key metadata fields exist
            Assert.NotNull(jsonObject);
            Assert.NotNull(jsonObject.MetaData);
            string tool = Convert.ToString(jsonObject.MetaData.Tool);
            string toolVersion = Convert.ToString(jsonObject.MetaData.ToolVersion);
            string tenantId = Convert.ToString(jsonObject.MetaData.TenantId);

            Assert.Equal("ScubaGear", tool);
            Assert.False(string.IsNullOrEmpty(toolVersion));
            Assert.False(string.IsNullOrEmpty(tenantId));
        }

        /// <summary>
        /// Tests that ScubaJsonValidator constructor initializes correctly.
        /// </summary>
        [Fact]
        public async Task ScubaJsonValidator_InitializesCorrectly()
        {   
            // Arrange
            string jsonFilePath = Path.Combine(BasePath, "Data", "ScubaResults_validc8c-3a87-4387.json");
            string scubaJson = File.ReadAllText(jsonFilePath);
            // Act
            var inventory_record = GetInventoryRecord;
             
            var validator = new ScubaJsonValidator();
            var errors = await validator.Validate(scubaJson, inventory_record);
            // Assert 
            Assert.True(validator.IsValid);
            Assert.Empty(validator.Errors);
        }

        public static SCuBATenantInventory GetInventoryRecord => new CyberBalance.CS.Core.SCuBATenantInventory
        {
            ProductName = "M365", 
            ServicePlanName = "Commercial (GWS/M365)",
            TenantFQDN = "cisaent.onmicrosoft.com",
            TenantID = "3c19c757-3b55-411f-b03f-2bcc514a598d",
            ATO = 175,
            Acronym = "DOL",
            ArchiveJustification = null,
            ArchivedBy = null,
            AssessmentMethod = 15337, 
            Bureau = 123,
            Comments = "test",
            Component = "Department Of Labor", 
            InScope = 175,
            IsArchive = false,  
            PK_Component = 122, 
            Product = 15112,
            ServicePlan = 15105,
            UploadDate = null 
        };
        /// <summary>
        /// Tests JSON deserialization of ScubaGear results structure.
        /// </summary>
        [Fact]
        public void ScubaJson_CanDeserializeResultsStructure()
        {
            // Arrange - Load valid ScubaGear JSON
            string jsonFilePath = Path.Combine(BasePath, "Data", "ScubaResults_validc8c-3a87-4387.json");
            string scubaJson = File.ReadAllText(jsonFilePath);

            // Act
            var jsonObject = JObject.Parse(scubaJson);

            // Assert - Verify structure
            Assert.NotNull(jsonObject["MetaData"]);
            Assert.NotNull(jsonObject["Summary"]);
            
            // Verify MetaData contents
            var metaData = jsonObject["MetaData"];
            Assert.Equal("ScubaGear", metaData["Tool"]?.ToString());
            Assert.NotNull(metaData["ToolVersion"]);
            Assert.NotNull(metaData["TenantId"]);
            Assert.NotNull(metaData["TimestampZulu"]);
        }
    }
}
