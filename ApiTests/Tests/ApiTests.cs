using ApiTests.Configuration;
using ApiTests.Constants;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ApiTests.Tests
{
    public class ApiTests
    {
        private readonly HttpClient _httpClient;
        private static readonly string GitHubUsername = ConfigurationLoader.Settings.GitHubUsername;
        private static readonly string GitHubToken = ConfigurationLoader.Settings.GitHubToken;
        private static readonly string GitHubApiUrl = ConfigurationLoader.Settings.GitHubApiUrl;

        public ApiTests()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(GitHubApiUrl)
            };
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", GitHubToken);
        }

        [Test]
        public async Task Test_Post_Request()
        {
            // Create POST repo request
            var requestBody = new
            {
                Name = "TestingGitApiRepo",
                Description = "This is a test repository",
                Private = false
            };

            var response = await _httpClient.PostAsJsonAsync(EndpointsConstants.CreateRepository, requestBody);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(content);

            // Validate response data
            jsonResponse[RequestResponseConstants.Name].ToString().Should().Be(requestBody.Name);
            jsonResponse[RequestResponseConstants.Description].ToString().Should().Be(requestBody.Description);
            jsonResponse[RequestResponseConstants.Private].ToObject<bool>().Should().BeFalse();

            // Clean up (delete the test repo)
            var deleteResponse = await _httpClient.DeleteAsync(EndpointsConstants.GetDeleteRepository(GitHubUsername, requestBody.Name));
            deleteResponse.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task Test_Error_Handling_NoRepoName_PostRequest()
        {
            // Create POST repo request
            var requestBody = new
            {
                Name = "",
                Description = "This is a test repository",
                Private = false
            };

            var response = await _httpClient.PostAsJsonAsync(EndpointsConstants.CreateRepository, requestBody);
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)422);

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(content);

            // Validate response data
            jsonResponse[RequestResponseConstants.Message].ToString().Should().Contain("Repository creation failed.");
        }

        [Test]
        public async Task Test_Error_Handling_AddRepoWithTheSameName_PostRequest()
        {
            // Create POST repo request
            var requestBody = new
            {
                Name = "Test repo",
                Description = "This is a test repository",
                Private = false
            };
            var response = await _httpClient.PostAsJsonAsync(EndpointsConstants.CreateRepository, requestBody);

            // Attempt to create repo with the same name
            var secondResponse = await _httpClient.PostAsJsonAsync(EndpointsConstants.CreateRepository, requestBody);
            secondResponse.StatusCode.Should().Be((System.Net.HttpStatusCode)422);

            var content = await secondResponse.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(content);

            // Validate response data
            jsonResponse[RequestResponseConstants.Message].ToString().Should().Contain("Repository creation failed.");

            // Clean up (delete the test repo)
            var deleteResponse = await _httpClient.DeleteAsync(EndpointsConstants.GetDeleteRepository(GitHubUsername, requestBody.Name));
            deleteResponse.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task Test_Get_Request()
        {
            // Create repo first
            var requestBody = new
            {
                Name = "TestingApiRepo",
                Description = "This is a test repository",
                Private = false
            };

            // Execute request
            await _httpClient.PostAsJsonAsync(EndpointsConstants.CreateRepository, requestBody);

            // Execute GET repo request
            var response = await _httpClient.GetAsync(EndpointsConstants.GetDeleteRepository(GitHubUsername, requestBody.Name));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(content);

            // Validate response data
            jsonResponse[RequestResponseConstants.FullName].ToString().Should().Be($"{GitHubUsername}/{requestBody.Name}");
            jsonResponse[RequestResponseConstants.Private].ToObject<bool>().Should().BeFalse();

            // Clean up (delete the test repo)
            var deleteResponse = await _httpClient.DeleteAsync(EndpointsConstants.GetDeleteRepository(GitHubUsername, requestBody.Name));
            deleteResponse.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task Test_Delete_Request()
        {
            // Create a repo to delete
            var requestBody = new
            {
                Name = "test-repo-to-delete",
                Description = "This repo will be deleted",
                @private = false
            };

            var createResponse = await _httpClient.PostAsJsonAsync(EndpointsConstants.CreateRepository, requestBody);
            createResponse.EnsureSuccessStatusCode();

            // Delete the repo
            var deleteResponse = await _httpClient.DeleteAsync(EndpointsConstants.GetDeleteRepository(GitHubUsername, requestBody.Name));
            deleteResponse.EnsureSuccessStatusCode();

            // Ensure repo is not found after deleting
            var response = await _httpClient.GetAsync(EndpointsConstants.GetDeleteRepository(GitHubUsername, requestBody.Name));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Test]
        public async Task Test_Error_Handling_Invalid_Input()
        {
            var response = await _httpClient.GetAsync(EndpointsConstants.GetDeleteRepository(GitHubUsername, "non-existent-repo"));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(content);

            // Validate error response
            jsonResponse[RequestResponseConstants.Message].ToString().Should().Contain("Not Found");
        }

        [Test]
        public async Task Test_Error_Handling_Rate_Limiting()
        {
            // Execute many requests
            for (int i = 0; i < 100; i++)
            {
                await _httpClient.GetAsync(EndpointsConstants.GetDeleteRepository(GitHubUsername, "TestingApiRepo"));
            }

            var response = await _httpClient.GetAsync(EndpointsConstants.GetDeleteRepository(GitHubUsername, "TestingApiRepo"));

            if (response.StatusCode == (System.Net.HttpStatusCode)403)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(content);

                // Validate rate limiting error
                jsonResponse[RequestResponseConstants.Message].ToString().Should().Contain("API rate limit exceeded");
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }
        }
    }
}