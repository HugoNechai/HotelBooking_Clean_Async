using System;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.UnitTests.TestDoubles;
using Moq;
using Xunit;
using static HotelBooking.UnitTests.TestDoubles.BookingManagerBuilder;

namespace HotelBooking.UnitTests
{
    // Tests for BookingManager.FindAvailableRoom.
    //
    // Unless a test builds its own hotel, the scenario is: two rooms (1 and 2), both booked
    // from day 10 to day 20 inclusive. The data-driven tests below walk the equivalence
    // classes of a requested period relative to that occupied period, plus the boundaries
    // between them.
    public class BookingManagerFindAvailableRoomTests
    {
        private const int OccupiedFrom = 10;
        private const int OccupiedTo = 20;

        private static BookingManager FullyBookedHotel() =>
            new BookingManagerBuilder()
                .WithRooms(1, 2)
                .WithAllRoomsOccupied(OccupiedFrom, OccupiedTo)
                .Build();

        // Data-driven with inline data: each row is one equivalence class or boundary.
        [Theory]
        // Requested period lies completely before the occupied period.
        [InlineData(1, 5, true)]
        // Requested period ends the day before the occupied period starts (boundary).
        [InlineData(1, 9, true)]
        // Requested period ends on the first occupied day (boundary).
        [InlineData(1, 10, false)]
        // Requested period overlaps the beginning of the occupied period.
        [InlineData(5, 15, false)]
        // Requested period encloses the occupied period.
        [InlineData(1, 30, false)]
        // Requested period starts on the first occupied day (boundary).
        [InlineData(10, 10, false)]
        // Requested period lies completely inside the occupied period.
        [InlineData(12, 18, false)]
        // Requested period ends on the last occupied day (boundary).
        [InlineData(15, 20, false)]
        // Requested period starts on the last occupied day (boundary).
        [InlineData(20, 25, false)]
        // Requested period overlaps the end of the occupied period.
        [InlineData(18, 25, false)]
        // Requested period starts the day after the occupied period ends (boundary).
        [InlineData(21, 21, true)]
        // Requested period lies completely after the occupied period.
        [InlineData(25, 30, true)]
        public async Task FindAvailableRoom_PeriodRelativeToOccupiedPeriod_ReturnsExpectedAvailability(
            int startOffset, int endOffset, bool expectedToBeAvailable)
        {
            // Arrange
            var bookingManager = FullyBookedHotel();

            // Act
            int roomId = await bookingManager.FindAvailableRoom(Day(startOffset), Day(endOffset));

            // Assert
            Assert.Equal(expectedToBeAvailable, roomId != -1);
        }

        // Strong assertion: the returned room must really be free for the whole period,
        // not just "some id that is not -1".
        [Theory]
        [InlineData(1, 5)]
        [InlineData(1, 9)]
        [InlineData(21, 21)]
        [InlineData(25, 30)]
        public async Task FindAvailableRoom_RoomIsAvailable_ReturnedRoomHasNoOverlappingActiveBooking(
            int startOffset, int endOffset)
        {
            // Arrange
            var builder = new BookingManagerBuilder()
                .WithRooms(1, 2)
                .WithAllRoomsOccupied(OccupiedFrom, OccupiedTo);
            var bookingManager = builder.Build();
            DateTime startDate = Day(startOffset), endDate = Day(endOffset);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(startDate, endDate);

            // Assert
            Assert.NotEqual(-1, roomId);
            var allBookings = await builder.BookingRepository.Object.GetAllAsync();
            var overlapping = allBookings.Where(b => b.IsActive
                                                     && b.RoomId == roomId
                                                     && b.StartDate <= endDate
                                                     && b.EndDate >= startDate);
            Assert.Empty(overlapping);
        }

        // Data-driven with member data: every way of passing an invalid period.
        public static TheoryData<int, int> InvalidPeriods => new TheoryData<int, int>
        {
            { 0, 0 },    // start date is today - must be strictly in the future
            { 0, 5 },    // start date is today, end date in the future
            { -1, 5 },   // start date is yesterday
            { -10, -5 }, // whole period is in the past
            { 5, 3 },    // start date is later than the end date
            { 30, 1 }    // start date is far later than the end date
        };

        [Theory]
        [MemberData(nameof(InvalidPeriods))]
        public async Task FindAvailableRoom_InvalidPeriod_ThrowsArgumentException(int startOffset, int endOffset)
        {
            // Arrange
            var bookingManager = FullyBookedHotel();

            // Act
            Task result() => bookingManager.FindAvailableRoom(Day(startOffset), Day(endOffset));

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task FindAvailableRoom_OnlyOneOfTwoRoomsOccupied_ReturnsTheFreeRoom()
        {
            // Arrange
            var bookingManager = new BookingManagerBuilder()
                .WithRooms(1, 2)
                .WithBooking(roomId: 1, startOffset: OccupiedFrom, endOffset: OccupiedTo)
                .Build();

            // Act
            int roomId = await bookingManager.FindAvailableRoom(Day(12), Day(18));

            // Assert
            Assert.Equal(2, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_OverlappingBookingIsInactive_RoomIsTreatedAsFree()
        {
            // Arrange - a cancelled booking must not block the room.
            var bookingManager = new BookingManagerBuilder()
                .WithRooms(1)
                .WithBooking(roomId: 1, startOffset: OccupiedFrom, endOffset: OccupiedTo, isActive: false)
                .Build();

            // Act
            int roomId = await bookingManager.FindAvailableRoom(Day(12), Day(18));

            // Assert
            Assert.Equal(1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_HotelHasNoRooms_ReturnsMinusOne()
        {
            // Arrange
            var bookingManager = new BookingManagerBuilder().Build();

            // Act
            int roomId = await bookingManager.FindAvailableRoom(Day(1), Day(2));

            // Assert
            Assert.Equal(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_NoBookingsExist_ReturnsFirstRoom()
        {
            // Arrange
            var bookingManager = new BookingManagerBuilder().WithRooms(7, 8).Build();

            // Act
            int roomId = await bookingManager.FindAvailableRoom(Day(1), Day(100));

            // Assert
            Assert.Equal(7, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_ValidPeriod_ReadsBothRepositories()
        {
            // Arrange
            var builder = new BookingManagerBuilder().WithRooms(1).WithAllRoomsOccupied(OccupiedFrom, OccupiedTo);
            var bookingManager = builder.Build();

            // Act
            await bookingManager.FindAvailableRoom(Day(1), Day(2));

            // Assert against the mock objects
            builder.BookingRepository.Verify(r => r.GetAllAsync(), Times.Once);
            builder.RoomRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task FindAvailableRoom_InvalidPeriod_RepositoriesAreNotQueried()
        {
            // Arrange - the guard clause must reject the call before any data is read.
            var builder = new BookingManagerBuilder().WithRooms(1);
            var bookingManager = builder.Build();

            // Act
            await Assert.ThrowsAsync<ArgumentException>(
                () => bookingManager.FindAvailableRoom(Day(5), Day(3)));

            // Assert against the mock objects
            builder.BookingRepository.Verify(r => r.GetAllAsync(), Times.Never);
            builder.RoomRepository.Verify(r => r.GetAllAsync(), Times.Never);
        }
    }
}
