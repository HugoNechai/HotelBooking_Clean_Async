using HotelBooking.Core;
using HotelBooking.UnitTests.TestDoubles;
using Moq;
using static HotelBooking.UnitTests.TestDoubles.BookingManagerBuilder;

namespace MyReqnrollProject.StepDefinitions;

[Binding]
public sealed class CreateBookingStepsDefinition
{
    // Reqnroll creates a new binding instance for each scenario.
    private readonly BookingManagerBuilder _builder = new();
    private Booking _booking = null!;
    private bool _result;
    private Exception? _exception;

    [Given("the hotel has no rooms")]
    public void GivenTheHotelHasNoRooms()
    {
        // The builder starts with empty rtheepositories.
    }

    [Given("the hotel has room {int}")]
    public void GivenTheHotelHasRoom(int roomId)
    {
        _builder.WithRooms(roomId);
    }

    [Given("the hotel has rooms {int} and {int}")]
    public void GivenTheHotelHasRooms(int firstRoomId, int secondRoomId)
    {
        _builder.WithRooms(firstRoomId, secondRoomId);
    }

    [Given("room {int} is booked from day {int} to {int}")]
    public void GivenRoomIsBooked(int roomId, int start, int end)
    {
        _builder.WithBooking(roomId, start, end);
    }

    [Given("room {int} has a cancelled booking from day {int} to {int}")]
    public void GivenRoomHasACancelledBooking(int roomId, int start, int end)
    {
        _builder.WithBooking(roomId, start, end, isActive: false);
    }

    [When("the customer tries to make a booking from day {int} to {int}")]
    public async Task WhenTheCustomerTriesToMakeABooking(int start, int end)
    {
        _booking = new Booking
        {
            CustomerId = 1,
            StartDate = Day(start),
            EndDate = Day(end)
        };

        try
        {
            _result = await _builder.Build().CreateBooking(_booking);
        }
        catch (ArgumentException exception)
        {
            // Invalid dates are rejected by throwing, rather than returning false.
            _exception = exception;
        }
    }

    [Then("the booking should be accepted")]
    public void ThenTheBookingShouldBeAccepted()
    {
        Assert.That(_exception, Is.Null);
        Assert.That(_result, Is.True);
        Assert.That(_booking.IsActive, Is.True);
        _builder.BookingRepository.Verify(r => r.AddAsync(_booking), Times.Once);
    }

    [Then("the booking should be assigned to room {int}")]
    public void ThenTheBookingShouldBeAssignedToRoom(int roomId)
    {
        Assert.That(_booking.RoomId, Is.EqualTo(roomId));
    }

    [Then("the booking should be rejected")]
    public void ThenTheBookingShouldBeRejected()
    {
        Assert.That(_exception, Is.Null);
        Assert.That(_result, Is.False);
        VerifyBookingWasNotSaved();
    }

    [Then("the booking should be rejected as invalid")]
    public void ThenTheBookingShouldBeRejectedAsInvalid()
    {
        Assert.That(_exception, Is.TypeOf<ArgumentException>());
        VerifyBookingWasNotSaved();
    }

    private void VerifyBookingWasNotSaved()
    {
        _builder.BookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
    }
}
