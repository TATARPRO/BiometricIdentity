using BiometricIdentity.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

namespace BiometricIdentity.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        //Data Source=(LocalDb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\BiometricIdentity.mdf;Initial Catalog=BiometricIdentity;Integrated Security=True"providerName="System.Data.SqlClient
        // Server=(localdb)\\mssqllocaldb;Database=BiometricIdentity;Trusted_Connection=True;MultipleActiveResultSets=true
        //
        public ApplicationDbContext() : base("Data Source=SQL5103.site4now.net;Initial Catalog=DB_A6FBB5_AttendanceWokFox;User Id=DB_A6FBB5_AttendanceWokFox_admin;Password=NativeCode123@")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<FingerPrint> FingerPrints { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
        {
            return base.ValidateEntity(entityEntry, items);
        }
    }
}
