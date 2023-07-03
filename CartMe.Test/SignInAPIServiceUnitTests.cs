using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CartMe.Test
{
    [TestClass]
    public class SignInAPIServiceUnitTests
    {
        private HttpClient httpClient;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize the HttpClient
            httpClient = new HttpClient();
            // Set up the base address of your web API
            httpClient.BaseAddress = new Uri("https://localhost:7180");
        }

        [TestMethod]
        [DataRow("", "", HttpStatusCode.BadRequest)]
        [DataRow("abc", "abc", HttpStatusCode.BadRequest)]
        [DataRow("abc@", "abc", HttpStatusCode.BadRequest)]
        [DataRow("abc@abc", "abc", HttpStatusCode.NotFound)]
        [DataRow("shehroz.munir@powersoft19.com", "abc", HttpStatusCode.InternalServerError)]
        [DataRow("shehroz@powersoft19.com", "abc", HttpStatusCode.Forbidden)]
        [DataRow("shehroz.munir@powersoft19.com", "Welcome@123", HttpStatusCode.OK)]
        public async Task SignInAPIServiceUnitTest(string email, string password, HttpStatusCode statusCode)
        {
            try
            {
                var user = new LogInUserAPIServiceUnitTests()
                {
                    Email = email,
                    Password = password
                };

                // Mock the HttpClient
                var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
                mockHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<System.Threading.CancellationToken>()
                    )
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = statusCode,
                        Content = new StringContent(GetJsonResponse(statusCode))
                    });

                var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object)
                {
                    BaseAddress = new Uri("https://localhost:7180")
                };

                var apiService = new APIService(mockHttpClient);

                // Perform the API call
                var response = await apiService.Login(user);

                // Verify the response
                Assert.AreEqual(statusCode, response.StatusCode);

                var contents = await response.Content.ReadAsStringAsync();
                var a = 10;
            }
            catch (Exception ex)
            {
                // Handle any exception that occurs during the test
                Assert.Fail($"Exception occurred: {ex.Message}");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up resources
            httpClient.Dispose();
        }

        private string GetJsonResponse(HttpStatusCode statusCode)
        {
            var jsonData = new JSonDataSignInAPIServiceUnitTests
            {
                Type = "Error",
                Title = "API Error",
                Status = (int)statusCode,
                TraceId = Guid.NewGuid().ToString(),
                Errors = new ErrorDataSignInAPIServiceUnitTests()
            };

            return JsonConvert.SerializeObject(jsonData);
        }
    }

    public class LogInUserAPIServiceUnitTests
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class JSonDataSignInAPIServiceUnitTests
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
        public ErrorDataSignInAPIServiceUnitTests Errors { get; set; }
    }

    public class ErrorDataSignInAPIServiceUnitTests
    {
        public string[] Email { get; set; }
        public string[] Password { get; set; }
    }

    public class APIService
    {
        private readonly HttpClient httpClient;

        public APIService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> Login(LogInUserAPIServiceUnitTests user)
        {
            return await httpClient.PostAsJsonAsync("api/User/Login", user);
        }
    }
}
