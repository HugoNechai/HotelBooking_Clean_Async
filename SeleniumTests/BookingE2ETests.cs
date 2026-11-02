
using System;
using OpenQA.Selenium.Chrome;
using SeleniumTests.Pages;
using Xunit;

namespace SeleniumTests;

public class BookingE2ETests
{
    [Fact]
    public void CreateBooking_WithValidDates_ShouldSucceed()
    {
        using var driver = new ChromeDriver();

        var startDate = DateTime.Today.AddDays(60);
        var endDate = startDate.AddDays(2);

        // Arrange: Open booking page
        var bookingPage = new CreateBookingPage(driver);
        bookingPage.Open();

        // Act: Create booking
        bookingPage.FillBooking(startDate, endDate);
        bookingPage.Submit();

        // Assert: Verify booking
        Assert.EndsWith("/Bookings", driver.Url);

        Assert.Contains(
            startDate.ToString("dd/MM/yyyy"),
            driver.PageSource
        );
    }
}
