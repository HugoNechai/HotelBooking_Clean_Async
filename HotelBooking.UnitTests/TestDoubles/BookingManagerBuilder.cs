using System;
using System.Collections.Generic;
using System.Linq;
using HotelBooking.Core;
using Moq;

namespace HotelBooking.UnitTests.TestDoubles
{
    // Builds a BookingManager on top of mocked repositories, so every test can describe
    // exactly the rooms and bookings its own scenario needs instead of sharing one fixed
    // fake. The mocks stay reachable through the properties so tests can also verify the
    // interaction between the manager and its repositories.
    public class BookingManagerBuilder
    {
        private readonly List<Room> rooms = new List<Room>();
        private readonly List<Booking> bookings = new List<Booking>();

        public Mock<IRepository<Room>> RoomRepository { get; } = new Mock<IRepository<Room>>();
        public Mock<IRepository<Booking>> BookingRepository { get; } = new Mock<IRepository<Booking>>();

        // All test data is expressed as a number of days from today, so the tests never
        // depend on the calendar date they happen to run on.
        public static DateTime Day(int offsetFromToday) => DateTime.Today.AddDays(offsetFromToday);

        public BookingManagerBuilder WithRooms(params int[] roomIds)
        {
            rooms.AddRange(roomIds.Select(id => new Room { Id = id, Description = "Room " + id }));
            return this;
        }

        public BookingManagerBuilder WithBooking(int roomId, int startOffset, int endOffset, bool isActive = true)
        {
            bookings.Add(new Booking
            {
                Id = bookings.Count + 1,
                RoomId = roomId,
                CustomerId = 1,
                StartDate = Day(startOffset),
                EndDate = Day(endOffset),
                IsActive = isActive
            });
            return this;
        }

        // Books every room added so far for the given period.
        public BookingManagerBuilder WithAllRoomsOccupied(int startOffset, int endOffset)
        {
            foreach (var room in rooms.ToList())
                WithBooking(room.Id, startOffset, endOffset);
            return this;
        }

        public BookingManager Build()
        {
            RoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            BookingRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
            return new BookingManager(BookingRepository.Object, RoomRepository.Object);
        }
    }
}
