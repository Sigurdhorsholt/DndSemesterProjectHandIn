using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using BackendAPI.DataAccess;
using BackendAPI.DataAccess.DTOs;

namespace BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("settings/{roomId}")]
public IActionResult SaveLaundryRoomSettings(int roomId, [FromBody] ComplexSettingsDto settingsDto)
{
    Console.WriteLine($"[INFO] SaveLaundryRoomSettings called for Room ID: {roomId}");

    // Log the incoming settings data
    Console.WriteLine($"[INFO] Received settings DTO: {System.Text.Json.JsonSerializer.Serialize(settingsDto, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");
    Console.WriteLine($"SaveLaundryRoomSettings called for Room ID: {roomId}");
    Console.WriteLine($"Received settings DTO: {System.Text.Json.JsonSerializer.Serialize(settingsDto)}");


    /*
    if (!User.Identity.IsAuthenticated)
    {
        Console.WriteLine($"[ERROR] Unauthorized attempt to save settings for Room ID: {roomId}");
        return Unauthorized(new { message = "Unauthorized save" });
    }
    */

    try
    {
        
        
        
        
        // Get the laundry room
        Console.WriteLine($"[INFO] Fetching LaundryRoom with ID: {roomId}");
        var laundryRoom = _context.LaundryRooms
            .Include(lr => lr.ApartmentComplex)
            .FirstOrDefault(lr => lr.LaundryRoomId == roomId);

        if (laundryRoom == null)
        {
            Console.WriteLine($"[ERROR] LaundryRoom with ID: {roomId} not found.");
            return NotFound(new { message = "Laundry room not found." });
        }

        Console.WriteLine($"[INFO] LaundryRoom found: {laundryRoom.RoomName}");

        // Get the associated complex
        var complex = laundryRoom.ApartmentComplex;
        if (complex == null)
        {
            Console.WriteLine($"[ERROR] Associated ApartmentComplex for LaundryRoom ID: {roomId} not found.");
            return NotFound(new { message = "Associated complex not found." });
        }

        Console.WriteLine($"[INFO] Associated ApartmentComplex found: {complex.ComplexName}");

        // Save or update settings
        Console.WriteLine($"[INFO] Checking existing settings for Complex ID: {complex.Id}");
        var existingSettings = _context.ComplexSettings
            .FirstOrDefault(cs => cs.ComplexId == complex.Id);

        if (existingSettings == null)
        {
            Console.WriteLine($"[INFO] No existing settings found for Complex ID: {complex.Id}. Creating new settings.");
            var newSettings = new ComplexSettings
            {
                ComplexId = complex.Id,
                MaxBookingsPerUser = settingsDto.MaxBookingsPerUser,
                AllowShowUserInfo = settingsDto.AllowShowUserInfo ? 1 : 0
            };
            _context.ComplexSettings.Add(newSettings);
        }
        else
        {
            Console.WriteLine($"[INFO] Existing settings found for Complex ID: {complex.Id}. Updating settings.");
            existingSettings.MaxBookingsPerUser = settingsDto.MaxBookingsPerUser;
            existingSettings.AllowShowUserInfo = settingsDto.AllowShowUserInfo ? 1 : 0;
        }

        // Handle time slots
        Console.WriteLine($"[INFO] Saving time slots for Complex ID: {complex.Id}");
        SaveTimeSlots(complex.Id, settingsDto.TimeSlots);

        // Handle laundry machines
        Console.WriteLine($"[INFO] Saving laundry machines for Room ID: {roomId}");
        SaveLaundryMachines(roomId, settingsDto.LaundryMachines);

        // Save all changes
        Console.WriteLine($"[INFO] Saving changes to the database...");
        _context.SaveChanges();

        Console.WriteLine($"[SUCCESS] Settings saved successfully for Room ID: {roomId}");
        return Ok(new { message = "Settings saved successfully." });
    }
    catch (Exception e)
    {
        Console.WriteLine($"[ERROR] Exception occurred while saving settings for Room ID: {roomId}");
        Console.WriteLine($"[ERROR] Exception Message: {e.Message}");
        Console.WriteLine($"[ERROR] Stack Trace: {e.StackTrace}");
        return StatusCode(500, new { message = "Failed to save settings.", error = e.Message });
    }
}


       [HttpGet("settings/{roomId}")]
public IActionResult GetLaundryRoomSettings(int roomId)
{
    try
    {
        // Step 1: Fetch the laundry room
        var laundryRoom = _context.LaundryRooms
            .Include(lr => lr.ApartmentComplex)
            .FirstOrDefault(lr => lr.LaundryRoomId == roomId);

        if (laundryRoom == null)
        {
            return NotFound(new { message = "Laundry room not found." });
        }

        // Step 2: Fetch the settings for the complex
        var settings = _context.ComplexSettings
            .FirstOrDefault(cs => cs.ComplexId == laundryRoom.ComplexId);

        if (settings == null)
        {
            return NotFound(new { message = "Settings not found for this laundry room." });
        }

        // Step 3: Fetch timeslots and laundry machines and shape them
        var timeSlots = _context.Timeslots
            .Where(ts => ts.ComplexId == laundryRoom.ComplexId)
            .Select(ts => new
            {
                timeslotId = ts.TimeslotId,
                startTime = ts.StartTime,
                endTime = ts.EndTime,
                complexId = ts.ComplexId,
                complexName = _context.ApartmentComplexes
                    .Where(ac => ac.Id == ts.ComplexId)
                    .Select(ac => ac.ComplexName)
                    .FirstOrDefault()
            })
            .ToList();

        var laundryMachines = _context.LaundryMachines
            .Where(lm => lm.LaundryRoomId == roomId)
            .Select(lm => new
            {
                machineId = lm.MachineId,
                machineName = lm.MachineName,
                machineType = lm.MachineType,
                status = lm.Status,
                laundryRoomId = lm.LaundryRoomId,
                laundryRoomName = laundryRoom.RoomName
            })
            .ToList();

        // Step 4: Construct a clean response
        var settingsDto = new
        {
            laundryRoomId = laundryRoom.LaundryRoomId,
            complexId = laundryRoom.ComplexId,
            maxBookingsPerUser = settings.MaxBookingsPerUser,
            allowShowUserInfo = settings.AllowShowUserInfo == 1,
            timeSlots,
            laundryMachines
        };

        var options = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles // Avoid $id and $values
        };

        return new JsonResult(settingsDto, options);
    }
    catch (Exception e)
    {
        // Log the error and return a server error response
        Console.WriteLine($"Error fetching settings for laundry room {roomId}: {e.Message}");
        return StatusCode(500, new { message = "Failed to retrieve settings.", error = e.Message });
    }
}




        // Helper methods to save TimeSlots and LaundryMachines

        private void SaveTimeSlots(int complexId, List<TimeslotDto> timeSlots)
        {
            // Remove existing time slots for the complex
            var existingTimeSlots = _context.Timeslots.Where(ts => ts.ComplexId == complexId).ToList();
            _context.Timeslots.RemoveRange(existingTimeSlots);

            // Add new/updated time slots
            foreach (var slot in timeSlots)
            {
                var newTimeslot = new Timeslot
                {
                    TimeslotId = slot.TimeslotId > 0 ? slot.TimeslotId : 0, // Handle new slots with ID = 0
                    ComplexId = complexId,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime
                };

                _context.Timeslots.Add(newTimeslot);
            }
        }


        private void SaveLaundryMachines(int roomId, List<LaundryMachineDto> machines)
        {
            // Remove existing machines
            var existingMachines = _context.LaundryMachines
                .Where(lm => lm.LaundryRoomId == roomId)
                .ToList();
            _context.LaundryMachines.RemoveRange(existingMachines);

            // Add new machines
            foreach (var machine in machines)
            {
                var newMachine = new LaundryMachine
                {
                    LaundryRoomId = roomId,
                    MachineName = machine.MachineName,
                    MachineType = machine.MachineType == "Vaskemaskine" ? "Washer" : machine.MachineType == "Tørretumbler" ? "Dryer" : machine.MachineType,
                    Status = "Available" // Assuming newly added machines are available by default
                };
                _context.LaundryMachines.Add(newMachine);
            }
        }

        private List<TimeslotDto> GetTimeSlotsWithIds(int complexId)
        {
            // This ensures we are using IQueryable and eliminates the ambiguity
            var timeslots = _context.Timeslots
                .Where(ts => ts.ComplexId == complexId)
                .Select(ts => new TimeslotDto
                {
                    TimeslotId = ts.TimeslotId,
                    StartTime = ts.StartTime, // Assign directly as TimeSpan
                    EndTime = ts.EndTime, // Assign directly as TimeSpan
                    ComplexId = ts.ComplexId,
                    ComplexName = ts.ApartmentComplex.ComplexName // Assuming you have navigation property configured for ApartmentComplex
                })
                .ToList();

            return timeslots;
        }


        private List<LaundryMachineDto> GetLaundryMachinesWithIds(int roomId)
        {
            var machines = _context.LaundryMachines
                .Where(lm => lm.LaundryRoomId == roomId)
                .Select(lm => new LaundryMachineDto
                {
                    MachineId = lm.MachineId,
                    MachineName = lm.MachineName,
                    MachineType = lm.MachineType
                })
                .ToList();

            return machines;
        }
        
        /*
        [HttpPost("timeslot")]
        public IActionResult AddTimeSlot([FromBody] TimeslotDto slotDto)
        {
            if (slotDto == null)
            {
                Console.WriteLine("Slot is null");
                return BadRequest(new { message = "Time slot data is required." });
            }

            Console.WriteLine($"Received Timeslot: StartTime={slotDto.StartTime}, EndTime={slotDto.EndTime}, ComplexId={slotDto.ComplexId}");

            try
            {
                var newSlot = new Timeslot
                {
                    StartTime = slotDto.StartTime,
                    EndTime = slotDto.EndTime,
                    ComplexId = slotDto.ComplexId,
                };

                _context.Timeslots.Add(newSlot);
                _context.SaveChanges();

                var createdSlot = new TimeslotDto
                {
                    TimeslotId = newSlot.TimeslotId,
                    StartTime = newSlot.StartTime,
                    EndTime = newSlot.EndTime,
                    ComplexId = newSlot.ComplexId
                };

                return Ok(createdSlot);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error adding time slot: {e.Message}");
                return StatusCode(500, new { message = "Failed to add time slot.", error = e.Message });
            }
        }

            
        
        [HttpPut("timeslot/{id}")]
        public IActionResult UpdateTimeSlot(int id, [FromBody] TimeslotDto slotDto)
        {
            try
            {
                if (slotDto == null || id <= 0)
                {
                    return BadRequest(new { message = "Invalid time slot data or ID." });
                }

                // Find the existing time slot
                var existingSlot = _context.Timeslots.FirstOrDefault(ts => ts.TimeslotId == id);
                if (existingSlot == null)
                {
                    return NotFound(new { message = "Time slot not found." });
                }

                // Update fields
                existingSlot.StartTime = slotDto.StartTime;
                existingSlot.EndTime = slotDto.EndTime;

                // Save changes
                _context.SaveChanges();

                return Ok(new { message = "Time slot updated successfully." });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error updating time slot: {e.Message}");
                return StatusCode(500, new { message = "Failed to update time slot.", error = e.Message });
            }
        }

        
        [HttpDelete("timeslot/{id}")]
        public IActionResult DeleteTimeSlot(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid time slot ID." });
                }

                // Find the time slot
                var slot = _context.Timeslots.FirstOrDefault(ts => ts.TimeslotId == id);
                if (slot == null)
                {
                    return NotFound(new { message = "Time slot not found." });
                }

                // Remove the slot
                _context.Timeslots.Remove(slot);
                _context.SaveChanges();

                return Ok(new { message = "Time slot deleted successfully." });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error deleting time slot: {e.Message}");
                return StatusCode(500, new { message = "Failed to delete time slot.", error = e.Message });
            }
        }
*/
        
    }
}
