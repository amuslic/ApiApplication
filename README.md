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

If possible fix the grpc implementation and communicate through it with third party provider


## Solution

