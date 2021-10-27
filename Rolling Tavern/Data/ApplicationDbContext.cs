using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Rolling_Tavern.Models;

namespace Rolling_Tavern.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
