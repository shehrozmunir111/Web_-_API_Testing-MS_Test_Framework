using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Xml.Linq;
using SeleniumExtras.WaitHelpers;

namespace CartMe.Test
{
    [TestClass]
    public class SignInTests
    {
        public static IWebDriver driver;
        // Login
        private const string userNameTextBoxXPath = "//*[@id=\"root\"]/div/form/div/input[1]";
        private const string passwordTextBoxXPath = "//*[@id=\"root\"]/div/form/div/input[2]";
        private const string loginErrorBoxXPath = "/html/body/div/div/form/div/div[1]";
        private const string loginButtonXPath = "//*[@id=\"root\"]/div/form/div/button";

        private const string tpErrEmpty = "Please fill out this field.";
        private const string tpErrUserName = "Please include an '@' in the email address. 'abc' is missing an '@'.";
        private const string tpScndErrUserName = "Please enter a part following '@'. 'abc@' is incomplete.";
        private const string loginError = "Unable to login as user 'abc@abc' not found.";

        //[AssemblyInitialize()]
        [ClassInitialize()]
        //[TestInitialize()]
        [Obsolete]
        public static void Setup(TestContext context)
        {
            // Create Reference for the Chrome Browser
            driver = new ChromeDriver();
            // Navigate to URl
            driver.Navigate().GoToUrl("http://localhost:3000");
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
            driver.FindElement(By.LinkText("Log In")).Click();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
        }

        [TestMethod]
        [DataRow("", "", tpErrEmpty)]
        [DataRow("abc", "abc", tpErrUserName)]
        [DataRow("abc@", "abc", tpScndErrUserName)]
        [DataRow("abc@abc", "abc", loginError)] 
        [DataRow("shehroz.munir@powersoft19.com", "Welcome@123", "")]
        public void SignIn(string userName, string password, string expectedErrorMessage)
        {
            try
            {
                // Name
                driver.FindElement(By.XPath(userNameTextBoxXPath)).Clear();
                driver.FindElement(By.XPath(userNameTextBoxXPath)).SendKeys(userName);

                // Password
                driver.FindElement(By.XPath(passwordTextBoxXPath)).Clear();
                driver.FindElement(By.XPath(passwordTextBoxXPath)).SendKeys(password);

                // Click on Login Button
                driver.FindElement(By.XPath(loginButtonXPath)).Click();
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));

                IWebElement userNameTextBox = driver.FindElement(By.XPath(userNameTextBoxXPath));
                string getUserNameText = userNameTextBox.GetAttribute("value");

                if (string.IsNullOrEmpty(getUserNameText))
                {
                    IWebElement userNameErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(userNameTextBoxXPath)));
                    string actualUserNameError = userNameErrorBox.GetAttribute("validationMessage");
                    Assert.AreEqual(expectedErrorMessage, actualUserNameError);
                }

                IWebElement passwordTextBox = driver.FindElement(By.XPath(passwordTextBoxXPath));
                string getPasswordText = passwordTextBox.GetAttribute("value");

                if (string.IsNullOrEmpty(getPasswordText))
                {
                    IWebElement passwordTpErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(passwordTextBoxXPath)));
                    string actualTpPasswordError = passwordTpErrorBox.GetAttribute("validationMessage");
                    Assert.AreEqual(expectedErrorMessage, actualTpPasswordError);
                }
                else if (expectedErrorMessage == tpErrUserName)
                {
                    IWebElement userNameErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(userNameTextBoxXPath)));
                    string actualUserNameError = userNameErrorBox.GetAttribute("validationMessage");
                    Assert.AreEqual(expectedErrorMessage, actualUserNameError);
                }
                else if (expectedErrorMessage == tpScndErrUserName)
                {
                    IWebElement userNameErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(userNameTextBoxXPath)));
                    string actualUserNameError = userNameErrorBox.GetAttribute("validationMessage");
                    Assert.AreEqual(expectedErrorMessage, actualUserNameError);
                }
                else if (expectedErrorMessage == loginError)
                {
                    Thread.Sleep(5000);

                    IWebElement loginErrorBox = driver.FindElement(By.XPath(loginErrorBoxXPath));
                    string actualLoginError = loginErrorBox.Text;
                    Assert.AreEqual(expectedErrorMessage, actualLoginError);
                }
                else if (expectedErrorMessage == "")
                {
                    // Assert that there are no error messages
                    bool isUserNameErrorDisplayed = IsElementDisplayed(By.Id(userNameTextBoxXPath));
                    bool isPasswordErrorDisplayed = IsElementDisplayed(By.Id(passwordTextBoxXPath));

                    Assert.IsFalse(isUserNameErrorDisplayed, "Name error message is displayed");
                    Assert.IsFalse(isPasswordErrorDisplayed, "Password error message is displayed");
                }
                else
                {
                    Assert.Fail("Invalid expected error message provided.");
                }
            }
            catch (NoSuchElementException)
            {

                Assert.Fail("Field not find.");
            }

        }

        private bool IsElementDisplayed(By locator)
        {
            try
            {
                return driver.FindElement(locator).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        //[AssemblyCleanup()]
        [ClassCleanup()]
        //[TestCleanup()]
        public static void CleanUp()
        {
            // shutdown
            driver!.Quit();
            driver!.Dispose();
        }
    }
}
