
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;
using Moq;
using Reqnroll;
using Xunit;

namespace CucumberTests.StepDefinitions;

[Binding]
public class FailedBookingSteps
{
    private readonly Mock<IRepository<Booking>> bookingRepository = new();
    private readonly Mock<IRepository<Room>> roomRepository = new();

    private DateTime startDate = DateTime.Today.AddDays(1);
    private DateTime endDate = DateTime.Today.AddDays(2);

    private bool bookingResult;
    private bool bookingRejected;

    private void SetRooms(int count)
    {
        var rooms = new List<Room>();

        for (int i = 1; i <= count; i++)
        {
            rooms.Add(new Room { Id = i });
        }

        roomRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(rooms);

        bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Booking>());
    }

    private void SetExistingBooking(int startDay, int endDay)
    {
        bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Booking>
            {
                new Booking
                {
                    RoomId = 1,
                    StartDate = DateTime.Today.AddDays(startDay),
                    EndDate = DateTime.Today.AddDays(endDay),
                    IsActive = true
                }
            });
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

        try
        {
            bookingResult = await manager.CreateBooking(booking);
        }
        catch (ArgumentException)
        {
            bookingRejected = true;
        }
    }

    // TC02 - All rooms are booked

    [Given("all hotel rooms are booked for the requested dates")]
    public void GivenAllRoomsBooked()
    {
        SetRooms(1);
        SetExistingBooking(1, 5);
    }

    [When("the customer creates a booking for those dates")]
    public async Task WhenBookingUnavailableDates()
    {
        startDate = DateTime.Today.AddDays(2);
        endDate = DateTime.Today.AddDays(3);

        await CreateBooking();
    }

    // TC03, TC04, TC05 - Invalid dates

    [When("the customer books from yesterday to tomorrow")]
    public async Task WhenStartDateIsYesterday()
    {
        startDate = DateTime.Today.AddDays(-1);
        endDate = DateTime.Today.AddDays(1);

        await CreateBooking();
    }

    [When("the customer books from today to tomorrow")]
    public async Task WhenStartDateIsToday()
    {
        startDate = DateTime.Today;
        endDate = DateTime.Today.AddDays(1);

        await CreateBooking();
    }

    [When("the customer books with an end date before the start date")]
    public async Task WhenEndDateBeforeStartDate()
    {
        startDate = DateTime.Today.AddDays(5);
        endDate = DateTime.Today.AddDays(2);

        await CreateBooking();
    }

    // TC06 - Overlapping booking

    [Given("the room is booked from tomorrow to 5 days from now")]
    public void GivenOverlappingBooking()
    {
        SetExistingBooking(1, 5);
    }

    [When("the customer books from 2 days to 7 days from now")]
    public async Task WhenBookingOverlappingDates()
    {
        startDate = DateTime.Today.AddDays(2);
        endDate = DateTime.Today.AddDays(7);

        await CreateBooking();
    }

    // TC08 - No rooms

    [Given("a hotel has no rooms")]
    public void GivenNoRooms()
    {
        SetRooms(0);
    }

    [When("the customer creates a booking with valid dates")]
    public async Task WhenBookingWithValidDates()
    {
        await CreateBooking();
    }

    // Results

    [Then("the booking should fail")]
    public void ThenBookingFails()
    {
        Assert.False(bookingResult);
    }

    [Then("the booking should be rejected")]
    public void ThenBookingRejected()
    {
        Assert.True(bookingRejected);
    }
}
