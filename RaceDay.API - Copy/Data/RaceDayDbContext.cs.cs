using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        // DbSets — one per entity
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<WeatherLog> WeatherLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. User — unique email + role check constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .ToTable(t => t.HasCheckConstraint("CK_User_Role",
                    "[Role] IN ('Organiser', 'Participant')"));

            // 2. Event → Organiser (many events to one user)
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Category → Event (many categories to one event)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Enrolment → User (many enrolments to one user)
            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.User)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(en => en.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Enrolment → Category
            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(en => en.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Unique constraint: a user cannot enrol twice in the same category
            modelBuilder.Entity<Enrolment>()
                .HasIndex(en => new { en.UserId, en.CategoryId })
                .IsUnique();

            // 7. Result → Enrolment (one-to-one)
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // 8. Result — unique constraint on EnrolmentId
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentId)
                .IsUnique();

            // 9. WeatherLog → Event
            modelBuilder.Entity<WeatherLog>()
                .HasOne(w => w.Event)
                .WithMany(e => e.WeatherLogs)
                .HasForeignKey(w => w.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}