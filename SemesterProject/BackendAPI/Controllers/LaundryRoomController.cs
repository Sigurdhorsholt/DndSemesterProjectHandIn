using System.Security.Claims;
using BackendAPI.DataAccess;
using BackendAPI.DataAccess.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LaundryRoomController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LaundryRoomController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Get User's Laundry Room
    [HttpGet("user-laundry-room")]
    public IActionResult GetUserLaundryRoom()
    {
        Console.WriteLine("GetUserLaundryRoom endpoint is being hit successfully...");

        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        string userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(new { message = "User ID not found in token." });
        }

        Console.WriteLine($"Authenticated User ID: {userId}");

        try
        {
            var laundryRoom = _context.LaundryRooms
                .Join(_context.LivesIn, lr => lr.ComplexId, li => li.ComplexId,
                    (lr, li) => new { LaundryRoom = lr, LivesIn = li })
                .Where(joinResult => joinResult.LivesIn.UserId == userId)
                .Select(joinResult => new LaundryRoomDto
                {
                    LaundryRoomId = joinResult.LaundryRoom.LaundryRoomId,
                    RoomName = joinResult.LaundryRoom.RoomName,
                    ComplexId = joinResult.LaundryRoom.ComplexId
                })
                .FirstOrDefault();

            if (laundryRoom == null)
            {
                return NotFound(new { message = "Laundry-room not found associated with this user." });
            }

            return Ok(laundryRoom);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    // 2. Get Time Slots for a Specific Laundry Room
    [HttpGet("laundry-room/{id}")]
    public IActionResult GetTimeSlotsForLaundryRoom(int id)
    {
        try
        {
            var timeslots = _context.Timeslots
                .Join(_context.LaundryRooms, t => t.ComplexId, lr => lr.ComplexId,
                    (t, lr) => new { Timeslot = t, LaundryRoom = lr })
                .Where(joinResult => joinResult.LaundryRoom.LaundryRoomId == id)
                .Select(joinResult => new TimeslotDto
                {
                    TimeslotId = joinResult.Timeslot.TimeslotId,
                    StartTime = joinResult.Timeslot.StartTime,
                    EndTime = joinResult.Timeslot.EndTime
                })
                .ToList();
            
            //returned from DB un sorted. lets sort them herre: 
            timeslots = timeslots.OrderBy(ts => ts.StartTime).ToList();


            if (timeslots.Count == 0)
            {
                return NotFound(new { message = "No timeslots found for this laundry room." });
            }

            return Ok(timeslots);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    // 3. Get Accessible Laundry Rooms and Machines for a admin
    // 3. Get Accessible Laundry Rooms and Machines for an admin
    [HttpGet("laundry-room/accessible/{userId}")]
    public IActionResult GetAccessibleLaundryRoomsAndMachines(int userId)
    {
        try
        {
            // Step 1: Get accessible complex IDs for the user (either lived-in or managed)
            var accessibleComplexIds = _context.LivesIn
                .Where(li => li.UserId == userId)
                .Select(li => li.ComplexId)
                .Concat(
                    _context.AdminManages
                        .Where(am => am.UserId == userId)
                        .Select(am => am.ComplexId)
                )
                .Distinct() // Use Distinct to remove duplicates
                .ToList();

            // Step 2: Get laundry rooms and related machines for the accessible complexes
            var accessibleRooms = _context.LaundryRooms
                .Where(lr => accessibleComplexIds.Contains(lr.ComplexId))  // Corrected from ApartmentComplexId to ComplexId
                .Include(lr => lr.LaundryMachines)
                .ToList();

            // Step 3: Map to a list of anonymous objects (or DTOs if you prefer)
            var roomsAndMachines = accessibleRooms.Select(lr => new
            {
                RoomId = lr.LaundryRoomId,
                RoomName = lr.RoomName,
                Machines = lr.LaundryMachines.Select(m => new
                {
                    MachineId = m.MachineId,
                    MachineName = m.MachineName
                }).ToList()
            }).ToList();

            return Ok(new { rooms = roomsAndMachines });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching accessible rooms and machines: " + ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


    // DTO for LaundryRoom
    
}
