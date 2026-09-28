using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;
using System;
using System.Threading;

namespace FikirHavuzu.UITests
{
    public class LoginTest : IDisposable
    {
        private readonly IWebDriver _driver;

        public LoginTest()
        {
            _driver = new ChromeDriver();
            _driver.Manage().Window.Maximize();
        }

        [Fact]
        public void User_Can_Fill_Login_Form()
        {
            _driver.Navigate().GoToUrl("http://localhost:3000/auth/login");
            Thread.Sleep(2000);

            IWebElement sicilInput = _driver.FindElement(By.Id("sicilno"));
            sicilInput.SendKeys("12345");
            Thread.Sleep(1000); 

            IWebElement passwordInput = _driver.FindElement(By.Id("password"));
            passwordInput.SendKeys("GizliSifre123!");
            Thread.Sleep(1000);

            IWebElement loginButton = _driver.FindElement(By.CssSelector("button.p-button"));
            loginButton.Click();
            
            Thread.Sleep(3000);
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
