Feature: Failed Booking
  Test invalid bookings and unavailable rooms.

  Scenario: TC02 - All rooms are booked
    Given all hotel rooms are booked for the requested dates
    When the customer creates a booking for those dates
    Then the booking should fail

  Scenario: TC03 - Start date is in the past
    Given a hotel has an available room
    When the customer books from yesterday to tomorrow
    Then the booking should be rejected

  Scenario: TC04 - Start date is today
    Given a hotel has an available room
    When the customer books from today to tomorrow
    Then the booking should be rejected

  Scenario: TC05 - End date is before start date
    Given a hotel has an available room
    When the customer books with an end date before the start date
    Then the booking should be rejected

  Scenario: TC06 - Booking dates overlap
    Given a hotel has one room
    And the room is booked from tomorrow to 5 days from now
    When the customer books from 2 days to 7 days from now
    Then the booking should fail

  Scenario: TC08 - Hotel has no rooms
    Given a hotel has no rooms
    When the customer creates a booking with valid dates
    Then the booking should fail
