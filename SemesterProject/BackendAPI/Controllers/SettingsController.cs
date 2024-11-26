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

        // 1. Save settings for a specific laundry room
        [HttpPost("laundry-room/{roomId}/settings")]
        public IActionResult SaveLaundryRoomSettings(int roomId, [FromBody] ComplexSettingsDto settingsDto)
        {
            Console.WriteLine("Settings saved route HIT");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "Unauthorized save" });
            }

            try
            {
                // Get the complex associated with the laundry room
                var laundryRoom = _context.LaundryRooms
                    .Include(lr => lr.ApartmentComplex)
                    .FirstOrDefault(lr => lr.LaundryRoomId == roomId);

                if (laundryRoom == null)
                {
                    return NotFound(new { message = "Laundry room not found." });
                }

                var complex = laundryRoom.ApartmentComplex;

                if (complex == null)
                {
                    return NotFound(new { message = "Associated complex not found." });
                }

                // Save or update settings
                var existingSettings = _context.ComplexSettings
                    .FirstOrDefault(cs => cs.ComplexId == complex.Id);

                if (existingSettings == null)
                {
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
                    existingSettings.MaxBookingsPerUser = settingsDto.MaxBookingsPerUser;
                    existingSettings.AllowShowUserInfo = settingsDto.AllowShowUserInfo ? 1 : 0;
                }

                // Handle TimeSlots and LaundryMachines
                SaveTimeSlots(roomId, settingsDto.TimeSlots);
                SaveLaundryMachines(roomId, settingsDto.LaundryMachines);

                // Save all changes
                _context.SaveChanges();

                return Ok(new { message = "Settings saved successfully." });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, new { message = "Failed to save settings.", error = e.Message });
            }
        }

        // 2. Get settings for a specific laundry room
        [HttpGet("laundry-room/{roomId}/settings")]
        public IActionResult GetLaundryRoomSettings(int roomId)
        {
            try
            {
                var laundryRoom = _context.LaundryRooms
                    .Include(lr => lr.ApartmentComplex)
                    .FirstOrDefault(lr => lr.LaundryRoomId == roomId);

                if (laundryRoom == null)
                {
                    return NotFound(new { message = "Laundry room not found." });
                }

                var settings = _context.ComplexSettings
                    .FirstOrDefault(cs => cs.ComplexId == laundryRoom.ComplexId);

                if (settings == null)
                {
                    return NotFound(new { message = "Settings not found for this laundry room." });
                }

                var settingsDto = new ComplexSettingsDto
                {
                    LaundryRoomId = laundryRoom.LaundryRoomId,
                    ComplexId = laundryRoom.ComplexId,
                    MaxBookingsPerUser = settings.MaxBookingsPerUser,
                    AllowShowUserInfo = settings.AllowShowUserInfo == 1,
                    TimeSlots = GetTimeSlotsWithIds(laundryRoom.ComplexId),
                    LaundryMachines = GetLaundryMachinesWithIds(roomId)
                };

                return Ok(settingsDto);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, new { message = "Failed to retrieve settings.", error = e.Message });
            }
        }

        // Helper methods to save TimeSlots and LaundryMachines

        private void SaveTimeSlots(int complexId, List<TimeslotDto> timeSlots)
        {
            // Remove existing timeslots
            var existingTimeSlots = _context.Timeslots
                .Where(ts => ts.ComplexId == complexId)
                .ToList();
            _context.Timeslots.RemoveRange(existingTimeSlots);

            // Add new timeslots
            foreach (var slot in timeSlots)
            {
                var newTimeslot = new Timeslot
                {
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
    }
}
