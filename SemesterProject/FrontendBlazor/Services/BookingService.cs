using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using FrontendBlazor.Model.DTOs;

public class BookingService
{
    private readonly HttpClient _httpClient;

    // Inject the HttpClient via constructor
    public BookingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 1. Get all bookings for a specific user
    public async Task<List<BookingDto>> GetBookingsForUser(int userId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<BookingDto>>($"/api/bookings/user/{userId}");
            return response ?? new List<BookingDto>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching bookings for user: {ex.Message}");
            throw;
        }
    }

    // 2. Get all bookings for a specific laundry room
    public async Task<List<BookingDto>> GetAllBookingsForLaundryRoom(int laundryRoomId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<BookingDto>>($"/api/laundryroom/{laundryRoomId}");
            return response ?? new List<BookingDto>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching bookings for laundry room: {ex.Message}");
            throw;
        }
    }

    // 3. Get a specific booking by booking ID
    public async Task<BookingDto> GetBookingById(int bookingId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<BookingDto>($"/api/bookings/{bookingId}");
            return response;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching booking by ID: {ex.Message}");
            throw;
        }
    }

    // 4. Get all bookings for a specific machine
    public async Task<List<BookingDto>> GetBookingsForMachine(int machineId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<BookingDto>>($"/api/bookings/machine/{machineId}");
            return response ?? new List<BookingDto>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching bookings for machine: {ex.Message}");
            throw;
        }
    }

    // 5. Get all upcoming bookings (from today onward)
    public async Task<List<BookingDto>> GetUpcomingBookings()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<BookingDto>>($"/api/bookings/upcoming");
            return response ?? new List<BookingDto>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching upcoming bookings: {ex.Message}");
            throw;
        }
    }

    // 6. Get all bookings for a specific room and date
    public async Task<List<BookingDto>> GetBookingsForRoomAndDate(int laundryRoomId, string bookingDate)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<BookingDto>>($"/api/laundryroom/{laundryRoomId}/date/{bookingDate}");
            return response ?? new List<BookingDto>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching bookings for room and date: {ex.Message}");
            throw;
        }
    }

    // Create a booking
    public async Task<BookingDto> CreateBooking(int userId, int machineId, string bookingDate, int timeslotId, int laundryRoomId)
    {
        try
        {
            var bookingRequest = new
            {
                UserId = userId,
                MachineId = machineId,
                BookingDate = bookingDate,
                TimeslotId = timeslotId,
                LaundryRoomId = laundryRoomId
            };

            var response = await _httpClient.PostAsJsonAsync($"/api/booking", bookingRequest);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BookingDto>();
            }
            else
            {
                Console.WriteLine($"Error creating booking. Status Code: {response.StatusCode}");
                throw new HttpRequestException("Failed to create booking");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error creating booking: {ex.Message}");
            throw;
        }
    }
}
