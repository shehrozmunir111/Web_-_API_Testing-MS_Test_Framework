using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CartMe.Test
{
    [TestClass]
    public class WebApiSignUpTests
    {
        private HttpClient httpClient;
        private const string errorNameRequired = "The Email field is required.";
        private const string errorEmailRequired = "The Email field is required.";
        private const string errorEmailNotValid = "The Email field is not a valid e-mail address.";
        private const string errorEmailInValid = "Email InValid";
        private const string errorPasswordRequired = "The Password field is required.";

        private const string passwordErrorMessage = "Unable to create user: Passwords must be at least 6 characters.\r\nPasswords must have at least one non alphanumeric character.\r\nPasswords must have at least one digit ('0'-'9').\r\nPasswords must have at least one uppercase ('A'-'Z').\r\n";
        private const string duplicateUserMessage = "Unable to create user: Username 'abc@abc.com' is already taken.\r\n";

        [TestInitialize]
        public void Initialize()
        {
            // Initialize the HttpClient
            httpClient = new HttpClient();
            // Set up the base address of your web API
            httpClient.BaseAddress = new Uri("https://localhost:7180");
        }

        [TestMethod]
        [DataRow("", "", "", 0, HttpStatusCode.BadRequest, null, new string[] { errorEmailRequired }, new string[] { errorNameRequired }, new string[] { errorPasswordRequired })] // Single Message should print in Email field
        [DataRow("AAAAAAAAAAAAAAAA", "AAAAAAAAAAAAAAAA@AAAAAAAAAAAAAAAA", "", 0, HttpStatusCode.BadRequest, null, new string[] { errorEmailInValid }, null, new string[] { errorPasswordRequired })]
        [DataRow("AAAAAAAAAAAAAAAA", "AAAAAAAAAAAAAAAA@AAAAAAAAAAAAAAAA.com", "", 0, HttpStatusCode.BadRequest, null, null, null, new string[] { errorPasswordRequired })] // Repeated and long length word
        [DataRow("shehroz", "abc", "Lhr01@Lhr", 0, HttpStatusCode.BadRequest, null, new string[] { errorEmailNotValid }, null, null)] // Single Message should print
        [DataRow("shehroz", "abc@", "Lhr01@Lhr", 0, HttpStatusCode.BadRequest, null, new string[] { errorEmailNotValid }, null, null)] // Single Message should print
        [DataRow("shehroz", "abc@abc", "Lhr01@Lhr", 0, HttpStatusCode.BadRequest, null, new string[] { errorEmailInValid }, null, null)]
        [DataRow("shehroz", "abc@abc.com", "abc", 0, HttpStatusCode.InternalServerError, passwordErrorMessage, null, null, null)]
        [DataRow("shehroz", "abc@abc.com", "Lhr01@Lhr", 0, HttpStatusCode.InternalServerError, duplicateUserMessage, null, null, null)]
        [DataRow("shehroz", "shehroz12@shehroz.com", "Lhr01@Lhr", 0, HttpStatusCode.Created, null, null, null, null)]
        public async Task SignUpTest(string name, string email, string password, int userRole, HttpStatusCode statusCode, string expectedErrorMessage ,string[] expectedEmailErrorMessages, string[] expectedNameErrorMessage, string[] expectedPasswordErrorMessages)
        {
            try
            {
                var user = new SignUpUser()
                {
                    Name = name,
                    Email = email,
                    Password = password,
                    UserRole = userRole
                };

                HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/User/CreateUser", user);
                var contents = await response.Content.ReadAsStringAsync();

                Assert.AreEqual(statusCode, response.StatusCode);

                // Check if contents is a valid JSON string
                if (IsJson(contents))
                {
                    try
                    {
                        // De-serialize the JSON response
                        JSonDataSignUp jsonData = JsonConvert.DeserializeObject<JSonDataSignUp>(contents);

                        // Access the properties of jsonData and perform assertions if necessary
                        if (jsonData.Errors != null)
                        {
                            if (jsonData.Errors.Email != null)
                            {
                                CollectionAssert.AreEqual(expectedEmailErrorMessages, jsonData.Errors.Email);
                            }
                            if (jsonData.Errors.Name != null)
                            {
                                CollectionAssert.AreEqual(expectedNameErrorMessage, jsonData.Errors.Name);
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
                else if (expectedErrorMessage == passwordErrorMessage || expectedErrorMessage == duplicateUserMessage)
                {
                    Assert.AreEqual(expectedErrorMessage, contents);
                }
                //else if (expectedErrorMessage == passwordSixCharacters || expectedErrorMessage == passwordOneNonAlpha || expectedErrorMessage == passwordOneDigit || expectedErrorMessage == passwordOneUpperCase || expectedErrorMessage == duplicateUserMessage)
                //{
                //    Assert.AreEqual(expectedErrorMessage, contents);
                //}
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
    public class SignUpUser
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int UserRole { get; set; }
    }
    public class JSonDataSignUp
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
        public ErrorDataSignUp Errors { get; set; }
    }
    public class ErrorDataSignUp
    {
        public string[] Name { get; set; }
        public string[] Email { get; set; }
        public string[] Password { get; set; }
    }
}
