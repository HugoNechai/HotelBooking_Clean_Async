
using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumTests.Pages;
using Xunit;

namespace SeleniumTests;

public class BookingE2ETests
{
    [Fact]
    public void CreateBooking_WithValidDates_ShouldSucceed()
    {
        using var driver = new ChromeDriver();

        var startDate = DateTime.Today.AddDays(90);
        var endDate = startDate.AddDays(2);

        // Arrange
        var bookingPage = new CreateBookingPage(driver);
        bookingPage.Open();

        // Act
        bookingPage.FillBooking(startDate, endDate);
        bookingPage.Submit();

        // Assert
        var wait = new WebDriverWait(
            driver,
            TimeSpan.FromSeconds(5)
        );

        wait.Until(d =>
            d.Url.EndsWith("/Bookings")
        );

        Assert.Contains(
            startDate.ToString("dd/MM/yyyy"),
            driver.PageSource
        );
    }
}
