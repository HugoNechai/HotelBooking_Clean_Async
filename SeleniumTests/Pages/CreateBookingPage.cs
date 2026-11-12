
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests.Pages;

public class CreateBookingPage
{
    private readonly IWebDriver driver;

    public CreateBookingPage(IWebDriver driver)
    {
        this.driver = driver;
    }

    public void Open()
    {
        driver.Navigate().GoToUrl(
            "http://localhost:5000/Bookings/Create"
        );
    }

    public void FillBooking(DateTime startDate, DateTime endDate)
    {
        var js = (IJavaScriptExecutor)driver;

        js.ExecuteScript(
            "arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('input', { bubbles: true })); arguments[0].dispatchEvent(new Event('change', { bubbles: true }));",
            driver.FindElement(By.Id("StartDate")),
            startDate.ToString("yyyy-MM-dd")
        );

        js.ExecuteScript(
            "arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('input', { bubbles: true })); arguments[0].dispatchEvent(new Event('change', { bubbles: true }));",
            driver.FindElement(By.Id("EndDate")),
            endDate.ToString("yyyy-MM-dd")
        );

        var customer = new SelectElement(
            driver.FindElement(By.Id("CustomerId"))
        );

        customer.SelectByText("John Smith");
    }

    public void Submit()
    {
        driver.FindElement(
            By.CssSelector("input[type='submit']")
        ).Click();
    }
}
