Feature: Successful Booking
  Test successful hotel room bookings.

  Scenario: TC01 - Book an available room
    Given a hotel has an available room
    And the booking dates are valid
    When the customer creates a booking
    Then the booking should succeed

  Scenario: TC07 - Book after an existing booking
    Given a hotel has one room
    And the room is booked from tomorrow to 3 days from now
    When the customer books from 4 days to 6 days from now
    Then the booking should succeed
