using BackendAPI.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using BackendAPI.DataAccess.DTOs;

namespace BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        
        
                // Constructor to inject the database context and configuration

        public BookingController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. Get all bookings for a specific user
        [HttpGet("user/{userId}")]
        public IActionResult GetBookingsForUser(int userId)
        {
            try
            {
                                // Fetch bookings for the specified user, including user and timeslot details

                var bookings = _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Timeslot)
                    .Where(b => b.UserId == userId)
                    .Select(b => new BookingDto
                    {
                        BookingId = b.BookingId,
                        UserId = b.UserId,
                        MachineId = b.MachineId,
                        BookingDate = b.BookingDate,
                        Apartment = b.User.Apartment,
                        FullName = b.User.FullName,
                        StartTime = b.Timeslot.StartTime.ToString(@"hh\:mm"),
                        EndTime = b.Timeslot.EndTime.ToString(@"hh\:mm"),
                        LaundryRoomId = b.LaundryRoomId
                    }).ToList();

                return Ok(bookings);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "Internal server error");
            }
        }

        // 2. Get all bookings for a specific laundry room
        [HttpGet("laundryroom/{laundryRoomId}")]
        public IActionResult GetAllBookingsForLaundryRoom(int laundryRoomId)
        {
            try
            {
                // Fetch bookings for the specified laundry room
                var bookings = _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Timeslot)
                    .Where(b => b.LaundryRoomId == laundryRoomId)
                    .Select(b => new
                    {
                        bookingId = b.BookingId,
                        userId = b.UserId,
                        machineId = b.MachineId,
                        bookingDate = b.BookingDate,
                        apartment = b.User.Apartment,
                        fullName = b.User.FullName,
                        startTime = b.Timeslot.StartTime.ToString(@"hh\:mm"),
                        endTime = b.Timeslot.EndTime.ToString(@"hh\:mm"),
                        TimeSlotId = b.Timeslot.TimeslotId,
                        laundryRoomId = b.LaundryRoomId
                    })
                    .ToList();
                
                
                                // Serialize the response to avoid reference cycles
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles // Avoid $id and $values
                };
                
                

                // Return the explicitly shaped response
                return new JsonResult(bookings, options);
            }
            catch (Exception e)
            {
                // Log the exception and return an error response
                Console.WriteLine($"Error fetching bookings for laundry room {laundryRoomId}: {e.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        // 3. Get a specific booking by booking ID
        [HttpGet("{bookingId}")]
        public IActionResult GetBookingById(int bookingId)
        {
            try
            {
                var booking = _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Timeslot)
                    .Where(b => b.BookingId == bookingId)
                    .Select(b => new BookingDto
                    {
                        BookingId = b.BookingId,
                        UserId = b.UserId,
                        MachineId = b.MachineId,
                        BookingDate = b.BookingDate,
                        Apartment = b.User.Apartment,
                        FullName = b.User.FullName,
                        StartTime = b.Timeslot.StartTime.ToString(@"hh\:mm"),
                        EndTime = b.Timeslot.EndTime.ToString(@"hh\:mm"),
                        LaundryRoomId = b.LaundryRoomId
                    })
                    .FirstOrDefault();

                if (booking == null)
                {
                    return NotFound();
                }

                return Ok(booking);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "Internal server error");
            }
        }

        // 4. Get all bookings for a specific machine
        [HttpGet("machine/{machineId}")]
        public IActionResult GetBookingsForMachine(int machineId)
        {
            try
            {
                // Fetch bookings for the specified machine
                var bookingsData = _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Timeslot)
                    .Where(b => b.MachineId == machineId)
                    .ToList();

                // Map bookings to DTOs
                var bookings = bookingsData.Select(b => new BookingDto
                {
                    BookingId = b.BookingId,
                    UserId = b.UserId,
                    MachineId = b.MachineId,
                    BookingDate = b.BookingDate,
                    Apartment = b.User != null ? b.User.Apartment ?? "N/A" : "N/A",
                    FullName = b.User != null ? b.User.FullName ?? "N/A" : "N/A",
                    StartTime = b.Timeslot != null ? b.Timeslot.StartTime.ToString(@"hh\:mm") : "N/A",
                    EndTime = b.Timeslot != null ? b.Timeslot.EndTime.ToString(@"hh\:mm") : "N/A",
                    LaundryRoomId = b.LaundryRoomId
                }).ToList();

                return Ok(bookings);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error fetching bookings for machine {machineId}: {e.Message}");
                return StatusCode(500, "Internal server error");
            }
        }




        // 5. Get all upcoming bookings (from today onward)
        [HttpGet("upcoming/{laundryRoomId}")]
        public IActionResult GetUpcomingBookings(int laundryRoomId)
        {
            try
            {
                var today = DateTime.Today;
                var bookings = _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Timeslot)
                    .Where(b => b.LaundryRoomId == laundryRoomId && b.BookingDate >= today)
                    .Select(b => new BookingDto
                    {
                        BookingId = b.BookingId,
                        UserId = b.UserId,
                        MachineId = b.MachineId,
                        BookingDate = b.BookingDate,
                        Apartment = b.User.Apartment,
                        FullName = b.User.FullName,
                        StartTime = b.Timeslot.StartTime.ToString(@"hh\:mm"),
                        EndTime = b.Timeslot.EndTime.ToString(@"hh\:mm"),
                        LaundryRoomId = b.LaundryRoomId
                    }).ToList();

                return Ok(bookings);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "Internal server error");
            }
        }

        // 6. Get all bookings for a specific room and date
        [HttpGet("laundryroom/{laundryRoomId}/date/{bookingDate}")]
        public IActionResult GetBookingsForRoomAndDate(int laundryRoomId, DateTime bookingDate)
        {
            try
            {
                var bookings = _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Timeslot)
                    .Where(b => b.LaundryRoomId == laundryRoomId && b.BookingDate.Date == bookingDate.Date)
                    .Select(b => new BookingDto
                    {
                        BookingId = b.BookingId,
                        UserId = b.UserId,
                        MachineId = b.MachineId,
                        BookingDate = b.BookingDate,
                        Apartment = b.User.Apartment,
                        FullName = b.User.FullName,
                        StartTime = b.Timeslot.StartTime.ToString(@"hh\:mm"),
                        EndTime = b.Timeslot.EndTime.ToString(@"hh\:mm"),
                        LaundryRoomId = b.LaundryRoomId
                    }).ToList();


                return Ok(bookings);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("booking")]
        public IActionResult CreateBooking([FromBody] BookingRequestDto bookingRequest)
        {
            try
            {
                // Check if the timeslot is already booked
                var isBooked = _context.Bookings.Any(b =>
                    b.MachineId == bookingRequest.MachineId &&
                    b.BookingDate == bookingRequest.BookingDate &&
                    b.TimeslotId == bookingRequest.TimeslotId);

                if (isBooked)
                {
                    return Conflict("Timeslot is already booked");
                }

                var newBooking = new Booking
                {
                    UserId = bookingRequest.UserId,
                    MachineId = bookingRequest.MachineId,
                    BookingDate = bookingRequest.BookingDate,
                    TimeslotId = bookingRequest.TimeslotId,
                    LaundryRoomId = bookingRequest.LaundryRoomId,
                    BookOnDate = DateTime.Today
                };

                _context.Bookings.Add(newBooking);
                _context.SaveChanges();
                
                
                
                // Map the new booking to a DTO to return
                var createdBooking = new BookingDto
                {
                    BookingId = newBooking.BookingId,
                    UserId = newBooking.UserId,
                    MachineId = newBooking.MachineId,
                    BookingDate = newBooking.BookingDate,
                    StartTime = _context.Timeslots.FirstOrDefault(t => t.TimeslotId == newBooking.TimeslotId)?.StartTime.ToString(@"hh\:mm"),
                    EndTime = _context.Timeslots.FirstOrDefault(t => t.TimeslotId == newBooking.TimeslotId)?.EndTime.ToString(@"hh\:mm"),
                    LaundryRoomId = newBooking.LaundryRoomId,
                    Apartment = _context.Users.FirstOrDefault(u => u.Id == newBooking.UserId)?.Apartment,
                    FullName = _context.Users.FirstOrDefault(u => u.Id == newBooking.UserId)?.FullName
                };

                return Ok(createdBooking);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "Internal server error");
            }
        }
    }
    
    


}
