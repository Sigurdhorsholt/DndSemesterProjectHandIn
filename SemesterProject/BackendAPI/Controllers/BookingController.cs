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
                var bookings = _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Timeslot)
                    .Where(b => b.LaundryRoomId == laundryRoomId)
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
                // Step 1: Fetch data from the database
                var bookingsData = _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Timeslot)
                    .Where(b => b.MachineId == machineId)
                    .ToList();

                // Step 2: Map to DTOs, handling nulls where necessary
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

                return Ok("Booking successfully created.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "Internal server error");
            }
        }
    }
    
    


}
