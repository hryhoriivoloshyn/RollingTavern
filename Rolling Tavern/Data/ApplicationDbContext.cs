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
        public virtual DbSet<GameImage> GameImages { get; set; }

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


            builder.Entity<GameImage>(entity =>
            {
                entity.HasKey(e => e.ImageId);

                entity.Property(e => e.ImagePath)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Images)
                    .HasForeignKey(d => d.GameId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Games_Images");


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
                    .HasMaxLength(450);

                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.CreatedMeetings)
                    .HasForeignKey(d => d.CreatorId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Meetings_Users");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Meetings)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("FK_Meetings_BoardGames");
            });

            builder.Entity<Request>(entity =>
            {
                
                entity.HasKey(e=>new {e.UserId, e.MeetingId});

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p=>p.Requests)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Requests_Users");

                entity.HasOne(d => d.Meeting)
                    .WithMany(p=>p.Requests)
                    .HasForeignKey(d => d.MeetingId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Requests_Meetings");

                entity.HasOne(d => d.State)
                    .WithMany(p=>p.Requests)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Requests_States");
                entity.Property(e => e.Rated)
                    .IsRequired()
                    .HasDefaultValue(false);
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

            builder.Entity<ApplicationUser>()
                .Property(e => e.Rating)
                .HasDefaultValue(1000);


        }
        
    }
}
