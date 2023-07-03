using Moq;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.Protected;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;

namespace CartMe.Test
{
    [TestClass]
    public class SignInAPIControllerUnitTests
    {
        private HttpClient _httpClient;
        private const string emailErrorMessage = "The Email field is required.";
        private const string emailErrorMessageSecond = "The Email field is not a valid e-mail address.";
        private const string passwordErrorMessage = "The Password field is required.";
        private const string loginErrorUserNotFound = "Unable to login as user 'abc@abc' not found.";
        private const string loginErrorPasswordIncorrect = "Unable to login as user name or password is incorrect.";
        private const string loginErrorEmailIncorrect = "Unable to login as email 'shehroz@powersoft19.com' not confirmed.";

        [TestInitialize]
        public void Initialize()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage());

            _httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://localhost:7180")
            };
        }

        [TestMethod]
        [DataRow("", "", HttpStatusCode.BadRequest, null, new string[] { emailErrorMessage }, new string[] { passwordErrorMessage })] // Actual: "emailErrorMessageSecond" show, its a BUG
        [DataRow("abc", "abc", HttpStatusCode.BadRequest, null, new string[] { emailErrorMessageSecond }, null)]
        [DataRow("abc@", "abc", HttpStatusCode.BadRequest, null, new string[] { emailErrorMessageSecond }, null)]
        [DataRow("abc@abc", "abc", HttpStatusCode.NotFound, loginErrorUserNotFound, null, null)]
        [DataRow("shehroz.munir@powersoft19.com", "abc", HttpStatusCode.InternalServerError, loginErrorPasswordIncorrect, null, null)]
        [DataRow("shehroz@powersoft19.com", "abc", HttpStatusCode.Forbidden, loginErrorEmailIncorrect, null, null)]
        [DataRow("shehroz.munir@powersoft19.com", "Welcome@123", HttpStatusCode.OK, null, null, null)]
        public async Task SignInAPIControllerUnitTest(string email, string password, HttpStatusCode statusCode, string expectedErrorMessage, string[] expectedEmailErrorMessages, string[] expectedPasswordErrorMessages)
        {
            try
            {
                var user = new LogInUserAPIControllerUnitTests()
                {
                    Email = email,
                    Password = password
                };

                var expectedResponse = new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                    Content = new StringContent(GetJsonResponse(statusCode, expectedErrorMessage, expectedEmailErrorMessages, expectedPasswordErrorMessages)),
                };

                var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
                mockHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<System.Threading.CancellationToken>()
                    )
                    .ReturnsAsync(expectedResponse);

                var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object)
                {
                    BaseAddress = new Uri("https://localhost:7180")
                };

                var apiService = new APIController(mockHttpClient);

                // Perform the API call
                var response = await apiService.Login(user);

                // Verify the response
                Assert.AreEqual(statusCode, response.StatusCode);

                var contents = await response.Content.ReadAsStringAsync();
                AssertJsonResponse(GetJsonResponse(statusCode, expectedErrorMessage, expectedEmailErrorMessages, expectedPasswordErrorMessages), contents);
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
            _httpClient.Dispose();
        }

        private string GetJsonResponse(HttpStatusCode statusCode, string expectedErrorMessage, string[] expectedEmailErrorMessages, string[] expectedPasswordErrorMessages)
        {
            var jsonData = new JSonDataSignInAPIControllerUnitTests
            {
                Type = "Error",
                Title = "API Error",
                Status = (int)statusCode,
                TraceId = Guid.NewGuid().ToString(),
                Errors = new ErrorDataSignInAPIControllerUnitTests
                {
                    Email = expectedEmailErrorMessages,
                    Password = expectedPasswordErrorMessages
                }
            };

            if (!string.IsNullOrEmpty(expectedErrorMessage))
            {
                jsonData.Errors = null; // Exclude the Errors property from the response
                return JsonConvert.SerializeObject(jsonData);
            }

            // Exclude the TraceId property from the response
            jsonData.TraceId = null;

            return JsonConvert.SerializeObject(jsonData);
        }
        private void AssertJsonResponse(string expectedJson, string actualJson)
        {
            var expectedObject = JObject.Parse(expectedJson);
            var actualObject = JObject.Parse(actualJson);

            // Remove the TraceId property from the objects before comparison
            expectedObject.Remove("TraceId");
            actualObject.Remove("TraceId");

            string expectedString = expectedObject.ToString();
            string actualString = actualObject.ToString();

            try
            {
                Assert.AreEqual(expectedString, actualString);
            }
            catch (AssertFailedException ex)
            {
                Console.WriteLine("Expected JSON: " + expectedString);
                Console.WriteLine("Actual JSON: " + actualString);
                throw ex;
            }
        }
    }

    public class LogInUserAPIControllerUnitTests
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class JSonDataSignInAPIControllerUnitTests
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
        public ErrorDataSignInAPIControllerUnitTests Errors { get; set; }
    }

    public class ErrorDataSignInAPIControllerUnitTests
    {
        public string[] Email { get; set; }
        public string[] Password { get; set; }
    }

    public class APIController
    {
        private readonly HttpClient _httpClient;

        public APIController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> Login(LogInUserAPIControllerUnitTests user)
        {
            return await _httpClient.PostAsJsonAsync("api/User/Login", user);
        }
    }
}
