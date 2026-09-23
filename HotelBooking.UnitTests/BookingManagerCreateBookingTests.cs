using System;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.UnitTests.TestDoubles;
using Moq;
using Xunit;
using static HotelBooking.UnitTests.TestDoubles.BookingManagerBuilder;

namespace HotelBooking.UnitTests
{
    // Tests for BookingManager.CreateBooking.
    //
    // Unless a test builds its own hotel, the scenario is two rooms (1 and 2), both booked
    // from day 10 to day 20 inclusive.
    public class BookingManagerCreateBookingTests
    {
        private const int OccupiedFrom = 10;
        private const int OccupiedTo = 20;

        private static BookingManagerBuilder FullyBookedHotel() =>
            new BookingManagerBuilder()
                .WithRooms(1, 2)
                .WithAllRoomsOccupied(OccupiedFrom, OccupiedTo);

        private static Booking BookingFor(int startOffset, int endOffset) => new Booking
        {
            CustomerId = 1,
            StartDate = Day(startOffset),
            EndDate = Day(endOffset)
        };

        // Data-driven with inline data: the same equivalence classes as FindAvailableRoom,
        // seen through the public CreateBooking API.
        [Theory]
        [InlineData(1, 5, true)]    // completely before the occupied period
        [InlineData(1, 9, true)]    // ends the day before it starts (boundary)
        [InlineData(1, 10, false)]  // ends on the first occupied day (boundary)
        [InlineData(5, 15, false)]  // overlaps the beginning
        [InlineData(12, 18, false)] // completely inside
        [InlineData(18, 25, false)] // overlaps the end
        [InlineData(20, 25, false)] // starts on the last occupied day (boundary)
        [InlineData(21, 21, true)]  // starts the day after it ends (boundary)
        [InlineData(25, 30, true)]  // completely after the occupied period
        public async Task CreateBooking_PeriodRelativeToOccupiedPeriod_ReturnsExpectedResult(
            int startOffset, int endOffset, bool expectedResult)
        {
            // Arrange
            var bookingManager = FullyBookedHotel().Build();
            var booking = BookingFor(startOffset, endOffset);

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        // Strong assertion: the booking handed to the repository must be activated and
        // assigned to the free room.
        [Fact]
        public async Task CreateBooking_RoomAvailable_SavesActivatedBookingWithRoomId()
        {
            // Arrange
            var builder = FullyBookedHotel();
            var bookingManager = builder.Build();
            var booking = BookingFor(1, 5);

            int savedRoomId = -1;
            bool savedIsActive = false;
            builder.BookingRepository
                .Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Callback<Booking>(b =>
                {
                    // Captured at the moment of the call, so the test also proves that the
                    // properties were set before the booking was persisted.
                    savedRoomId = b.RoomId;
                    savedIsActive = b.IsActive;
                })
                .Returns(Task.CompletedTask);

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result);
            Assert.True(savedIsActive);
            Assert.Contains(savedRoomId, new[] { 1, 2 });
            Assert.Equal(booking.RoomId, savedRoomId);
        }

        [Fact]
        public async Task CreateBooking_RoomAvailable_SavesTheSameBookingInstanceExactlyOnce()
        {
            // Arrange
            var builder = FullyBookedHotel();
            var bookingManager = builder.Build();
            var booking = BookingFor(1, 5);

            // Act
            await bookingManager.CreateBooking(booking);

            // Assert against the mock object
            builder.BookingRepository.Verify(r => r.AddAsync(booking), Times.Once);
        }

        [Fact]
        public async Task CreateBooking_OnlyOneRoomFree_BooksThatRoom()
        {
            // Arrange
            var bookingManager = new BookingManagerBuilder()
                .WithRooms(1, 2)
                .WithBooking(roomId: 1, startOffset: OccupiedFrom, endOffset: OccupiedTo)
                .Build();
            var booking = BookingFor(12, 18);

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result);
            Assert.Equal(2, booking.RoomId);
        }

        [Fact]
        public async Task CreateBooking_NoRoomAvailable_DoesNotSaveTheBooking()
        {
            // Arrange
            var builder = FullyBookedHotel();
            var bookingManager = builder.Build();
            var booking = BookingFor(12, 18);

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(result);
            builder.BookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        }

        [Fact]
        public async Task CreateBooking_NoRoomAvailable_LeavesTheBookingUntouched()
        {
            // Arrange
            var bookingManager = FullyBookedHotel().Build();
            var booking = BookingFor(12, 18);

            // Act
            await bookingManager.CreateBooking(booking);

            // Assert - a rejected booking must not be activated or assigned a room.
            Assert.False(booking.IsActive);
            Assert.Equal(0, booking.RoomId);
        }

        [Fact]
        public async Task CreateBooking_HotelHasNoRooms_ReturnsFalse()
        {
            // Arrange
            var builder = new BookingManagerBuilder();
            var bookingManager = builder.Build();

            // Act
            bool result = await bookingManager.CreateBooking(BookingFor(1, 5));

            // Assert
            Assert.False(result);
            builder.BookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        }

        // Data-driven with member data: CreateBooking must propagate the validation performed
        // by FindAvailableRoom instead of silently rejecting or saving the booking.
        public static TheoryData<int, int> InvalidPeriods => new TheoryData<int, int>
        {
            { 0, 0 },
            { 0, 5 },
            { -1, 5 },
            { -10, -5 },
            { 5, 3 }
        };

        [Theory]
        [MemberData(nameof(InvalidPeriods))]
        public async Task CreateBooking_InvalidPeriod_ThrowsArgumentExceptionAndSavesNothing(
            int startOffset, int endOffset)
        {
            // Arrange
            var builder = FullyBookedHotel();
            var bookingManager = builder.Build();
            var booking = BookingFor(startOffset, endOffset);

            // Act
            Task result() => bookingManager.CreateBooking(booking);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
            builder.BookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        }

        [Fact]
        public async Task CreateBooking_OverlappingBookingIsInactive_BookingSucceeds()
        {
            // Arrange - a cancelled booking must not prevent a new one.
            var bookingManager = new BookingManagerBuilder()
                .WithRooms(1)
                .WithBooking(roomId: 1, startOffset: OccupiedFrom, endOffset: OccupiedTo, isActive: false)
                .Build();
            var booking = BookingFor(12, 18);

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result);
            Assert.Equal(1, booking.RoomId);
            Assert.True(booking.IsActive);
        }
    }
}
