
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;
using Moq;
using Reqnroll;
using Xunit;

namespace CucumberTests.StepDefinitions;

[Binding]
public class SuccessfulBookingSteps
{
    private readonly Mock<IRepository<Booking>> bookingRepository = new();
    private readonly Mock<IRepository<Room>> roomRepository = new();

    private DateTime startDate = DateTime.Today.AddDays(1);
    private DateTime endDate = DateTime.Today.AddDays(2);
    private bool bookingResult;

    [Given("a hotel has an available room")]
    [Given("a hotel has one room")]
    public void GivenHotelHasRoom()
    {
        roomRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Room>
            {
                new Room { Id = 1 }
            });

        bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Booking>());
    }

    [Given("the booking dates are valid")]
    public void GivenValidDates()
    {
        startDate = DateTime.Today.AddDays(1);
        endDate = DateTime.Today.AddDays(2);
    }

    [Given("the room is booked from tomorrow to 3 days from now")]
    public void GivenExistingBooking()
    {
        bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Booking>
            {
                new Booking
                {
                    RoomId = 1,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(3),
                    IsActive = true
                }
            });
    }

    [When("the customer creates a booking")]
    public async Task WhenCustomerCreatesBooking()
    {
        await CreateBooking();
    }

    [When("the customer books from 4 days to 6 days from now")]
    public async Task WhenCustomerBooksAfterExistingBooking()
    {
        startDate = DateTime.Today.AddDays(4);
        endDate = DateTime.Today.AddDays(6);

        await CreateBooking();
    }

    private async Task CreateBooking()
    {
        var manager = new BookingManager(
            bookingRepository.Object,
            roomRepository.Object
        );

        var booking = new Booking
        {
            StartDate = startDate,
            EndDate = endDate
        };

        bookingResult = await manager.CreateBooking(booking);
    }

    [Then("the booking should succeed")]
    public void ThenBookingSucceeds()
    {
        Assert.True(bookingResult);
    }
}
