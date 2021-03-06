﻿using EcommerceApi.Models.Moneris;
using EcommerceApi.Models.Website;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Models
{
    public partial class EcommerceContext : IdentityDbContext<ApplicationUser>
    {
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<CustomerUsers> CustomerUsers { get; set; }

        public virtual DbSet<CustomerStoreCredit> CustomerStoreCredit { get; set; }
        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<ApplicationStep> ApplicationStep { get; set; }
        public virtual DbSet<ApplicationStepDetail> ApplicationStepDetail { get; set; }
        public virtual DbSet<ApplicationStepDetailTag> ApplicationStepDetailTag { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderDetail> OrderDetail { get; set; }
        public virtual DbSet<OrderPayment> OrderPayment { get; set; }
        public virtual DbSet<OrderTax> OrderTax { get; set; }
        public virtual DbSet<PaymentType> PaymentType { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductPackage> ProductPackage { get; set; }
        public virtual DbSet<LoginHistory> LoginHistory { get; set; }
        public virtual DbSet<ProductType> ProductType { get; set; }
        public virtual DbSet<Tax> Tax { get; set; }
        public virtual DbSet<ProductInventory> ProductInventory { get; set; }
        public virtual DbSet<ProductInventoryHistory> ProductInventoryHistory { get; set; }
        public virtual DbSet<UserLocation> UserLocation { get; set; }
        public virtual DbSet<Purchase> Purchase { get; set; }
        public virtual DbSet<PurchaseDetail> PurchaseDetail { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<PortalSettings> PortalSettings { get; set; }
        public virtual DbSet<MonerisTransactionLog> MonerisTransactionLog { get; set; }
        public virtual DbSet<MonerisCallbackLog> MonerisCallbackLog { get; set; }
        public virtual DbSet<ClientPosSettings> ClientPosSettings { get; set; }
        public virtual DbSet<Tag> Tag { get; set; }
        public virtual DbSet<ProductTag> ProductTag { get; set; }
        public virtual DbSet<CustomerStatementSetting> CustomerStatementSetting { get; set; }
        public virtual DbSet<InvoiceEmailAndPrintSetting> InvoiceEmailAndPrintSetting { get; set; }
        public virtual DbSet<ProductWebsite> ProductWebsite { get; set; }
        public virtual DbSet<WebsiteSlider> WebsiteSlider { get; set; }
        public virtual DbSet<BlogPost> BlogPost { get; set; }
        public virtual DbSet<ProductWebsiteImage> ProductWebsiteImage { get; set; }
        public virtual DbSet<WebsiteAbout> WebsiteAbout { get; set; }
        public virtual DbSet<WebsiteAboutPopOver> WebsiteAboutPopOver { get; set; }
        public virtual DbSet<WebsiteFaq> WebsiteFaq { get; set; }
        public virtual DbSet<WebsitePage> WebsitePage { get; set; }
        public EcommerceContext(DbContextOptions<EcommerceContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // optionsBuilder.UseSqlServer(@"connection string");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CustomerUsers>().HasKey(c => new { c.UserId, c.CustomerId });

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Remove 'AspNet' prefix and convert table name from PascalCase to snake_case. E.g. AspNetRoleClaims -> role_claims
                entity.Relational().TableName = entity.Relational().TableName.Replace("AspNet", "");

                // Convert column names from PascalCase to snake_case.
                foreach (var property in entity.GetProperties())
                {
                    property.Relational().ColumnName = property.Name;
                }

                // Convert primary key names from PascalCase to snake_case. E.g. PK_users -> pk_users
                foreach (var key in entity.GetKeys())
                {
                    key.Relational().Name = key.Relational().Name;
                }

                // Convert foreign key names from PascalCase to snake_case.
                foreach (var key in entity.GetForeignKeys())
                {
                    key.Relational().Name = key.Relational().Name;
                }

                // Convert index names from PascalCase to snake_case.
                foreach (var index in entity.GetIndexes())
                {
                    index.Relational().Name = index.Relational().Name;
                }
            }
        }
    }
}
