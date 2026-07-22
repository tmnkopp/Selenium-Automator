using Aspose.Slides.Warnings;
using Atlas.Client;
using CyberBalance.CS;
using CyberBalance.CS.Core.JSON.FedRampAto;
using CyberBalance.VB.Core;
 
using DocumentFormat.OpenXml.Office.Word;
using Duende.IdentityModel; // or Duende.IdentityServer.Models
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit; 
 

namespace CyberScope.Tests.JSON
{

 
    /// <summary>
    /// A static generic helper that caches a compiled mapping function for type T.
    /// This avoids using Reflection (Slow) inside loops by using Expression Trees (Fast).
    /// </summary>
    public static class DataTableMapper<T>
    {
        // A simple struct to hold column metadata since we are in C# 7.3
        private struct ColumnInfo
        {
            public string Name;
            public Type Type;
        }

        // This is our 'Magic' function: It takes an object of type T and returns an object[] for a DataRow
        private static readonly Func<T, object[]> _rowMapper;

        // Stores the column names and types derived from your custom attributes
        private static readonly List<ColumnInfo> _columns = new List<ColumnInfo>();

        /// <summary>
        /// Static constructor: Runs exactly once per Type T when the class is first accessed.
        /// </summary>
        static DataTableMapper()
        {
            _rowMapper = CreateMapper();
        }

        private static Func<T, object[]> CreateMapper()
        {
            // 1. Find all properties decorated with [ImportDataMappingAttribute]
            PropertyInfo[] properties = typeof(T).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(FieldMapperAttribute)))
                .ToArray();

            // 2. Define the input parameter for our function: (T item) => ...
            ParameterExpression parameter = Expression.Parameter(typeof(T), "item");

            // This list will hold the "instructions" for fetching each property value
            List<Expression> expressions = new List<Expression>();

            foreach (PropertyInfo prop in properties)
            {
                var attr = (FieldMapperAttribute)Attribute.GetCustomAttribute(prop, typeof(FieldMapperAttribute));
                bool isStringArray = prop.PropertyType == typeof(string[]);

                // Define the DataTable column schema
                _columns.Add(new ColumnInfo
                {
                    Name = attr.ColumnName,
                    Type = isStringArray ? typeof(string) : prop.PropertyType
                });

                // Instruction: item.PropertyName
                Expression propertyAccess = Expression.Property(parameter, prop);
                Expression valueExpression;

                if (isStringArray)
                {
                    // Logic: string.Join(", ", item.PropertyName)
                    MethodInfo joinMethod = typeof(string).GetMethod("Join", new[] { typeof(string), typeof(string[]) });
                    Expression joinCall = Expression.Call(joinMethod, Expression.Constant(", "), propertyAccess);
                    // 3. Build the condition: (item.PropertyName == null) ? (object)DBNull.Value : (object)joinCall
                    // We cast both to object so the ternary operator has matching types
                    valueExpression = Expression.Condition(
                        Expression.Equal(propertyAccess, Expression.Constant(null, typeof(string[]))),
                        Expression.Constant(DBNull.Value, typeof(object)),
                        Expression.Convert(joinCall, typeof(object))
                    );
                }
                else
                {
                    // Instruction: (object)item.PropertyName
                    // We convert to object so all properties fit into the object[] array
                    valueExpression = Expression.Convert(propertyAccess, typeof(object));
                }

                // Logic: value ?? DBNull.Value
                // DataTables require DBNull.Value instead of C# null for database compatibility
                ConstantExpression dbNull = Expression.Constant(DBNull.Value, typeof(object));
                valueExpression = Expression.Coalesce(valueExpression, dbNull);
                 
                expressions.Add(valueExpression);
            }

            // 3. Instruction: new object[] { result1, result2, ... }
            NewArrayExpression arrayInit = Expression.NewArrayInit(typeof(object), expressions);

            // 4. Compile the instructions into a real, executable function (Delegate)
            // This is the expensive part, but it only happens once!
            return Expression.Lambda<Func<T, object[]>>(arrayInit, parameter).Compile();
        }

        /// <summary>
        /// Converts a collection of items into a DataTable using the cached mapper.
        /// </summary>
        public static DataTable Fill(IEnumerable<T> items)
        {
            DataTable dt = new DataTable();

            // Create the columns once
            foreach (ColumnInfo col in _columns)
            {
                dt.Columns.Add(col.Name, col.Type);
            }

            // BeginLoadData turns off index maintenance and constraints for better speed
            dt.BeginLoadData();

            foreach (T item in items)
            {
                // Execute the pre-compiled function (Near-native speed)
                try
                {
                    dt.Rows.Add(_rowMapper(item));
                }
                catch (Exception ex)
                { 
                    throw ex;
                } 
            }

            dt.EndLoadData();

            return dt;
        }
    }

    [SprocMapperAttribute("spATOFedRampData", "@ATODataTable", "FedRampATOType")]
    public class TestItem
    {
        [FieldMapper("fedramp_id", "FEDRAMPID")]
        public string fedramp_id { get; set; }

        [FieldMapper("cloud_service_provider", "CloudServiceProvider")]
        public string cloud_service_provider { get; set; } 

        [FieldMapper("service_model", "ServiceModel")]
        public string[] service_model { get; set; }

        [FieldMapper("status", "AuthorizationStatus")]
        public string status { get; set; } 
    }

     
    public class FedRAMPTests
    {
        [Fact]
        public void Fill_ShouldMapPropertiesToDataTableCorrecty()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem
                {
                    fedramp_id = "101",
                    cloud_service_provider = "Azure",
                    service_model = new[] { "IaaS", "SaaS" },
                    status = "Active"
                },
                new TestItem
                {
                    fedramp_id = "102",
                    cloud_service_provider = "AWS",
                    service_model = null, // Testing null array
                    status = null         // Testing null string
                }
            };

            // Act
            DataTable dt = DataTableMapper<TestItem>.Fill(items);

            // Assert - Schema
            Assert.Equal(4, dt.Columns.Count);
            Assert.Equal("FEDRAMPID", dt.Columns[0].ColumnName);
            Assert.Equal("CloudServiceProvider", dt.Columns[1].ColumnName);
            Assert.Equal("ServiceModel", dt.Columns[2].ColumnName);
            Assert.Equal(typeof(string), dt.Columns["ServiceModel"].DataType);

            // Assert - Data Row 1 (Flattening check)
            DataRow row1 = dt.Rows[0];
            Assert.Equal("101", row1["FEDRAMPID"]);
            Assert.Equal("IaaS, SaaS", row1["ServiceModel"]); // Verified joined string

            // Assert - Data Row 2 (Null/DBNull check)
            DataRow row2 = dt.Rows[1];
            Assert.Equal("102", row2["FEDRAMPID"]);
            Assert.Equal(DBNull.Value, row2["AuthorizationStatus"]);
            Assert.Equal(DBNull.Value, row2["ServiceModel"]);
        }

        /// <summary>
        /// Tests that GetToken returns a valid token using mocked Atlas client.
        /// </summary>
        [Fact]
        public async Task FedRAMPTest_Inserter_INSERTS()
        {
            // Pass the URL directly to avoid WebConfigurationManager requiring an ASP.NET web context
            var fedRampAtoFeedProvider = new FedRampAtoFeedProvider(FedRampAtoFeedProvider.DefaultFedRampAtoAPIBaseUrl);
            RootAtoObject atoJson = fedRampAtoFeedProvider.Provide(); 
            // Converts the array to a List<T>
            List<FedRampAtoItem> atos = atoJson.data.ATOs.ToList();
            Assert.True(atos.Count > 0, "atos should items"); 
            // Pass the list to your generic importer
            var fedRampParser = new JsonDbTypeImporter<FedRampAtoItem>(atos);
            Assert.NotNull(fedRampParser);
        } 
    } 
}




