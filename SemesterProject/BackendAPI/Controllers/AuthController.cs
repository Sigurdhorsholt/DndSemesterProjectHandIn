using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendAPI.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterModel model)
    {
        try
        {
            // Check if user already exists
            if (_context.Users.Any(u => u.UserName == model.UserName))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Create a new user object
            var newUser = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,  // Note: Ideally, you should hash the password.
                UserType = model.UserType
            };

            // Add and save the new user
            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { message = "User registered successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        try
        {
            // Check if user exists in the database
            var user = GetUserWithComplexInfo(model.UserName);
            if (user == null)
            {
                return Unauthorized(new { message = "User does not exist" });
            }
            // Check password
            if (user.Password != model.Password) // Again, you should be using a hashed password comparison.
            {
                return Unauthorized(new { message = "Incorrect password" });
            }
            // Generate JWT Token with additional claims
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Login error: " + ex.Message);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    
    
    private User GetUserWithComplexInfo(string userName)
    {
        return _context.Users
            .Include(u => u.LivesIn)
            .ThenInclude(l => l.ApartmentComplex)
            .FirstOrDefault(u => u.UserName == userName);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("ThisIsASecretKeyWithAtLeast32Characters");

        // Create a claims list and add default claims
        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Username", user.UserName ?? ""),
            new Claim("Email", user.Email ?? ""),
            new Claim("FullName", (user.FullName ?? "").Replace("\n", "").Replace("\r", "")),
            new Claim("Apartment", (user.Apartment ?? "").Replace("\n", "").Replace("\r", "")),
            new Claim("IsAdmin", user.IsAdmin ? "true" : "false")
        };

        // Add LivesIn-related claims
        foreach (var livesIn in user.LivesIn)
        {
            if (livesIn.ApartmentComplex != null)
            {
                claims.Add(new Claim("ComplexName", (livesIn.ApartmentComplex.ComplexName ?? "").Replace("\n", "").Replace("\r", "")));
                claims.Add(new Claim("Street", (livesIn.ApartmentComplex.Street ?? "").Replace("\n", "").Replace("\r", "")));
                claims.Add(new Claim("City", (livesIn.ApartmentComplex.City ?? "").Replace("\n", "").Replace("\r", "")));
                claims.Add(new Claim("ZipCode", livesIn.ApartmentComplex.Zipcode.ToString()));
            }
        }

        // Create the token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = _configuration["Issuer"],
            Audience = _configuration["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        // Generate the token
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public class RegisterModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; } // Enum: SystemAdmin, ComplexAdmin, DailyUser
    }

    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    
    
    



}
