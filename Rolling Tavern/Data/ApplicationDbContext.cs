using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Rolling_Tavern.Models;

namespace Rolling_Tavern.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>,long>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
           
        }

        public virtual DbSet<BoardGame> BoardGames { get; set; }
        public virtual DbSet<Meeting> Meetings { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<State> States { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            

            builder.Entity<BoardGame>(entity =>
            {
                entity.HasKey(e => e.GameId);

                entity.Property(e => e.GameName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Genre).HasMaxLength(50);
            });

            builder.Entity<Meeting>(entity =>
            {
                entity.Property(e => e.AddresOfMeeting)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DateOfMeeting).HasColumnType("datetime");

                entity.Property(e => e.MeetingName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PhotoLink).HasMaxLength(50);

                entity.Property(e => e.CreatorId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.CreatedMeetings)
                    .HasForeignKey(d => d.CreatorId)
                    .HasConstraintName("FK_Meetings_Users");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Meetings)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("FK_Meetings_BoardGames");
            });

            builder.Entity<Request>(entity =>
            {
                
                entity.HasKey(e=>e.UserId);
                entity.HasKey(e => e.MeetingId);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Requests_Users");

                entity.HasOne(d => d.Meeting)
                    .WithMany()
                    .HasForeignKey(d => d.MeetingId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Requests_Meetings");

                entity.HasOne(d => d.State)
                    .WithMany()
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Requests_States");
            });

            builder.Entity<State>(entity =>
            {
                entity.Property(e => e.StateId).ValueGeneratedNever();

                entity.Property(e => e.StateImage)
                    .IsRequired()
                    .HasMaxLength(450);
            });

            


            builder.Entity<ApplicationUser>()
                .Property(e => e.FirstName)
                .HasMaxLength(450);

            builder.Entity<ApplicationUser>()
                .Property(e => e.LastName)
                .HasMaxLength(450);

            builder.Entity<ApplicationUser>()
                .Property(e => e.ProfilePicture)
                .HasMaxLength(450);

            
        }
        
    }
}
