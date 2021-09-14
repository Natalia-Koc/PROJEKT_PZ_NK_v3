using Neo4j.Driver;
using PROJEKT_PZ_NK_v3.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace PROJEKT_PZ_NK_v3.DAL
{
    public class OfferContext : DbContext
    {
        public readonly IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

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
                /*modelBuilder.Entity<Comments>().HasRequired<Profile>(comment => comment.Profile)
                    .WithMany(profile => profile.Comments).HasForeignKey(comment => comment.ProfileID)
                    .WillCascadeOnDelete(false);*/
                //usuniecie cyklicznego usuwania 
                //ręczne definiowanie relacji i niecykliczne usuwanie 

                modelBuilder.Entity<Applications>().HasRequired<Profile>(application => application.Owner)
                    .WithMany(profile => profile.Applications).HasForeignKey(application => application.OwnerID)
                    .WillCascadeOnDelete(false);

                modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            }

        public System.Data.Entity.DbSet<PROJEKT_PZ_NK_v3.Models.Applications> Applications { get; set; }
    }
}