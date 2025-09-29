using AuthMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AuthMicroservice.Infrastructure.Persistence
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> opts) : base(opts) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        //public DbSet<EmailToken> EmailTokens { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<ServiceClient> ServiceClients { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed roles de base
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = "Admin", Name = "Admin" },
                new Role { Id = "Player", Name = "Player" }
            );

            // UserRole composite key & relations
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            //// EmailToken primary key
            //modelBuilder.Entity<EmailToken>()
            //    .HasKey(et => et.Id);
            //modelBuilder.Entity<EmailToken>()
            //    .HasOne(et => et.User)
            //    .WithMany(u => u.EmailTokens)
            //    .HasForeignKey(et => et.UserId);

            // RefreshToken primary key & relation
            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Token);
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);

            // Primary key and unique ClientId for OAuth2 Client Credentials Flow
            modelBuilder.Entity<ServiceClient>()
                .HasKey(c => c.Id); 
            modelBuilder.Entity<ServiceClient>()
                .HasIndex(c => c.ClientId)
                .IsUnique();
        }
    }
}