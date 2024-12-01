using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using FrontendBlazor.Model.DTOs;
using Microsoft.AspNetCore.Components;

public class LaundryRoomService
{
    private readonly HttpClient _httpClient;

    [Inject]
    private BookingService _BookingService { get; set; }

    // Inject HttpClient via constructor
    public LaundryRoomService(HttpClient httpClient, BookingService bookingService)
    {
        _httpClient = httpClient;
        _BookingService = bookingService;
    }
    
    public List<LaundryRoomDto> AccessibleRooms { get; private set; } = new();
    public List<LaundryMachineDto> AccessibleMachines { get; private set; } = new();
    public List<TimeSlotDto> TimeSlotsFromSettings { get; private set; } = new();
    public ComplexSettingsDto ComplexSettings { get; private set; } = new();
    
    public List<BookingDto> BookingsForLaundryRoom { get; private set; } = new();
    
    


    // Get timeslots for a specific laundry room
    public async Task<List<TimeSlotDto>> GetTimeSlotsForLaundryRoom(int laundryRoomId)
    {
        try
        {
            // Fetch JSON from the backend
            var jsonResponse = await _httpClient.GetStringAsync($"/api/LaundryRoom/laundry-room/{laundryRoomId}");
         //   Console.WriteLine($"Raw JSON Response: {jsonResponse}");
            // Deserialize the response to match the new structure
            var response = System.Text.Json.JsonSerializer.Deserialize<TimeSlotsResponseDto>(jsonResponse, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            
            return response?.Timeslots ?? new List<TimeSlotDto>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching timeslots for laundry room: {ex.Message}");
            throw;
        }
    }

    // Get accessible laundry rooms and machines for a user
    public async Task<List<LaundryRoomDto>> GetAccessibleLaundryRooms(int userId)
    {
        try
        {
            // Fetch JSON from the backend
            var jsonResponse = await _httpClient.GetStringAsync($"/api/LaundryRoom/laundry-room/accessible/{userId}");
           // Console.WriteLine($"Raw JSON Response: {jsonResponse}");

            // Deserialize the response
            var response = System.Text.Json.JsonSerializer.Deserialize<AccessibleLaundryRoomsDto>(jsonResponse, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response?.Rooms == null || response.Rooms.Count == 0)
            {
                Console.WriteLine("No rooms found in the response.");
                return new List<LaundryRoomDto>();
            }

            return response.Rooms;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching accessible rooms and machines: {ex.Message}");
            throw;
        }
    }


    public async Task  SetAccessibleRooms(int userId)
    {
        AccessibleRooms = await GetAccessibleLaundryRooms(userId);
    }

    public async Task SetTimeSlots(int LaundryRoomId)
    {
        var unsortedTimeSlots = await GetTimeSlotsForLaundryRoom(LaundryRoomId);
        unsortedTimeSlots.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
        TimeSlotsFromSettings = new List<TimeSlotDto>(unsortedTimeSlots);

    }


    public async Task SetSettingsForLaundryRoom(int laundryRoomId)
    {
        try
        {
            ComplexSettings = await GetSettingsForLaundryRoom(laundryRoomId);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
    
   
    
    

    public async Task<ComplexSettingsDto> GetSettingsForLaundryRoom(int laundryRoomId)
    {
        
        try
        {
            // Fetch settings for the laundry room from the API
            var settings = await _httpClient.GetFromJsonAsync<ComplexSettingsDto>($"/api/Settings/settings/{laundryRoomId}");
            if (settings == null)
            {
                Console.WriteLine($"No settings found for Laundry Room ID: {laundryRoomId}");
                return new ComplexSettingsDto();
            }

            return settings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching settings for Laundry Room ID {laundryRoomId}: {ex.Message}");
            throw;
        }
        
    }

    public async Task SetBookingsForLaundryRoom(int laundryRoomId)
    {
        try
        {
            BookingsForLaundryRoom = await _BookingService.GetAllBookingsForLaundryRoom(laundryRoomId);

        }   catch (Exception ex)
        {
            Console.WriteLine($"Error fetching settings for Laundry Room ID {laundryRoomId}: {ex.Message}");
            throw;
        }
        
    }
    
    public async Task<bool> UpdateSettingsForLaundryRoom(int laundryRoomId, ComplexSettingsDto updatedSettings)
    {
        try
        {
            // Prepare the API endpoint URL
            var url = $"/api/Settings/settings/{laundryRoomId}";

            // Log the settings being sent for debugging
            Console.WriteLine($"Sending settings for Laundry Room ID {laundryRoomId}: {System.Text.Json.JsonSerializer.Serialize(updatedSettings)}");

            // Send updated settings as JSON to the API
            var response = await _httpClient.PostAsJsonAsync(url, updatedSettings);

            // Check the response status
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Settings updated successfully for Laundry Room ID: " + laundryRoomId);
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to update settings. Status Code: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating settings for Laundry Room ID {laundryRoomId}: {ex.Message}");
            throw;
        }
    }


   public async Task<List<UserDto>> GetUsersForLaundryRoom(int laundryRoomId)
{
    try
    {
        // Send GET request to fetch users for the specified laundry room
        var response = await _httpClient.GetFromJsonAsync<UsersResponseDto>($"/api/User/laundryroom/{laundryRoomId}/users");

        
        if (response?.Users == null || response.Users.Count == 0)
        {
            Console.WriteLine($"No users found for laundry room ID: {laundryRoomId}");
            return new List<UserDto>();
        }

        return response.Users;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching users for laundry room ID {laundryRoomId}: {ex.Message}");
        throw;
    }
}

public async Task RegisterUser(UserRegistrationDto newUser)
{
    try
    {
        // Send POST request to register a new user
        var response = await _httpClient.PostAsJsonAsync("/api/User", newUser);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to register user. Status Code: {response.StatusCode}");
            var errorDetails = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error registering user: {errorDetails}");
        }

        Console.WriteLine($"User registered successfully: {newUser.UserName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error registering user: {ex.Message}");
        throw;
    }
}

public async Task DeleteUser(int userId)
{
    try
    {
        // Send DELETE request to remove the user
        var response = await _httpClient.DeleteAsync($"/api/User/{userId}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to delete user ID {userId}. Status Code: {response.StatusCode}");
            var errorDetails = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error deleting user: {errorDetails}");
        }

        Console.WriteLine($"User deleted successfully: ID {userId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting user ID {userId}: {ex.Message}");
        throw;
    }
}

}
