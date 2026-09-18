using System;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using System.Collections.Generic;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        IRepository<Booking> bookingRepository;

        public BookingManagerTests(){
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }

        [Fact]
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task FindAvailableRoom_StartDateAfterEndDate_ThrowsArgumentException()
        {
             // Arrange
             DateTime startDate = DateTime.Today.AddDays(5);
             DateTime endDate = DateTime.Today.AddDays(3);

             // Act
             Task result() => bookingManager.FindAvailableRoom(startDate, endDate);

             // Assert
             await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task FindAvailableRoom_AllRoomsOccupied_ReturnsMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(15);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            // Assert
            Assert.Equal(-1, roomId);
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData(10, 15)]
        [InlineData(15, 20)]
        [InlineData(20, 20)]
        public async Task FindAvailableRoom_DatesOverlapFullyOccupiedPeriod_ReturnsMinusOne(
            int startDay, int endDay)
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(startDay);
            DateTime endDate = DateTime.Today.AddDays(endDay);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(startDate, endDate);

            // Assert
            Assert.Equal(-1, roomId);
        }

        [Fact]
        public async Task CreateBooking_RoomAvailable_ReturnsTrue()
        {
            // Arrange
            Booking booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(2)
            };

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateBooking_RoomAvailable_ActivatesBookingAndAssignsRoom()
        {
            // Arrange
            Booking booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(2)
            };

            // Act
            await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(booking.IsActive);
            Assert.NotEqual(0, booking.RoomId);
        }

        [Fact]
        public async Task CreateBooking_RoomAvailable_AddsBookingToRepository()
        {
            // Arrange
            Booking booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(2)
            };

            // Act
            await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(((FakeBookingRepository)bookingRepository).addWasCalled);
        }

        [Fact]
        public async Task CreateBooking_RoomAvailable_AddsBookingToRepository_UsingMoq()
        {
            // Arrange
            var bookingRepositoryMock = new Mock<IRepository<Booking>>();
            var roomRepositoryMock = new Mock<IRepository<Room>>();

            bookingRepositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Booking>());

            roomRepositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Room>
                {
                    new Room { Id = 1, Description = "A" }
                });

            var manager = new BookingManager(
                bookingRepositoryMock.Object,
                roomRepositoryMock.Object);

            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(2)
            };

            // Act
            await manager.CreateBooking(booking);

            // Assert
            bookingRepositoryMock.Verify(
                r => r.AddAsync(booking),
                Times.Once);
        }

        [Fact]
        public async Task CreateBooking_NoRoomAvailable_DoesNotAddBookingToRepository_UsingMoq()
        {
            // Arrange
            var bookingRepositoryMock = new Mock<IRepository<Booking>>();
            var roomRepositoryMock = new Mock<IRepository<Room>>();

            DateTime date = DateTime.Today.AddDays(15);

            bookingRepositoryMock
               .Setup(r => r.GetAllAsync())
               .ReturnsAsync(new List<Booking>
               {
                   new Booking
                   {
                       StartDate = date,
                       EndDate = date,
                       IsActive = true,
                       RoomId = 1
                   }
               });

            roomRepositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Room>
                {
                    new Room { Id = 1, Description = "A" }
                });

            var manager = new BookingManager(
                bookingRepositoryMock.Object,
                roomRepositoryMock.Object);

            var booking = new Booking
            {
                StartDate = date,
                EndDate = date
            };

            // Act
            await manager.CreateBooking(booking);

            // Assert
            bookingRepositoryMock.Verify(
                r => r.AddAsync(booking),
                Times.Never);
        }

        [Fact]
        public async Task CreateBooking_NoRoomAvailable_ReturnsFalse()
        {
            // Arrange
            Booking booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(15),
                EndDate = DateTime.Today.AddDays(15)
            };

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateBooking_NoRoomAvailable_DoesNotAddBookingToRepository()
        {
            // Arrange
            Booking booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(15),
                EndDate = DateTime.Today.AddDays(15)
            };

            // Act
            await bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(((FakeBookingRepository)bookingRepository).addWasCalled);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_StartDateAfterEndDate_ThrowsArgumentException()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(5);
            DateTime endDate = DateTime.Today.AddDays(3);

            // Act
            Task result() => bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_FullyOccupiedDate_ReturnsDate()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(15);

            // Act
            var result = await bookingManager.GetFullyOccupiedDates(date, date);

            // Assert
            Assert.Contains(date, result);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_DateNotFullyOccupied_DoesNotReturnDate()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(5);

            // Act
            var result = await bookingManager.GetFullyOccupiedDates(date, date);

            // Assert
            Assert.DoesNotContain(date, result);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_Range_ReturnsAllFullyOccupiedDates()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(8);
            DateTime endDate = DateTime.Today.AddDays(22);

            // Act
            var result = await bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.Equal(11, result.Count);

            for (int day = 10; day <= 20; day++)
            {
                Assert.Contains(DateTime.Today.AddDays(day), result);
            }
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // This test was added to satisfy the following test design
            // principle: "Tests should have strong assertions".

            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            var bookingForReturnedRoomId = (await bookingRepository.GetAllAsync()).
                Where(b => b.RoomId == roomId
                           && b.StartDate <= date
                           && b.EndDate >= date
                           && b.IsActive);
            
            // Assert
            Assert.Empty(bookingForReturnedRoomId);
        }

    }
}
