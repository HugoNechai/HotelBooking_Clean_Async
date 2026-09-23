using System;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.UnitTests.TestData;
using HotelBooking.UnitTests.TestDoubles;
using Moq;
using Xunit;
using static HotelBooking.UnitTests.TestDoubles.BookingManagerBuilder;

namespace HotelBooking.UnitTests
{
    // Tests for BookingManager.GetFullyOccupiedDates.
    //
    // A date is fully occupied when the number of active bookings covering it is at least
    // the number of rooms in the hotel. Unless a test builds its own hotel, the scenario is
    // two rooms, both booked from day 10 to day 20 inclusive.
    public class BookingManagerGetFullyOccupiedDatesTests
    {
        private const int OccupiedFrom = 10;
        private const int OccupiedTo = 20;

        private static BookingManager FullyBookedHotel() =>
            new BookingManagerBuilder()
                .WithRooms(1, 2)
                .WithAllRoomsOccupied(OccupiedFrom, OccupiedTo)
                .Build();

        // Data-driven with class data: the rows live in OccupiedDateRanges.
        [Theory]
        [ClassData(typeof(OccupiedDateRanges))]
        public async Task GetFullyOccupiedDates_VariousRanges_ReturnsExpectedNumberOfDates(
            int startOffset, int endOffset, int expectedNumberOfDates)
        {
            // Arrange
            var bookingManager = FullyBookedHotel();

            // Act
            var occupiedDates = await bookingManager.GetFullyOccupiedDates(Day(startOffset), Day(endOffset));

            // Assert
            Assert.Equal(expectedNumberOfDates, occupiedDates.Count);
        }

        // Strong assertion: check which dates are returned, not just how many.
        [Fact]
        public async Task GetFullyOccupiedDates_RangeOverlapsOccupiedPeriod_ReturnsExactlyTheOccupiedDates()
        {
            // Arrange
            var bookingManager = FullyBookedHotel();

            // Act
            var occupiedDates = await bookingManager.GetFullyOccupiedDates(Day(8), Day(12));

            // Assert
            Assert.Equal(new[] { Day(10), Day(11), Day(12) }, occupiedDates);
        }

        // Data-driven with member data: the start date is later than the end date.
        public static TheoryData<int, int> RangesWithStartAfterEnd => new TheoryData<int, int>
        {
            { 5, 3 },
            { 1, 0 },
            { 30, 1 },
            { -1, -2 }
        };

        [Theory]
        [MemberData(nameof(RangesWithStartAfterEnd))]
        public async Task GetFullyOccupiedDates_StartDateAfterEndDate_ThrowsArgumentException(
            int startOffset, int endOffset)
        {
            // Arrange
            var bookingManager = FullyBookedHotel();

            // Act
            Task result() => bookingManager.GetFullyOccupiedDates(Day(startOffset), Day(endOffset));

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        // Unlike FindAvailableRoom, this method accepts dates in the past.
        [Theory]
        [InlineData(-10, -5)]
        [InlineData(0, 0)]
        [InlineData(-1, 1)]
        public async Task GetFullyOccupiedDates_RangeInThePast_DoesNotThrow(int startOffset, int endOffset)
        {
            // Arrange
            var bookingManager = FullyBookedHotel();

            // Act
            var occupiedDates = await bookingManager.GetFullyOccupiedDates(Day(startOffset), Day(endOffset));

            // Assert
            Assert.Empty(occupiedDates);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_OnlyOneOfTwoRoomsBooked_ReturnsEmptyList()
        {
            // Arrange - one free room means no date is fully occupied.
            var bookingManager = new BookingManagerBuilder()
                .WithRooms(1, 2)
                .WithBooking(roomId: 1, startOffset: OccupiedFrom, endOffset: OccupiedTo)
                .Build();

            // Act
            var occupiedDates = await bookingManager.GetFullyOccupiedDates(Day(OccupiedFrom), Day(OccupiedTo));

            // Assert
            Assert.Empty(occupiedDates);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_NoBookingsExist_ReturnsEmptyList()
        {
            // Arrange
            var bookingManager = new BookingManagerBuilder().WithRooms(1, 2).Build();

            // Act
            var occupiedDates = await bookingManager.GetFullyOccupiedDates(Day(1), Day(30));

            // Assert
            Assert.Empty(occupiedDates);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_AllBookingsAreInactive_ReturnsEmptyList()
        {
            // Arrange - cancelled bookings must not make a date fully occupied.
            var bookingManager = new BookingManagerBuilder()
                .WithRooms(1, 2)
                .WithBooking(roomId: 1, startOffset: OccupiedFrom, endOffset: OccupiedTo, isActive: false)
                .WithBooking(roomId: 2, startOffset: OccupiedFrom, endOffset: OccupiedTo, isActive: false)
                .Build();

            // Act
            var occupiedDates = await bookingManager.GetFullyOccupiedDates(Day(OccupiedFrom), Day(OccupiedTo));

            // Assert
            Assert.Empty(occupiedDates);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_InactiveBookingAlongsideActiveOnes_IsNotCounted()
        {
            // Arrange - two rooms, but only one active booking on the requested date.
            var bookingManager = new BookingManagerBuilder()
                .WithRooms(1, 2)
                .WithBooking(roomId: 1, startOffset: 10, endOffset: 12)
                .WithBooking(roomId: 2, startOffset: 10, endOffset: 12, isActive: false)
                .Build();

            // Act
            var occupiedDates = await bookingManager.GetFullyOccupiedDates(Day(10), Day(12));

            // Assert
            Assert.Empty(occupiedDates);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_BookingsOverlapOnlyPartly_ReturnsTheIntersection()
        {
            // Arrange - room 1 is booked for days 10-14, room 2 for days 12-16,
            // so only days 12-14 are fully occupied.
            var bookingManager = new BookingManagerBuilder()
                .WithRooms(1, 2)
                .WithBooking(roomId: 1, startOffset: 10, endOffset: 14)
                .WithBooking(roomId: 2, startOffset: 12, endOffset: 16)
                .Build();

            // Act
            var occupiedDates = await bookingManager.GetFullyOccupiedDates(Day(1), Day(30));

            // Assert
            Assert.Equal(new[] { Day(12), Day(13), Day(14) }, occupiedDates);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_ValidRange_ReadsBothRepositories()
        {
            // Arrange
            var builder = new BookingManagerBuilder()
                .WithRooms(1, 2)
                .WithAllRoomsOccupied(OccupiedFrom, OccupiedTo);
            var bookingManager = builder.Build();

            // Act
            await bookingManager.GetFullyOccupiedDates(Day(1), Day(30));

            // Assert against the mock objects
            builder.RoomRepository.Verify(r => r.GetAllAsync(), Times.Once);
            builder.BookingRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_StartDateAfterEndDate_RepositoriesAreNotQueried()
        {
            // Arrange
            var builder = new BookingManagerBuilder().WithRooms(1, 2);
            var bookingManager = builder.Build();

            // Act
            await Assert.ThrowsAsync<ArgumentException>(
                () => bookingManager.GetFullyOccupiedDates(Day(5), Day(3)));

            // Assert against the mock objects
            builder.RoomRepository.Verify(r => r.GetAllAsync(), Times.Never);
            builder.BookingRepository.Verify(r => r.GetAllAsync(), Times.Never);
        }

        // Characterisation test. With zero rooms the condition "number of bookings >= number
        // of rooms" is trivially true, so every requested date is reported as fully occupied
        // as long as at least one booking exists. This documents the behaviour of the current
        // implementation; arguably an empty list would be the more sensible answer.
        [Fact]
        public async Task GetFullyOccupiedDates_HotelHasNoRoomsButBookingsExist_ReportsEveryDateAsOccupied()
        {
            // Arrange
            var bookingManager = new BookingManagerBuilder()
                .WithBooking(roomId: 1, startOffset: 10, endOffset: 12)
                .Build();

            // Act
            var occupiedDates = await bookingManager.GetFullyOccupiedDates(Day(1), Day(3));

            // Assert
            Assert.Equal(new[] { Day(1), Day(2), Day(3) }, occupiedDates);
        }
    }
}
