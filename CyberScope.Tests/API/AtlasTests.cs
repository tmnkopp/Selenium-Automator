using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Atlas.Client; 
using Duende.IdentityModel; // or Duende.IdentityServer.Models
namespace CyberScope.Tests.JSON
{
    public class AtlasTests
    {
        private string UserIdToIssue = "1169";

        /// <summary>
        /// Tests that GetToken returns a valid token using mocked Atlas client.
        /// </summary>
        [Fact]
        public async Task Atlas_GetToken_ReturnsToken_WithMock()
        {
            // Arrange
            var mockToken = "mock-access-token";

            var mockOptions = new Mock<IPersonalAccessTokenOptions>();
            mockOptions.Setup(o => o.UserClaimType).Returns("urn:cscope:aa:user");
            mockOptions.Setup(o => o.ApiDefaultScopes).Returns(new[] { "urn:csam:aa:api:reports", "urn:csam:aa:api:systems" });

            var mockAtlasClient = new Mock<IAtlasClient<IPersonalAccessTokenOptions>>();
            mockAtlasClient.Setup(a => a.Options).Returns(mockOptions.Object);
            mockAtlasClient.Setup(a => a.GetToken()).ReturnsAsync(mockToken);

            // Act
            var token = await mockAtlasClient.Object.GetToken();

            // Assert
            Assert.NotNull(token);
            Assert.Equal(mockToken, token);
            mockAtlasClient.Verify(a => a.GetToken(), Times.Once());
        }

        /// <summary>
        /// Tests that DeleteClient (revoke) is called successfully using mocked Atlas client.
        /// </summary>
        [Fact]
        public async Task Atlas_RevokeToken_CallsDeleteClient_WithMock()
        {
            // Arrange
            var clientId = "d6209959-5ada-49b7-9ff7-cbfccaf247ed";
            var mockToken = "mock-access-token";

            var mockOptions = new Mock<IPersonalAccessTokenOptions>();
            mockOptions.Setup(o => o.UserClaimType).Returns("urn:cscope:aa:user");

            var mockAtlasClient = new Mock<IAtlasClient<IPersonalAccessTokenOptions>>();
            mockAtlasClient.Setup(a => a.Options).Returns(mockOptions.Object);
            mockAtlasClient.Setup(a => a.GetToken()).ReturnsAsync(mockToken);
            mockAtlasClient.Setup(a => a.DeleteClient(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var token = await mockAtlasClient.Object.GetToken();
            await mockAtlasClient.Object.DeleteClient(token, clientId, CancellationToken.None);

            // Assert
            Assert.NotNull(token);
            mockAtlasClient.Verify(a => a.DeleteClient(mockToken, clientId, It.IsAny<CancellationToken>()), Times.Once());
        }

        /// <summary>
        /// Tests PersonalAccessTokenOptions can be created with expected values.
        /// </summary>
        [Fact]
        public void PersonalAccessTokenOptions_CanBeCreated()
        {
            // Arrange & Act
            var options = new PersonalAccessTokenOptions
            {
                Address = "https://test.example.com/atlas",
                ClientId = "urn:cscope:aa:api",
                ClientSecret = "secret",
                ClientScopes = new[] { "IdentityServerApi urn:atlas:api" },
                UserClaimType = "urn:cscope:aa:user",
                ApiDefaultScopes = new[] { "urn:csam:aa:api:reports", "urn:csam:aa:api:systems" },
                MaximumSecretLifetime = 6000
            };

            // Assert
            Assert.Equal("https://test.example.com/atlas", options.Address);
            Assert.Equal("urn:cscope:aa:api", options.ClientId);
            Assert.Equal("secret", options.ClientSecret);
            Assert.Equal("urn:cscope:aa:user", options.UserClaimType);
            Assert.Equal(6000, options.MaximumSecretLifetime);
            Assert.Equal(2, options.ApiDefaultScopes.Count());
        }

        /// <summary>
        /// Tests that MakePersonalAccessToken extension creates a valid token object.
        /// </summary>
        [Fact]
        public void PersonalAccessToken_CanBeCreatedWithOptions()
        {
            // Arrange
            var options = GetPersonalAccessTokenOptions();
            var lifetime = TimeSpan.FromSeconds(600);
            var scopes = new[] { "urn:csam:aa:api:reports", "urn:csam:aa:api:systems" };

            // Act
            var pat = options.MakePersonalAccessToken(null, "test-pat", UserIdToIssue, lifetime, scopes);

            // Assert
            Assert.NotNull(pat);
            Assert.Equal("test-pat", pat.Name);
        }
        [Fact]
        public void Secret_HASHES()
        {
            // Arrange
            var secret = "f91b3e72-05dc-4a87-b263-7e4d1c9f8a30";
            var plainText = secret; // GUID WITH HYTPHENS 
            var hashed = plainText.ToSha256();
            Console.WriteLine(hashed); // this equals the DB Hashed value 

            //"EncryptedSecret":"mu9+pJ2OWbiLqdzzXOGX9Up6pEiKIzeYsbV8ydQxxnI="
        }

        #region Helper Methods

        private IPersonalAccessTokenOptions GetPersonalAccessTokenOptions() => new PersonalAccessTokenOptions
        {
            Address = "https://test.example.com/atlas",
            ClientId = "urn:cscope:aa:api",
            ClientSecret = "secret",
            ClientScopes = new[] { "IdentityServerApi urn:atlas:api" },
            UserClaimType = "urn:cscope:aa:user",
            ApiDefaultScopes = new[] { "urn:csam:aa:api:reports", "urn:csam:aa:api:systems" },
            MaximumSecretLifetime = 6000
        };

        #endregion
    }
}
