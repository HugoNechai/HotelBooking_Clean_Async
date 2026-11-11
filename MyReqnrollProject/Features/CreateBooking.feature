@booking
Feature: Create booking
    A customer can book an available room for a valid period.
    Existing bookings include both their start and end dates.

Scenario: Booking when only one room is available
    Given the hotel has rooms 1 and 2
    And room 1 is booked from day 10 to 20
    When the customer tries to make a booking from day 12 to 18
    Then the booking should be accepted
    And the booking should be assigned to room 2

Scenario Outline: Booking relative to an occupied period
    Given the hotel has room 1
    And room 1 is booked from day 10 to 20
    When the customer tries to make a booking from day <start> to <end>
    Then the booking should be <result>

Examples:
    | start | end | result   |
    | 8     | 12  | rejected |
    | 12    | 18  | rejected |
    | 21    | 25  | accepted |

Scenario: Booking fails when all rooms overlap the requested period
    Given the hotel has rooms 1 and 2
    And room 1 is booked from day 10 to 20
    And room 2 is booked from day 15 to 23
    When the customer tries to make a booking from day 16 to 19
    Then the booking should be rejected

Scenario: An inactive booking does not block a room
    Given the hotel has room 1
    And room 1 has a cancelled booking from day 15 to 20
    When the customer tries to make a booking from day 17 to 20
    Then the booking should be accepted
    And the booking should be assigned to room 1

Scenario: Booking fails when the hotel has no rooms
    Given the hotel has no rooms
    When the customer tries to make a booking from day 1 to 5
    Then the booking should be rejected

Scenario Outline: Booking fails with invalid date periods
    Given the hotel has room 1
    When the customer tries to make a booking from day <start> to <end>
    Then the booking should be rejected as invalid

Examples:
    | start | end |
    | 5     | 3   |
    | 0     | 5   |
    | -1    | 5   |
