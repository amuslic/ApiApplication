## Movies challenge

## Requirements
- **Create showtime**
    Should create showtime and should grab the movie data from the ProvidedApi.
    
- **Reserve seats**
    - Reserving the seat response will contain a GUID of the reservation, also the number of seats, the auditorium used and the movie that will be played.
    - After 10 minutes after the reservation is created, the reservation is considered expired by the system.
    - It should not be possible to reserve the same seats two times in 10 minutes.
    - It shouldn't be possible to reserve an already sold seat.
    - All the seats, when doing a reservation, need to be contiguous.
- **Buy seats**
    - We will need the GUID of the reservation, it is only possible to do it while the seats are reserved.
    - It is not possible to buy the same seat two times.
    - Expired reservations (older than 10 minutes) cannot be confirmed.
    - We are not going to use a Payment abstraction for this case, just have an Endpoint which I can use to Confirm a Reservation.

### Cache

We will like to have a cache layer to cache the response from the Provided API because the API is slow and fails a lot. We will like to call the API and in case of failure try to use the cached response. The cache should use the Redis container provided in the docker-compose.yaml

### Execution Tracking

We want to track the execution time of each request done to the service and log the time in the Console.
By default, we set the loggers to log in to the Console, so you only need to worry where to put the Logger in the code.

### Testing

Please provider tests for implemented route 

### Third party provider

If possible communicate with third party provider through grpc


## Solution
To initiate the application, please follow these steps:

1. **Clone the Repository**: Start by cloning the repository to your local machine.
2. **Install Docker**: Ensure Docker is installed and running on your system. Docker is required to manage the application's containers.
3. **Run Docker Compose**: Navigate to the project's root directory and execute `docker-compose up`. This command starts the necessary services, including the third-party provider, Redis instance, and an SQL in-memory database that generates sample data upon application start.

The application is accessible via `http://localhost/swagger/index.html` by default. Here, you can explore and interact with the API through the Swagger UI, which simplifies the process of testing various endpoints.

## Architecture and Implementation Details

- **MediatR and CQRS Pattern**: The solution implements the MediatR library to follow the CQRS (Command Query Responsibility Segregation) pattern
- **Grpy**: Communication with third party movie provider is done via grpc client
- **Redis Cache**: A Redis cache is utilized to store responses from the Provided API. The cache uses protobuf serialization for efficient data storage, which is not human-readable if accessed via a viewer.
- **Execution Tracking**: Execution times for requests are tracked and logged to the console through MediatR pipeline behavior.
- **Exception Handling**: The API leverages a .NET exception filter (`onActionExecuted`) to handle exceptions
- **Layered Architecture**: The project is structured into distinct layers (Application, API, and Infrastructure) to separate concerns, which are for simplicitly represented in folders and not libraries

## Testing

- **Integration Tests**: The system includes integration tests that utilize their own in-memory SQL database

## Running Tests

Integration tests can be executed through your preferred IDE or command line tool, ensuring that all components function correctly in isolation and when integrated.

