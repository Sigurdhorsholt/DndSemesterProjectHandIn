namespace BackendAPI.DataAccess.DTOs;

public class DTOs
{
    
}

public class LaundryRoomDto
{
    public int LaundryRoomId { get; set; }
    public string RoomName { get; set; }
    public int ComplexId { get; set; }
    public string ComplexName { get; set; }
}

public class BookingDto
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int MachineId { get; set; }
    public DateTime BookingDate { get; set; }
    public string Apartment { get; set; }
    public string FullName { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public int LaundryRoomId { get; set; }
    public int TimeSlotId { get; set; }
}
    
public class BookingRequestDto
{
    public int UserId { get; set; }
    public int MachineId { get; set; }
    public DateTime BookingDate { get; set; }
    public int TimeslotId { get; set; }
    public int LaundryRoomId { get; set; }
} 
    
public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string UserType { get; set; } // SystemAdmin, ComplexAdmin, DailyUser
    public string Apartment { get; set; }
    public bool IsAdmin { get; set; }
}
public class ApartmentComplexDto
{
    public int Id { get; set; }
    public string ComplexName { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public int Zipcode { get; set; }
}

public class TimeslotDto
{
    public int TimeslotId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int ComplexId { get; set; }
    public string ComplexName { get; set; }
}
public class LaundryMachineDto
{
    public int MachineId { get; set; }
    public string MachineName { get; set; }
    public string MachineType { get; set; } // Washer or Dryer
    public string Status { get; set; } // Available or InUse
    public int LaundryRoomId { get; set; }
    public string LaundryRoomName { get; set; }
}

public class ComplexSettingsDto
{
    public int LaundryRoomId { get; set; } // Identifier for the laundry room the settings are associated with
    public int ComplexId { get; set; } // Identifier for the complex to which the laundry room belongs
    public int MaxBookingsPerUser { get; set; } // Maximum bookings allowed per user
    public bool AllowShowUserInfo { get; set; } // Whether to allow showing user information (e.g., names)
    public List<TimeslotDto> TimeSlots { get; set; } = new List<TimeslotDto>(); // List of available time slots
    public List<LaundryMachineDto> LaundryMachines { get; set; } = new List<LaundryMachineDto>(); // List of laundry machines in the laundry room
}


public class UserRegistrationDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Password { get; set; }
    public string UserType { get; set; }
    public string Apartment { get; set; }
    public bool IsAdmin { get; set; }
    public int ComplexId { get; set; }
}