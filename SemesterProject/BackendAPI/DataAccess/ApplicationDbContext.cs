using Microsoft.EntityFrameworkCore;

namespace BackendAPI.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Define DbSets for each table you need to interact with
        public DbSet<User> Users { get; set; }
        public DbSet<ApartmentComplex> ApartmentComplexes { get; set; }
        public DbSet<LivesIn> LivesIn { get; set; }
        public DbSet<Booking> Bookings { get; set; } // Added Bookings DbSet
        public DbSet<Timeslot> Timeslots { get; set; }
        public DbSet<LaundryRoom> LaundryRooms { get; set; }
        public DbSet<LaundryMachine> LaundryMachines { get; set; }
        public DbSet<AdminManages> AdminManages { get; set; }
        public DbSet<ComplexSettings> ComplexSettings { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Mapping entities to corresponding tables
    modelBuilder.Entity<User>().ToTable("User");
    modelBuilder.Entity<ApartmentComplex>().ToTable("ApartmentComplexes");
    modelBuilder.Entity<LivesIn>().ToTable("LivesIn");
    modelBuilder.Entity<Booking>().ToTable("Booking");
    modelBuilder.Entity<Timeslot>().ToTable("Timeslots");
    modelBuilder.Entity<LaundryRoom>().ToTable("LaundryRoom");
    modelBuilder.Entity<LaundryMachine>().ToTable("LaundryMachine");
    modelBuilder.Entity<ComplexSettings>().ToTable("ComplexSettings");
    modelBuilder.Entity<ComplexTimeslotConfig>().ToTable("Complex_Timeslot_Config");
    modelBuilder.Entity<AdminManages>().ToTable("AdminManages");

    // Define primary key for LaundryMachine
    modelBuilder.Entity<LaundryMachine>()
        .HasKey(lm => lm.MachineId);


    // Define composite primary key for LivesIn
    modelBuilder.Entity<LivesIn>()
        .HasKey(li => new { li.UserId, li.ComplexId });

    // Define relationships for LivesIn
    modelBuilder.Entity<LivesIn>()
        .HasOne(li => li.User)
        .WithMany(u => u.LivesIn)
        .HasForeignKey(li => li.UserId);

    modelBuilder.Entity<LivesIn>()
        .HasOne(li => li.ApartmentComplex)
        .WithMany(ac => ac.LivesIn)
        .HasForeignKey(li => li.ComplexId);

    // Define composite primary key for AdminManages
    modelBuilder.Entity<AdminManages>()
        .HasKey(am => new { am.UserId, am.ComplexId });

    // Define relationships for AdminManages
    modelBuilder.Entity<AdminManages>()
        .HasOne(am => am.User)
        .WithMany(u => u.AdminManages)
        .HasForeignKey(am => am.UserId);

    modelBuilder.Entity<AdminManages>()
        .HasOne(am => am.ApartmentComplex)
        .WithMany(ac => ac.AdminManages)
        .HasForeignKey(am => am.ComplexId);
    
    // Define primary key for ComplexSettings
    modelBuilder.Entity<ComplexSettings>()
        .HasKey(cs => cs.ConfigId);

    // Configure one-to-one relationship between ComplexSettings and ApartmentComplex
    modelBuilder.Entity<ComplexSettings>()
        .HasOne(cs => cs.ApartmentComplex)
        .WithOne(ac => ac.ComplexSettings)
        .HasForeignKey<ComplexSettings>(cs => cs.ComplexId)
        .OnDelete(DeleteBehavior.Cascade);

    // Define primary key for ComplexTimeslotConfig
    modelBuilder.Entity<ComplexTimeslotConfig>()
        .HasKey(ctc => ctc.ConfigId);

    modelBuilder.Entity<ComplexTimeslotConfig>()
        .HasOne(ctc => ctc.ApartmentComplex)
        .WithMany(ac => ac.ComplexTimeslotConfigs)
        .HasForeignKey(ctc => ctc.ComplexId)
        .OnDelete(DeleteBehavior.Cascade);

    // Define primary key for Booking
    modelBuilder.Entity<Booking>()
        .HasKey(b => b.BookingId);

    // Configure relationships for Booking
    modelBuilder.Entity<Booking>()
        .HasOne(b => b.User)
        .WithMany(u => u.Bookings)
        .HasForeignKey(b => b.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Booking>()
        .HasOne(b => b.LaundryMachine)
        .WithMany(lm => lm.Bookings)
        .HasForeignKey(b => b.MachineId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Booking>()
        .HasOne(b => b.Timeslot)
        .WithMany(ts => ts.Bookings)
        .HasForeignKey(b => b.TimeslotId);

    modelBuilder.Entity<Booking>()
        .HasOne(b => b.LaundryRoom)
        .WithMany(lr => lr.Bookings)
        .HasForeignKey(b => b.LaundryRoomId)
        .OnDelete(DeleteBehavior.Cascade);
    
    modelBuilder.Entity<Timeslot>()
        .ToTable("Timeslots")
        .HasKey(t => t.TimeslotId);

    modelBuilder.Entity<Timeslot>()
        .Property(t => t.ComplexId)
        .HasColumnName("ComplexId"); // Ensure this matches the actual column name in the database.

    modelBuilder.Entity<Timeslot>()
        .HasOne(t => t.ApartmentComplex)
        .WithMany(ac => ac.Timeslots)
        .HasForeignKey(t => t.ComplexId)
        .OnDelete(DeleteBehavior.Cascade);

    
    // Mapping entities to corresponding tables
    modelBuilder.Entity<LaundryRoom>().ToTable("LaundryRoom");

    // Define primary key and relationships for LaundryRoom
    modelBuilder.Entity<LaundryRoom>()
        .HasKey(lr => lr.LaundryRoomId);

    modelBuilder.Entity<LaundryRoom>()
        .HasOne(lr => lr.ApartmentComplex)
        .WithMany(ac => ac.LaundryRooms)
        .HasForeignKey(lr => lr.ComplexId)
        .OnDelete(DeleteBehavior.Cascade);
    
    //Console.WriteLine(modelBuilder.Model.ToDebugString());

}




    }

}