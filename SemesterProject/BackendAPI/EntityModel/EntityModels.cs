using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackendAPI.DataAccess
{
    public class ApartmentComplex
    {
        public int Id { get; set; }
        public string ComplexName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public int Zipcode { get; set; }

        // Navigation Properties
        public ComplexSettings ComplexSettings { get; set; }
        public ICollection<ComplexTimeslotConfig> ComplexTimeslotConfigs { get; set; } = new List<ComplexTimeslotConfig>();
        public ICollection<LaundryRoom> LaundryRooms { get; set; } = new List<LaundryRoom>();
        public ICollection<AdminManages> AdminManages { get; set; } = new List<AdminManages>();
        public ICollection<LivesIn> LivesIn { get; set; } = new List<LivesIn>();
        public ICollection<Timeslot> Timeslots { get; set; } = new List<Timeslot>();
    }

    public class ComplexSettings
    {
        public int ConfigId { get; set; } // Primary Key

        public int ComplexId { get; set; } // ComplexId is non-nullable
        public ApartmentComplex ApartmentComplex { get; set; }

        public int MaxBookingsPerUser { get; set; } = 2;
        public int AllowShowUserInfo { get; set; } = 0;
    }

    public class ComplexTimeslotConfig
    {
        public int ConfigId { get; set; }
        public int ComplexId { get; set; }
        public ApartmentComplex ApartmentComplex { get; set; }
        public TimeSpan EarliestStartTime { get; set; }
        public int TimeslotDurationHour { get; set; }
        public int SlotsPerDay { get; set; }
    }

    public class LaundryRoom
    {
        public int LaundryRoomId { get; set; }
        public string RoomName { get; set; }
        public int ComplexId { get; set; }  // Corrected to match DB schema
        public ApartmentComplex ApartmentComplex { get; set; }

        // Navigation Properties
        public ICollection<LaundryMachine> LaundryMachines { get; set; } = new List<LaundryMachine>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }


    public class LaundryMachine
    {
        [Key]
        public int MachineId { get; set; } // Primary Key
        public string MachineName { get; set; } // Non-nullable
        public string MachineType { get; set; } = "Washer"; // Non-nullable
        public string Status { get; set; } = "Available"; // Non-nullable
        public int LaundryRoomId { get; set; } // Non-nullable
        public LaundryRoom LaundryRoom { get; set; } // Navigation property

        // Navigation Properties
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }


    public class Timeslot
    {
        public int TimeslotId { get; set; }
        public int ComplexId { get; set; }  // This should match the database column name.
        public ApartmentComplex ApartmentComplex { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Navigation Properties
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }


    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
        public string? Apartment { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsAdmin { get; set; }

        // Navigation Properties
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<AdminManages> AdminManages { get; set; } = new List<AdminManages>();
        [JsonIgnore] // Prevents serialization loop

        public ICollection<LivesIn> LivesIn { get; set; } = new List<LivesIn>();
    }

    public class AdminManages
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int ComplexId { get; set; }
        public ApartmentComplex ApartmentComplex { get; set; }
    }

    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int MachineId { get; set; }
        public LaundryMachine LaundryMachine { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime BookOnDate { get; set; }
        public int? TimeslotId { get; set; }
        public Timeslot? Timeslot { get; set; }
        public int LaundryRoomId { get; set; }
        public LaundryRoom LaundryRoom { get; set; }
    }

    public class LivesIn
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int ComplexId { get; set; }
        public ApartmentComplex ApartmentComplex { get; set; }
    }
    
    
}
