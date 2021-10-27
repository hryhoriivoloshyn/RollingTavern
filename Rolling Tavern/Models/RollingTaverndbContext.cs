using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Rolling_Tavern.Models
{
    public partial class RollingTaverndbContext : DbContext
    {
        public RollingTaverndbContext()
        {
        }

        public RollingTaverndbContext(DbContextOptions<RollingTaverndbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BoardGame> BoardGames { get; set; }
        public virtual DbSet<Meeting> Meetings { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<State> States { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=rolling-tavern.database.windows.net;Initial Catalog=RollingTaverndb;User ID=Kratos;Password=Godofwar21;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<BoardGame>(entity =>
            {
                entity.HasKey(e => e.GameId);

                entity.Property(e => e.GameName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Genre).HasMaxLength(50);
            });

            modelBuilder.Entity<Meeting>(entity =>
            {
                entity.Property(e => e.AddresOfMeeting)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DateOfMeeting).HasColumnType("datetime");

                entity.Property(e => e.MeetingName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PhotoLink).HasMaxLength(50);

                entity.Property(e => e.SponsorId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Meetings)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("FK_Meetings_BoardGames");
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.Meeting)
                    .WithMany()
                    .HasForeignKey(d => d.MeetingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Requests_Meetings");

                entity.HasOne(d => d.State)
                    .WithMany()
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Requests_States");
            });

            modelBuilder.Entity<State>(entity =>
            {
                entity.Property(e => e.StateId).ValueGeneratedNever();

                entity.Property(e => e.StateImage)
                    .IsRequired()
                    .HasMaxLength(450);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
