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

         // Check if the user is authenticated
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }
        // Retrieve the user ID from the JWT token
        string userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(new { message = "User ID not found in token." });
        }

        Console.WriteLine($"Authenticated User ID: {userId}");

        try
        {            // Query the laundry room associated with the user's complex

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
            // Fetch timeslots and join them with laundry rooms
            var timeslots = _context.Timeslots
                .Join(_context.LaundryRooms, t => t.ComplexId, lr => lr.ComplexId,
                    (t, lr) => new { Timeslot = t, LaundryRoom = lr })
                .Where(joinResult => joinResult.LaundryRoom.LaundryRoomId == id)
                .Select(joinResult => new
                {
                    timeslotId = joinResult.Timeslot.TimeslotId,
                    startTime = joinResult.Timeslot.StartTime,
                    endTime = joinResult.Timeslot.EndTime
                })
                .ToList();

            if (!timeslots.Any())
            {
                return NotFound(new { message = "No timeslots found for this laundry room." });
            }

            // Shape the response to avoid $id and $values. These are added by Entity framwork and a hassle on the frontend...
            // Also Format the response to avoid issues with serialization

            var formattedResponse = timeslots.Select(ts => new
            {
                timeslotId = ts.timeslotId,
                startTime = ts.startTime,
                endTime = ts.endTime
            }).ToList();

            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles // Avoid $id and $values
            };

            return new JsonResult(new { timeslots = formattedResponse }, options);
        }
        catch (Exception e)
        {
            // Log the error and return a server error response
            Console.WriteLine($"Error fetching timeslots for laundry room {id}: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }





    // 3. Get Accessible Laundry Rooms and Machines for an admin
   [HttpGet("laundry-room/accessible/{userId}")]
public IActionResult GetAccessibleLaundryRoomsAndMachines(int userId)
{
    try
    {
        // Step 1: Get accessible complex IDs for the user
        var accessibleComplexIds = _context.LivesIn
            .Where(li => li.UserId == userId)
            .Select(li => li.ComplexId)
            .Concat(
                _context.AdminManages
                    .Where(am => am.UserId == userId)
                    .Select(am => am.ComplexId)
            )
            .Distinct()
            .ToList();

        if (!accessibleComplexIds.Any())
        {
            Console.WriteLine("No accessible complexes found for the user.");
            return NotFound(new { message = "No accessible complexes found for the user." });
        }

        // Step 2: Get laundry rooms and related machines for the accessible complexes
        var accessibleRooms = _context.LaundryRooms
            .Where(lr => accessibleComplexIds.Contains(lr.ComplexId))
            .Include(lr => lr.LaundryMachines)
            .ToList();

        if (!accessibleRooms.Any())
        {
            Console.WriteLine("No accessible laundry rooms found for the complexes.");
            return NotFound(new { message = "No accessible laundry rooms found for the complexes." });
        }

        // Step 3: Shape the response to avoid $values
        var formattedResponse = accessibleRooms.Select(lr => new
        {
            laundryRoomId = lr.LaundryRoomId,
            roomName = lr.RoomName,
            machineDtos = lr.LaundryMachines
                .Select(m => new
                {
                    machineId = m.MachineId,
                    machineName = m.MachineName,
                    machineType = m.MachineType,
                    status = m.Status,
                    laundryRoomId = m.LaundryRoomId
                })
                .ToList()
        })
        .ToList();

        var options = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles // Avoid $id and $values
        };

        return new JsonResult(new { rooms = formattedResponse }, options);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error fetching accessible rooms and machines: " + ex.Message);
        return StatusCode(500, "Internal server error");
    }
}



    // DTO for LaundryRoom
    
}
