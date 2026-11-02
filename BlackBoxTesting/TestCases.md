# Black-box Testing — Create Booking

## Test Techniques
- **Equivalence Partitioning** — test valid and invalid dates.
- **Boundary Value Analysis** — test dates at their limits.
- **Decision Table Testing** — test room availability and booking conflicts.

## TC01 — Successful Booking
- Start Date: Tomorrow
- End Date: Tomorrow + 2 days
- Room: Available
- Expected: Booking succeeds.

## TC02 — All Rooms Booked
- Start Date: Tomorrow
- End Date: Tomorrow + 2 days
- Room: All rooms already booked for these dates
- Expected: Booking fails.

## TC03 — Past Start Date
- Start Date: Yesterday
- End Date: Tomorrow
- Expected: Booking is rejected.

## TC04 — Today's Start Date
- Start Date: Today
- End Date: Tomorrow
- Expected: Booking is rejected.

## TC05 — Invalid Date Order
- Start Date: Tomorrow + 5 days
- End Date: Tomorrow + 2 days
- Expected: Booking is rejected.

## TC06 — Overlapping Booking
- Existing Booking: Tomorrow to Tomorrow + 5 days
- New Booking: Tomorrow + 2 days to Tomorrow + 7 days
- Room: Only one room available in the hotel
- Expected: Booking fails.

## TC07 — Booking After Existing Booking
- Existing Booking: Tomorrow to Tomorrow + 3 days
- New Booking: Tomorrow + 4 days to Tomorrow + 6 days
- Expected: Booking succeeds.

## TC08 — No Rooms in Hotel
- Start Date: Tomorrow
- End Date: Tomorrow + 2 days
- Room: No rooms exist
- Expected: Booking fails.