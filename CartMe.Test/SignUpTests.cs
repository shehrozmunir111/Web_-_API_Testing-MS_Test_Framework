using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace CartMe.Test
{
    [TestClass]
    public class SignUpTests
    {
        public static IWebDriver driver;
        //public static IWebElement alertElement;

        // SignUp 
        private const string nameTextBoxXPath = "//*[@id=\"root\"]/div/div/form/input[1]";
        private const string emailAddressTextBoxXPath = "//*[@id=\"root\"]/div/div/form/input[2]";
        private const string passwordTextBoxXPath = "//*[@id=\"root\"]/div/div/form/input[3]";
        private const string confirmPasswordTextBoxXPath = "//*[@id=\"root\"]/div/div/form/input[4]";
        private const string signUpButtonXPath = "//*[@id=\"root\"]/div/div/form/button";
        private const string emailErrorBoxXPath = "//*[@id=\"root\"]/div/div/form/div[1]/div";
        private const string passwordFieldErrorBoxXPath = "//*[@id=\"root\"]/div/div/form/div[1]/div";
        private const string confirmPasswordErrorBoxXPath = "//*[@id=\"root\"]/div/div/form/div[2]/div";
        private const string duplicateUserErrorBoxXPath = "/html/body/div/div/div/form/div[2]/div";
        private const string userCreatedXPath = "//*[@id=\"root\"]/div/div/form/div[4]/div";

        private const string tpNameErrEmpty = "Please fill out this field.";
        private const string errEmail = "Invalid Email";
        private const string repeatedEmailErr = "No more then 5 characters are allow after and before '@'.";
        private const string tpErrEmail = "Please include an '@' in the email address. 'abc' is missing an '@'.";
        private const string tpScndErrEmail = "Please enter a part following '@'. 'abc@' is incomplete.";
        private const string errPassword = "Password must contain at least 1 uppercase , 1 numeric and one special character(!@#$%^&). The password length must be greater than 8.";
        private const string errCnfrmPswrd = "Password Mismatch";
        private const string errDuplicateUser = "Unable to create user: Username 'abc@abc.com' is already taken.";
        private const string userCreatedMsg = "User Created Successfully";


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
            driver.FindElement(By.LinkText("Sign Up")).Click();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
        }
        
        // commented code for next build
        [TestMethod]
        [DataRow("", "", "", "", tpNameErrEmpty)]
        [DataRow("AAAAAAAAAAAAAAAA", "AAAAAAAAAAAAAAAA@AAAAAAAAAAAAAAA.com", "Lhr01@Lhr", "Lhr01@Lhr", repeatedEmailErr)] // Repeated and long length word
       //[DataRow("shehroz", "", "", "", tpEmailErrEmpty)]
        [DataRow("shehroz", "abc", "Lhr01@Lhr", "Lhr01@Lhr", tpErrEmail)]
        [DataRow("shehroz", "abc@", "Lhr01@Lhr", "Lhr01@Lhr", tpScndErrEmail)]
        //[DataRow("shehroz", "abc@abc", "", "", tpPasswordErrEmpty)]
        //[DataRow("shehroz", "abc@abc", "abc", "", tpCnfrmPswrdErrEmpty)]
        [DataRow("shehroz", "abc@abc", "Lhr01@Lhr", "Lhr01@Lhr", errEmail)]
        [DataRow("shehroz", "abc@abc.com", "abc", "Lhr01@Lhr", errPassword)]
        //[DataRow("shehroz", "abc@abc", "A1@", "", tpCnfrmPswrdErrEmpty)] 
        [DataRow("shehroz", "abc@abc.com", "Lhr01@Lhr", "abc", errCnfrmPswrd)]
        [DataRow("shehroz", "abc4@abc.com", "Lhr01@Lhr", "Lhr01@Lhr", userCreatedMsg)]
        [DataRow("shehroz", "abc@abc.com", "Lhr01@Lhr", "Lhr01@Lhr", errDuplicateUser)]
        public void SignUp(string name, string email, string password, string confirmPassword, string expectedErrorMessage)
        {

            try
            {
                // Name
                driver.FindElement(By.XPath(nameTextBoxXPath)).Clear();
                driver.FindElement(By.XPath(nameTextBoxXPath)).SendKeys(name);

                // Email
                driver.FindElement(By.XPath(emailAddressTextBoxXPath)).Clear();
                driver.FindElement(By.XPath(emailAddressTextBoxXPath)).SendKeys(email);

                // Password
                driver.FindElement(By.XPath(passwordTextBoxXPath)).Clear();
                driver.FindElement(By.XPath(passwordTextBoxXPath)).SendKeys(password);

                // Confirm Password
                driver.FindElement(By.XPath(confirmPasswordTextBoxXPath)).Clear();
                driver.FindElement(By.XPath(confirmPasswordTextBoxXPath)).SendKeys(confirmPassword);

                // Click on Sign Up Button
                driver.FindElement(By.XPath(signUpButtonXPath)).Click();
                //Thread.Sleep(5000);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));

                IWebElement nameTextBox = driver.FindElement(By.XPath(nameTextBoxXPath));
                string getNameText = nameTextBox.GetAttribute("value");

                if (string.IsNullOrEmpty(getNameText))
                {
                    IWebElement nameErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(nameTextBoxXPath)));
                    string actualNameError = nameErrorBox.GetAttribute("validationMessage");
                    Assert.AreEqual(expectedErrorMessage, actualNameError);
                }

                IWebElement emailTextBox = driver.FindElement(By.XPath(emailAddressTextBoxXPath));
                string getEmailText = emailTextBox.GetAttribute("value");

                if (string.IsNullOrEmpty(getEmailText))
                {
                    IWebElement emailTpErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(emailAddressTextBoxXPath)));
                    string actualTpEmailError = emailTpErrorBox.GetAttribute("validationMessage");
                    Assert.AreEqual(expectedErrorMessage, actualTpEmailError);
                }

                IWebElement passwordTextBox = driver.FindElement(By.XPath(passwordTextBoxXPath));
                string getPasswordText = passwordTextBox.GetAttribute("value");

                if (string.IsNullOrEmpty(getPasswordText))
                {
                    IWebElement passwordTpErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(passwordTextBoxXPath)));
                    string actualTpPasswordError = passwordTpErrorBox.GetAttribute("validationMessage");
                    Assert.AreEqual(expectedErrorMessage, actualTpPasswordError);
                }

                IWebElement confirmPasswordTextBox = driver.FindElement(By.XPath(confirmPasswordTextBoxXPath));
                string getConfirmPasswordText = confirmPasswordTextBox.GetAttribute("value");

                if (string.IsNullOrEmpty(getPasswordText))
                {
                    IWebElement cnfrmPswrdTpErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(confirmPasswordTextBoxXPath)));
                    string actualTpCnfrmPswrdError = cnfrmPswrdTpErrorBox.GetAttribute("validationMessage");
                    Assert.AreEqual(expectedErrorMessage, actualTpCnfrmPswrdError);
                }
                else if (expectedErrorMessage == errEmail)
                {
                    IWebElement confirmPasswordErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(emailErrorBoxXPath)));
                    string actualConfirmPasswordError = confirmPasswordErrorBox.Text;
                    Assert.AreEqual(expectedErrorMessage, actualConfirmPasswordError);
                }
                else if (expectedErrorMessage == repeatedEmailErr)
                {
                    IWebElement confirmPasswordErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(emailErrorBoxXPath)));
                    string actualConfirmPasswordError = confirmPasswordErrorBox.Text;
                    Assert.AreEqual(expectedErrorMessage, actualConfirmPasswordError);
                }
                else if (expectedErrorMessage == tpErrEmail)
                {
                    IWebElement emailErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(emailAddressTextBoxXPath)));
                    string actualEmailError = emailErrorBox.GetAttribute("validationMessage");
                    Assert.AreEqual(expectedErrorMessage, actualEmailError);
                }
                else if (expectedErrorMessage == tpScndErrEmail)
                {
                    IWebElement emailErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(emailAddressTextBoxXPath)));
                    string actualEmailError = emailErrorBox.GetAttribute("validationMessage");
                    Assert.AreEqual(expectedErrorMessage, actualEmailError);
                }
                else if (expectedErrorMessage == errPassword)
                {
                    IWebElement passwordErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(passwordFieldErrorBoxXPath)));
                    string actualPasswordError = passwordErrorBox.Text;
                    Assert.AreEqual(expectedErrorMessage, actualPasswordError);
                }
                else if (expectedErrorMessage == errCnfrmPswrd)
                {
                    IWebElement confirmPasswordErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(confirmPasswordErrorBoxXPath)));
                    string actualConfirmPasswordError = confirmPasswordErrorBox.Text;
                    Assert.AreEqual(expectedErrorMessage, actualConfirmPasswordError);
                }
                else if (expectedErrorMessage == errDuplicateUser)
                {
                    Thread.Sleep(5000);

                    IWebElement confirmPasswordErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(duplicateUserErrorBoxXPath)));
                    string actualConfirmPasswordError = confirmPasswordErrorBox.Text;
                    Assert.AreEqual(expectedErrorMessage, actualConfirmPasswordError);
                }
                else if (expectedErrorMessage == userCreatedMsg)
                {
                    Thread.Sleep(5000);

                    IWebElement confirmPasswordErrorBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(userCreatedXPath)));
                    string actualConfirmPasswordError = confirmPasswordErrorBox.Text;
                    Assert.AreEqual(expectedErrorMessage, actualConfirmPasswordError);
                }
                else if (expectedErrorMessage == "")
                {
                    // Assert that there are no error messages
                    bool isNameErrorDisplayed = IsElementDisplayed(By.Id(nameTextBoxXPath));
                    bool isEmailErrorDisplayed = IsElementDisplayed(By.Id(emailAddressTextBoxXPath));
                    bool isPasswordErrorDisplayed = IsElementDisplayed(By.Id(passwordFieldErrorBoxXPath));
                    bool isConfirmPasswordErrorDisplayed = IsElementDisplayed(By.Id(confirmPasswordErrorBoxXPath));

                    Assert.IsFalse(isNameErrorDisplayed, "Name error message is displayed");
                    Assert.IsFalse(isEmailErrorDisplayed, "Email error message is displayed");
                    Assert.IsFalse(isPasswordErrorDisplayed, "Password error message is displayed");
                    Assert.IsFalse(isConfirmPasswordErrorDisplayed, "Confirm Password error message is displayed");
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