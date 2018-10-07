using Exico.Shopify.Data.Domain.DBModels;
using Microsoft.EntityFrameworkCore;

namespace Exico.Shopify.Data
{
    public sealed class ExicoShopifyDbContext : DbContext
    {
        public ExicoShopifyDbContext(DbContextOptions<ExicoShopifyDbContext> options) : base(options)
        {


        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            /**
             * IMPORTANT : Enable the following line after creating the 
             * initial migration.So that the model is created only in the initial
             * migration and after that not anymore.So keep it commented out.
             * We need the AspNetUser model to be created in the first migration for 
             * foreign key linking.
             * 
             **/
            //builder.Ignore<AspNetUser>();
            base.OnModelCreating(builder);
            builder.Entity<Plan>().HasIndex(p => new { p.Name }).IsUnique(true);
        }

        public DbSet<SystemSetting> SystemSettings { get; set; }

        public DbSet<AspNetUser> AspNetUsers { get; set; }
        public DbSet<PlanDefinition> PlanDefinitions { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserInbox> UserInboxes { get; set; }
    }
}
