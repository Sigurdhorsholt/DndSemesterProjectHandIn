namespace FrontendBlazor.Model.DTOs;

public class DTOs
{
    
}

public class LaundryRoomDto
{
    public int LaundryRoomId { get; set; }
    public string RoomName { get; set; }
    public List<LaundryMachineDto> MachineDtos { get; set; } = new();
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
    public List<LivesInDto> LivesIn { get; set; } = new List<LivesInDto>();

    
}

public class LivesInDto
{
    public string ComplexName { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
}







public class ApartmentComplexDto
{
    public int Id { get; set; }
    public string ComplexName { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public int Zipcode { get; set; }
}

public class TimeSlotDto
{
    public int TimeslotId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int ComplexId { get; set; }
    public string ComplexName { get; set; }
}
public class TimeSlotsResponseDto
{
    public List<TimeSlotDto> Timeslots { get; set; }
}


public class LaundryMachineDto
{
    public int MachineId { get; set; }
    public string MachineName { get; set; }
    public string MachineType { get; set; }
    public string Status { get; set; }
    public int LaundryRoomId { get; set; }
    public string LaundryRoomName { get; set; }
}


public class ComplexSettingsDto
{
    public int LaundryRoomId { get; set; }
    public int ComplexId { get; set; } 
    public int MaxBookingsPerUser { get; set; } 
    public bool AllowShowUserInfo { get; set; } 
    public List<TimeSlotDto> TimeSlots { get; set; } = new List<TimeSlotDto>(); 
    public List<LaundryMachineDto> LaundryMachines { get; set; } = new List<LaundryMachineDto>(); 
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

public class UserLoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class AccessibleLaundryRoomsDto
{
    public List<LaundryRoomDto> Rooms { get; set; } = new();
}

public class UsersResponseDto
{
    public List<UserDto> Users { get; set; } = new();
}
