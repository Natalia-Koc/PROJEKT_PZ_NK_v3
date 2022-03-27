using PROJEKT_PZ_NK_v3.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace PROJEKT_PZ_NK_v3.DAL
{
        public class OfferContext : DbContext
        {
            public OfferContext()
                : base("DefaultConnection")
            {
            }

            public DbSet<Profile> Profiles { get; set; }
            public DbSet<Animal> Animals { get; set; }
            public DbSet<Offer> Offers { get; set; }
            public DbSet<Comments> Comments { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Comments>().HasRequired<Profile>(comment => comment.Profile)
                    .WithMany(profile => profile.Comments).HasForeignKey(comment => comment.ProfileID)
                    .WillCascadeOnDelete(false);

            
                modelBuilder.Entity<Offer>().HasRequired<Profile>(sp => sp.Profile)
                    .WithMany(profile => profile.Offers).HasForeignKey(sp => sp.ProfileID)
                    .WillCascadeOnDelete(false);
            

            
                modelBuilder.Entity<Notification>().HasRequired<Profile>(sp => sp.Profile)
                    .WithMany(profile => profile.Notifications).HasForeignKey(sp => sp.ProfileID)
                    .WillCascadeOnDelete(false);

            
                modelBuilder.Entity<SavedProfiles>().HasRequired<Profile>(sp => sp.MyProfile)
                    .WithMany(profile => profile.MySavedProfiles).HasForeignKey(sp => sp.MyProfileID)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<SavedProfiles>().HasRequired<Profile>(sp => sp.SavedProfile)
                    .WithMany(profile => profile.SavedProfiles).HasForeignKey(sp => sp.SavedProfileID)
                    .WillCascadeOnDelete(false);
                //usuniecie cyklicznego usuwania 
                //ręczne definiowanie relacji i niecykliczne usuwanie 
                
                modelBuilder.Entity<Applications>().HasRequired<Profile>(application => application.Owner)
                    .WithMany(profile => profile.OwnerApplications).HasForeignKey(application => application.OwnerID)
                    .WillCascadeOnDelete(false);
            
                modelBuilder.Entity<Applications>().HasRequired<Profile>(application => application.Guardian)
                    .WithMany(profile => profile.GuardianApplications).HasForeignKey(application => application.GuardianID)
                    .WillCascadeOnDelete(false);

                modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            }

        public System.Data.Entity.DbSet<PROJEKT_PZ_NK_v3.Models.Applications> Applications { get; set; }

        public System.Data.Entity.DbSet<PROJEKT_PZ_NK_v3.Models.SavedProfiles> SavedProfiles { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
    }