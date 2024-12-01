using BackendAPI.DataAccess;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using BackendAPI.DataAccess.DTOs;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Inject ApplicationDbContext via constructor
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

// Get all users for a specific laundry room (based on ComplexId)
        [HttpGet("laundryroom/{laundryRoomId}/users")]
        public IActionResult GetUsersByLaundryRoom(int laundryRoomId)
        {
            try
            {
                // Find the complex ID associated with the given laundry room
                var laundryRoom = _context.LaundryRooms.FirstOrDefault(lr => lr.LaundryRoomId == laundryRoomId);
                if (laundryRoom == null)
                {
                    return NotFound(new { message = "Laundry room not found." });
                }

                // Fetch users associated with the complex
                var users = _context.LivesIn
                    .Where(li => li.ComplexId == laundryRoom.ComplexId)
                    .Select(li => li.User)
                    .ToList();

                
                
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles // Avoid $id and $values
                };

                return new JsonResult(new { users = users }, options);
           
           //     return Ok(users); // Return the list of users
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // Get user by id
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                // Handle the error (you can return an appropriate HTTP error code and message)
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // Add a new user
          [HttpPost]
    public IActionResult RegisterUser([FromBody] UserRegistrationDto userRegistrationDto)
    {
        if (userRegistrationDto == null || !ModelState.IsValid)
        {
            return BadRequest("Invalid user data.");
        }

        try
        {
            // Step 1: Check if the username or email already exists
            var existingUser = _context.Users
                .FirstOrDefault(u => u.UserName == userRegistrationDto.UserName || u.Email == userRegistrationDto.Email);

            if (existingUser != null)
            {
                if (existingUser.UserName == userRegistrationDto.UserName)
                {
                    return Conflict(new { message = "Username already exists." });
                }
                if (existingUser.Email == userRegistrationDto.Email)
                {
                    return Conflict(new { message = "Email already exists." });
                }
            }

            // Step 2: Map UserRegistrationDto to User entity
            var newUser = new User
            {
                UserName = userRegistrationDto.UserName,
                Email = userRegistrationDto.Email,
                FullName = userRegistrationDto.FullName,
                Password = userRegistrationDto.Password, // Store securely (consider hashing in a real application)
                UserType = userRegistrationDto.UserType,
                Apartment = userRegistrationDto.Apartment,
                IsAdmin = userRegistrationDto.IsAdmin,
                LastLogin = DateTime.UtcNow // Set LastLogin to current time by default
            };

            // Step 3: Add the new user to the context
            _context.Users.Add(newUser);
            _context.SaveChanges(); // Save to get UserId generated
            
            
            //retrieve user after creation:
            var retrievedUser = _context.Users.FirstOrDefault(u => u.Id == newUser.Id);
            
            

            // Step 4: Add LivesIn relationship if a valid ComplexId is provided
            if (userRegistrationDto.ComplexId > 0)
            {
                var newLivesIn = new LivesIn
                {
                    UserId = retrievedUser.Id,
                    ComplexId = userRegistrationDto.ComplexId
                };
                _context.LivesIn.Add(newLivesIn);
                _context.SaveChanges();
            }

            // Step 5: Return the created response using UserDto
            var userDto = retrievedUser;
           

            return CreatedAtAction(nameof(GetUserById), new { id = userDto.Id }, userDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }
    

        // Update an existing user
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (updatedUser == null || id != updatedUser.Id || !ModelState.IsValid)
            {
                return BadRequest("Invalid user data.");
            }

            try
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Id == id);
                if (existingUser == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Update user properties
                existingUser.UserName = updatedUser.UserName;
                existingUser.Email = updatedUser.Email;
                existingUser.FullName = updatedUser.FullName;
                existingUser.UserType = updatedUser.UserType;
                existingUser.Apartment = updatedUser.Apartment;
                existingUser.LastLogin = updatedUser.LastLogin;
                existingUser.IsAdmin = updatedUser.IsAdmin;

                _context.SaveChanges();

                return Ok(new { message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // Delete a user
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                _context.Users.Remove(user);
                _context.SaveChanges();

                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
