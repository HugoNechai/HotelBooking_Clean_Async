using Xunit;

namespace HotelBooking.UnitTests.TestData
{
    // ClassData source for GetFullyOccupiedDates.
    //
    // Every row assumes the standard scenario used by BookingManagerGetFullyOccupiedDatesTests:
    // a hotel with two rooms, both booked from day 10 to day 20 (inclusive).
    //
    // Columns: start day offset, end day offset, expected number of fully occupied dates.
    public class OccupiedDateRanges : TheoryData<int, int, int>
    {
        public OccupiedDateRanges()
        {
            // Range lies completely before the occupied period.
            Add(1, 5, 0);

            // Range ends the day before the occupied period starts (lower boundary).
            Add(1, 9, 0);

            // Range ends on the first occupied day (lower boundary, inclusive).
            Add(1, 10, 1);

            // Range overlaps the beginning of the occupied period.
            Add(8, 12, 3);

            // Range lies completely inside the occupied period.
            Add(12, 18, 7);

            // Range overlaps the end of the occupied period.
            Add(18, 25, 3);

            // Range starts on the last occupied day (upper boundary, inclusive).
            Add(20, 25, 1);

            // Range starts the day after the occupied period ends (upper boundary).
            Add(21, 25, 0);

            // Range lies completely after the occupied period.
            Add(25, 30, 0);

            // Range encloses the whole occupied period.
            Add(1, 30, 11);

            // Single day inside the occupied period.
            Add(15, 15, 1);

            // Single day outside the occupied period.
            Add(9, 9, 0);

            // Dates in the past are allowed here - unlike FindAvailableRoom, this method
            // only rejects a start date that is later than the end date.
            Add(-10, 30, 11);
        }
    }
}
