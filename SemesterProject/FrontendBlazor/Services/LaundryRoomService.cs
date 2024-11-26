using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using FrontendBlazor.Model.DTOs;

public class LaundryRoomService
{
    private readonly HttpClient _httpClient;

    // Inject HttpClient via constructor
    public LaundryRoomService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public List<LaundryRoomDto> AccessibleRooms { get; private set; } = new();
    public List<LaundryMachineDto> AccessibleMachines { get; private set; } = new();


    // Get timeslots for a specific laundry room
    public async Task<List<TimeSlotDto>> GetTimeSlotsForLaundryRoom(int laundryRoomId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<TimeSlotDto>>($"/api/LaundryRoom/laundry-room/{laundryRoomId}");
            return response ?? new List<TimeSlotDto>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching timeslots for laundry room: {ex.Message}");
            throw;
        }
    }

    // Get accessible laundry rooms and machines for a user
    public async Task<(List<LaundryRoomDto> Rooms, List<LaundryMachineDto> Machines)> GetAccessibleLaundryRoomsAndMachines(int userId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<AccessibleLaundryRoomsDto>($"/api/LaundryRoom/laundry-room/accessible/{userId}");

            if (response == null)
            {
                return (new List<LaundryRoomDto>(), new List<LaundryMachineDto>());
            }

            var rooms = response.Rooms;
            var machines = new List<LaundryMachineDto>();

            foreach (var room in response.Rooms)
            {
                machines.AddRange(room.MachineDtos);
            }

            return (rooms, machines);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching accessible rooms and machines: {ex.Message}");
            throw;
        }
    }
    
    public async Task FetchAccessibleRoomsAndMachinesAsync(int userId)
    {
        var (rooms, machines) = await GetAccessibleLaundryRoomsAndMachines(userId);
        AccessibleRooms = rooms;
        AccessibleMachines = machines;
    }
}