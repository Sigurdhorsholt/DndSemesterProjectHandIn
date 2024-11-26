using BackendAPI.DataAccess;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestEFController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestEFController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("test")]
        public IActionResult TestDatabaseConnection()
        {
            try
            {
                // Simple LINQ query to get the first apartment complex
                var apartmentComplex = _context.ApartmentComplexes.FirstOrDefault();

                if (apartmentComplex != null)
                {
                    return Ok(new
                    {
                        Message = "Database connection successful",
                        ComplexName = apartmentComplex.ComplexName,
                        ComplexId = apartmentComplex.Id
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Message = "Database connection successful, but no data found"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Database connection failed",
                    Error = ex.Message
                });
            }
        }
    }
}