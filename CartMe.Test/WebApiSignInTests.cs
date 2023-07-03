using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;

namespace CartMe.Test
{
    [TestClass]
    public class WebApiSignInTests
    {
        private HttpClient httpClient;
        private const string emailErrorMessage = "The Email field is required.";
        private const string emailErrorMessageSecond = "The Email field is not a valid e-mail address.";
        private const string passwordErrorMessage = "The Password field is required.";
        private const string loginErrorUserNotFound = "Unable to login as user 'abc@abc' not found.";
        private const string loginErrorPasswordIncorrect = "Unable to login as user name or password is incorrect.";
        private const string loginErrorEmailIncorrect = "Unable to login as email 'shehroz@powersoft19.com' not confirmed.";


        [TestInitialize]
        public void Initialize()
        {
            // Initialize the HttpClient
            httpClient = new HttpClient();
            // Set up the base address of your web API
            httpClient.BaseAddress = new Uri("https://localhost:7180");
        }

        [TestMethod]
        [DataRow("", "", HttpStatusCode.BadRequest, null, new string[] { emailErrorMessage }, new string[] { passwordErrorMessage })] // Actual: "emailErrorMessageSecond" show, its a BUG
        [DataRow("abc", "abc", HttpStatusCode.BadRequest, null, new string[] { emailErrorMessageSecond }, null)]
        [DataRow("abc@", "abc", HttpStatusCode.BadRequest, null, new string[] { emailErrorMessageSecond }, null)]
        [DataRow("abc@abc", "abc", HttpStatusCode.NotFound, loginErrorUserNotFound, null, null)]
        [DataRow("shehroz.munir@powersoft19.com", "abc", HttpStatusCode.InternalServerError, loginErrorPasswordIncorrect, null, null)]
        [DataRow("shehroz@powersoft19.com", "abc", HttpStatusCode.Forbidden, loginErrorEmailIncorrect, null, null)]
        [DataRow("shehroz.munir@powersoft19.com", "Welcome@123", HttpStatusCode.OK, null, null, null)]
        public async Task SignInTest(string email, string password, HttpStatusCode statusCode, string expectedErrorMessage, string[] expectedEmailErrorMessages, string[] expectedPasswordErrorMessages)
        {
            try
            {
                var user = new LogInUser()
                {
                    Email = email,
                    Password = password
                };

                HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/User/Login", user);
                var contents = await response.Content.ReadAsStringAsync();

                Assert.AreEqual(statusCode, response.StatusCode);

                // Check if contents is a valid JSON string
                if (IsJson(contents))
                {
                    try
                    {
                        // De-serialize the JSON response
                        JSonDataSignIn jsonData = JsonConvert.DeserializeObject<JSonDataSignIn>(contents);

                        // Access the properties of jsonData and perform assertions if necessary
                        if (jsonData.Errors != null)
                        {
                            if (jsonData.Errors.Email != null)
                            {
                                CollectionAssert.AreEqual(expectedEmailErrorMessages, jsonData.Errors.Email);
                            }

                            if (jsonData.Errors.Password != null)
                            {
                                CollectionAssert.AreEqual(expectedPasswordErrorMessages, jsonData.Errors.Password);
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Handle the exception if the JSON De-serialization fails
                        Assert.Fail($"Failed to De-serialize JSON: {ex.Message}");
                    }
                }
                else if (expectedErrorMessage == loginErrorUserNotFound || expectedErrorMessage == loginErrorPasswordIncorrect || expectedErrorMessage == loginErrorEmailIncorrect)
                {
                    Assert.AreEqual(expectedErrorMessage, contents);
                }
                else
                {
                    // Handle the case when contents is not a valid JSON string
                    Assert.Fail("Response does not contain valid JSON.");
                }
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
        private bool IsJson(string input)
        {
            try
            {
                JToken.Parse(input);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

    }
    public class LogInUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class JSonDataSignIn
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
        public ErrorDataSignIn Errors { get; set; }
    }

    public class ErrorDataSignIn
    {
        public string[] Email { get; set; }
        public string[] Password { get; set; }
    }
}
