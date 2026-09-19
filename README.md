# HotelBooking_Clean_Async

A hotel booking system used for the Software Quality mini-project.

The main focus of this project is testing the business logic in `HotelBooking.Core` and setting up automated code quality checks.

## What We Implemented

### Unit Testing

Unit tests were created for the business logic in `BookingManager`.

The following methods are tested:

- `CreateBooking`
- `FindAvailableRoom`
- `GetFullyOccupiedDates`

The tests cover successful and unsuccessful booking scenarios, available and occupied rooms, invalid dates, and fully occupied date ranges.

The tests are written using **xUnit**.

### Data-Driven Testing

Data-driven testing is implemented using xUnit:

- `[Theory]`
- `[InlineData]`

This is used to test multiple date combinations that overlap with a fully occupied period.

### Mocking

**Moq** is used as the mocking framework.

Mocks of the booking and room repositories are used to verify important interactions, for example:

- `AddAsync()` is called when a booking can be created.
- `AddAsync()` is not called when no room is available.

The project also contains fake repositories used by other unit tests.

### Code Coverage

**Coverlet** is used to collect code coverage from the unit tests.

`BookingManager.cs`, which contains the business logic in `HotelBooking.Core`, has **100% line coverage and 100% branch coverage** in the local Coverlet results.

Coverage results are also sent to SonarQube through the CI pipeline.

### SonarQube

**SonarQube Cloud** is used for static code analysis.

It provides information about:

- reliability
- maintainability
- security
- code coverage
- duplicated code
- code quality issues

The current SonarQube Quality Gate passes.

### Continuous Integration

A **GitHub Actions** CI workflow is configured for the project.

The workflow runs when changes are pushed to `main` or when a pull request targets `main`.

It automatically:

1. Restores the project dependencies.
2. Builds the solution.
3. Runs the unit tests.
4. Collects code coverage.
5. Runs SonarQube analysis.
6. Sends the analysis and coverage results to SonarQube Cloud.

The current CI workflow runs successfully.

## Technologies and Tools

- C# / .NET
- xUnit
- Moq
- Coverlet
- SonarQube Cloud
- GitHub Actions

## Project Structure

The main business logic is located in:

`HotelBooking.Core/Services/BookingManager.cs`

The unit tests are located in:

`HotelBooking.UnitTests/BookingManagerTests.cs`

The CI configuration is located in:

`.github/workflows/main.yml`