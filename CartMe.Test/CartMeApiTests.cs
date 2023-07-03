using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;

namespace CartMe.Test
{
    [TestClass]
    public class CartMeApiTests
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
        [DataRow("abc@abc", "abc", HttpStatusCode.BadRequest)]
        [DataRow("shehroz.munir@powersoft19.com", "Welcome@123", HttpStatusCode.OK)]
        public async Task SignInTest(string email, string password, HttpStatusCode statusCode)
        {
            var user = new User()
            {
                Email = email,
                Password = password
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/User/Login", user);
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(statusCode, response.StatusCode);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up resources
            httpClient.Dispose();
        }
    }

    public class User
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
