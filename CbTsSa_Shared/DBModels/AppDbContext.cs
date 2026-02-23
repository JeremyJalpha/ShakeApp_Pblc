using CbTsSa_Shared.Interfaces;
using CbTsSa_Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CbTsSa_Shared.DBModels
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>, IAppDbContext
    {
        public new DbSet<ApplicationUserToken> UserTokens { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Catalog> Catalogs { get; set; }
        public DbSet<CatalogItem> CatalogItems { get; set; }
        
        // Sale-related tables
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleBasket> SaleBaskets { get; set; }
        public DbSet<SaleStatus> SaleStatuses { get; set; }
        public DbSet<Status> Statuses { get; set; }
        
        // Basket structure
        public DbSet<EffectiveBasket> EffectiveBaskets { get; set; }
        public DbSet<GatheredBasket> GatheredBaskets { get; set; }
        public DbSet<Bundle> Bundles { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<AdditionalDiscount> AdditionalDiscounts { get; set; }
        
        // Products and services
        public DbSet<Purchasable> Purchasables { get; set; }
        public DbSet<Saleable> Saleables { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Good> Goods { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<OfferType> OfferTypes { get; set; }
        public DbSet<Special> Specials { get; set; }
        
        // Delivery
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryLeg> DeliveryLegs { get; set; }
        public DbSet<DeliveryDriverLeg> DeliveryDriverLegs { get; set; }
        
        // Payment
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Failed> FailedPayments { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        
        // Comments
        public DbSet<Comment> Comments { get; set; }
        public DbSet<FoodPoisoningReport> FoodPoisoningReports { get; set; }
        public DbSet<UserSignUp> UserSignUps { get; set; }
        public DbSet<SignedUpWith> SignedUpWith { get; set; }

        // Add this property with the other DbSets
        public DbSet<BroadcastTemplate> BroadcastTemplates { get; set; }
        public DbSet<CampaignImage> CampaignImages { get; set; }

        // Add these DbSets
        public DbSet<BroadcastCampaign> BroadcastCampaigns { get; set; }
        public DbSet<BroadcastMessage> BroadcastMessages { get; set; }
        public DbSet<UserIdImage> UserIdImages { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore OrderItems - it's not a database table, just a JSON DTO
            modelBuilder.Ignore<OrderItems>();

            // Identity configurations (keep existing)
            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
                b.Property(e => e.UserId);
                b.Property(e => e.LoginProvider);
                b.Property(e => e.Name);
                b.Property(e => e.Value);
                b.HasDiscriminator<string>("Discriminator")
                    .HasValue<IdentityUserToken<string>>("IdentityUserToken")
                    .HasValue<ApplicationUserToken>("1");
                b.ToTable("AspNetUserTokens");
            });

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.SocialMedia).HasMaxLength(255).IsRequired(false);
                b.Property(u => u.POPIConsent).IsRequired(false);
                b.Property(u => u.DtTmJoined).HasColumnType("timestamp with time zone").IsRequired(false);
                b.Property(u => u.IsVerified).HasDefaultValue(false).IsRequired();
                b.Property(u => u.UserIndicatedCell).HasMaxLength(18).IsRequired(false);
                b.Property(u => u.CellNumber).HasMaxLength(18).IsRequired();
                b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
                b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();

                // Add unique index on CellNumber
                b.HasIndex(u => u.CellNumber)
                    .IsUnique();
            });

            // Driver (keep existing)
            modelBuilder.Entity<Driver>(b =>
            {
                b.HasKey(d => d.UserID);
                b.Property(d => d.UserID).HasMaxLength(450);
                b.Property(d => d.LastOnline).IsRequired(false);
                b.Property(d => d.GPSLat).IsRequired(false);
                b.Property(d => d.GPSLong).IsRequired(false);
                b.Property(d => d.LocationLastUpdated).IsRequired(false);
                b.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Business (update existing configuration)
            modelBuilder.Entity<Business>(b =>
            {
                b.HasKey(b => b.BusinessID);
                
                // Indexes
                b.HasIndex(b => b.Cellnumber).IsUnique();
                b.HasIndex(b => new { b.Industry, b.TradingName }).IsUnique();
                b.HasIndex(b => b.RegisteredName).IsUnique();
                
                // Properties
                b.Property(b => b.BusinessName).HasMaxLength(25).IsRequired();
                b.Property(b => b.RegisteredName).HasMaxLength(25).IsRequired(false);
                b.Property(b => b.TradingName).HasMaxLength(25).IsRequired(false);
                b.Property(b => b.StreetAddress).HasMaxLength(70).IsRequired(false);
                b.Property(b => b.PostalAddress).HasMaxLength(70).IsRequired(false);
                b.Property(b => b.Email).HasMaxLength(40).IsRequired(false);
                b.Property(b => b.Website).HasMaxLength(40).IsRequired(false);
                b.Property(b => b.Facebook).HasMaxLength(50).IsRequired(false);
                b.Property(b => b.Twitter).HasMaxLength(50).IsRequired(false);
                b.Property(b => b.TradingHours).HasMaxLength(128).IsRequired(false);
                b.Property(b => b.GPSLat).HasColumnType("decimal(9,6)").IsRequired(false);
                b.Property(b => b.GPSLong).HasColumnType("decimal(9,6)").IsRequired(false);
            });

            // Catalog (update existing configuration)
            modelBuilder.Entity<Catalog>(c =>
            {
                c.HasKey(c => c.CatalogID);
                c.Property(c => c.CreatorUserID).HasMaxLength(450).IsRequired();
                c.Property(c => c.Name).HasMaxLength(70).IsRequired();
                c.Property(c => c.Description).HasMaxLength(128).IsRequired(false);
                c.Property(c => c.LastUpdated).IsRequired(false);
                
                c.HasOne(c => c.Business)
                    .WithMany(b => b.Catalogs)
                    .HasForeignKey(c => c.BusinessID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                c.HasOne(c => c.CreatorUser)
                    .WithMany()
                    .HasForeignKey(c => c.CreatorUserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CatalogItem
            modelBuilder.Entity<CatalogItem>(ci =>
            {
                ci.HasKey(ci => new { ci.CatalogID, ci.ItemID });

                // Unique index on MenuCode within a catalog
                ci.HasIndex(ci => new { ci.CatalogID, ci.MenuCode })
                    .IsUnique()
                    .HasDatabaseName("IX_CatalogItem_Catalog_MenuCode");

                ci.HasOne(ci => ci.Catalog)
                    .WithMany(c => c.CatalogItems)
                    .HasForeignKey(ci => ci.CatalogID)
                    .OnDelete(DeleteBehavior.Cascade);

                ci.HasOne(ci => ci.Item)
                    .WithMany(i => i.CatalogItems)
                    .HasForeignKey(ci => ci.ItemID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Sale
            modelBuilder.Entity<Sale>(s =>
            {
                s.HasKey(s => s.SaleID);
                s.Property(s => s.UserID).HasMaxLength(450).IsRequired();
                s.Property(s => s.DontPoolWithStrangers).IsRequired();
                s.Property(s => s.DtTmRequestedCheckout).IsRequired();
                s.Property(s => s.ItemCount).IsRequired(false);
                s.Property(s => s.IsAmtManual).IsRequired();
                s.Property(s => s.TotalAmount).HasColumnType("decimal(18,2)").IsRequired(false);
                s.Property(s => s.DtTmCompleted).IsRequired(false);
                
                s.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SaleBasket
            modelBuilder.Entity<SaleBasket>(sb =>
            {
                sb.HasKey(sb => new { sb.SaleID, sb.EffectiveBasketID });
                sb.HasIndex(sb => sb.EffectiveBasketID).IsUnique();
                
                sb.HasOne(sb => sb.Sale)
                    .WithMany(s => s.SaleBaskets)
                    .HasForeignKey(sb => sb.SaleID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                sb.HasOne(sb => sb.EffectiveBasket)
                    .WithOne()
                    .HasForeignKey<SaleBasket>(sb => sb.EffectiveBasketID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // EffectiveBasket
            modelBuilder.Entity<EffectiveBasket>(eb =>
            {
                eb.HasKey(eb => eb.EffectiveBasketID);
                eb.HasIndex(eb => eb.GatheredBasketID).IsUnique();
                eb.Property(eb => eb.EffectiveDiscount).HasColumnType("decimal(18,2)").IsRequired(false);
                eb.Property(eb => eb.AuthorisedByUserID).HasMaxLength(450).IsRequired(false);
                
                eb.HasOne(eb => eb.GatheredBasket)
                    .WithOne()
                    .HasForeignKey<EffectiveBasket>(eb => eb.GatheredBasketID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                eb.HasOne(eb => eb.AuthorisedByUser)
                    .WithMany()
                    .HasForeignKey(eb => eb.AuthorisedByUserID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // GatheredBasket
            modelBuilder.Entity<GatheredBasket>(gb =>
            {
                gb.HasKey(gb => gb.GatheredBasketID);
                gb.HasIndex(gb => new { gb.BundleID}).IsUnique();
                gb.Property(gb => gb.BundleMultiplier).IsRequired();
                
                gb.HasOne(gb => gb.Bundle)
                    .WithMany()
                    .HasForeignKey(gb => new { gb.BundleID})
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Bundle
            modelBuilder.Entity<Bundle>(b =>
            {
                b.HasKey(b => new { b.BundleID});
                b.HasIndex(b => new { b.BundleID}).IsUnique();
                b.Property(b => b.ItemID).IsRequired();

                b.HasOne(b => b.Item)
                    .WithMany()
                    .HasForeignKey(b => b.ItemID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Item
            modelBuilder.Entity<Item>(i =>
            {
                i.HasKey(i => i.ItemID);
                i.HasIndex(i => new { i.ItemID, i.PurchasableID, i.SpecialID }).IsUnique();
                
                i.HasOne(i => i.Purchasable)
                    .WithMany()
                    .HasForeignKey(i => i.PurchasableID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                i.HasOne(i => i.Special)
                    .WithMany()
                    .HasForeignKey(i => i.SpecialID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // AdditionalDiscount
            modelBuilder.Entity<AdditionalDiscount>(ad =>
            {
                ad.HasKey(ad => ad.GatheredBasketID);
                ad.Property(ad => ad.AuthorisedByUserID).HasMaxLength(450).IsRequired();
                ad.Property(ad => ad.Discount).HasColumnType("decimal(18,2)").IsRequired();
                ad.Property(ad => ad.DiscountReason).HasColumnType("text").IsRequired();
                
                ad.HasOne(ad => ad.GatheredBasket)
                    .WithMany(gb => gb.AdditionalDiscounts)
                    .HasForeignKey(ad => ad.GatheredBasketID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                ad.HasOne(ad => ad.AuthorisedByUser)
                    .WithMany()
                    .HasForeignKey(ad => ad.AuthorisedByUserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Purchasable
            modelBuilder.Entity<Purchasable>(p =>
            {
                p.HasKey(p => p.PurchasableID);
                p.HasIndex(p => new { p.PurchasableID, p.SaleableID, p.OfferID }).IsUnique();
                
                p.HasOne(p => p.Saleable)
                    .WithMany()
                    .HasForeignKey(p => p.SaleableID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                p.HasOne(p => p.Offer)
                    .WithMany()
                    .HasForeignKey(p => p.OfferID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Saleable
            modelBuilder.Entity<Saleable>(s =>
            {
                s.HasKey(s => s.SaleableID);
                s.Property(s => s.ServiceID).IsRequired(false);
                s.Property(s => s.ProductID).IsRequired(false);
                s.HasIndex(s => s.ServiceID).IsUnique();
                s.HasIndex(s => s.ProductID).IsUnique();
                
                s.HasOne(s => s.Service)
                    .WithOne()
                    .HasForeignKey<Saleable>(s => s.ServiceID)
                    .OnDelete(DeleteBehavior.SetNull);
                
                s.HasOne(s => s.Product)
                    .WithOne()
                    .HasForeignKey<Saleable>(s => s.ProductID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Product
            modelBuilder.Entity<Product>(p =>
            {
                p.HasKey(p => p.ProductID);
                p.HasIndex(p => new { p.ProductID, p.BusinessID }).IsUnique();
                p.HasIndex(p => p.ProductID).IsUnique();

                // Ensure GoodID is treated as the FK to Goods (Product now holds GoodID)
                p.Property(p => p.GoodID).IsRequired();

                // Ensure GoodID is treated as the FK to Goods (Product now holds GoodID)
                p.Property(p => p.GoodID).IsRequired();

                p.HasOne(p => p.Good)
                    .WithMany() // Good can be referenced by many Products
                    .HasForeignKey(p => p.GoodID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                p.HasOne(p => p.Business)
                    .WithMany(b => b.Products) // explicitly link to Business.Products to avoid shadow FK
                    .HasForeignKey(p => p.BusinessID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Good
            modelBuilder.Entity<Good>(g =>
            {
                g.HasKey(g => g.GoodID);
                g.Property(g => g.Name).HasMaxLength(40).IsRequired();
                g.Property(g => g.Description).HasMaxLength(70).IsRequired(false);
                g.Property(g => g.Image).IsRequired(false);
            });

            // Service
            modelBuilder.Entity<Service>(s =>
            {
                s.HasKey(s => s.ServiceID);
                s.HasIndex(s => new { s.BusinessID, s.Name }).IsUnique();
                s.Property(s => s.Name).HasMaxLength(40).IsRequired();
                s.Property(s => s.Description).HasMaxLength(128).IsRequired(false);
                s.Property(s => s.Price).HasColumnType("decimal(18,2)").IsRequired();
                
                s.HasOne(s => s.Business)
                    .WithMany(b => b.Services) // explicitly link to Business.Services to avoid shadow FK
                    .HasForeignKey(s => s.BusinessID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Offer
            modelBuilder.Entity<Offer>(o =>
            {
                o.HasKey(o => o.OfferID);
                o.Property(o => o.CreatedByUserID).HasMaxLength(450).IsRequired();
                o.Property(o => o.BasePrice).HasColumnType("decimal(18,2)").IsRequired(false);
                
                o.HasOne(o => o.OfferType)
                    .WithMany()
                    .HasForeignKey(o => o.OfferTypeID)
                    .OnDelete(DeleteBehavior.Restrict);
                
                o.HasOne(o => o.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(o => o.CreatedByUserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OfferType
            modelBuilder.Entity<OfferType>(ot =>
            {
                ot.HasKey(ot => ot.OfferTypeID);
                ot.Property(ot => ot.TypeName).HasMaxLength(40).IsRequired();
                ot.Property(ot => ot.Description).HasMaxLength(128).IsRequired(false);
            });

            // Special
            modelBuilder.Entity<Special>(sp =>
            {
                sp.HasKey(sp => sp.SpecialID);
                sp.Property(sp => sp.CreatedByUserID).HasMaxLength(450).IsRequired();
                sp.Property(sp => sp.SpecialName).HasMaxLength(20).IsRequired();
                sp.Property(sp => sp.SpecialDescription).HasMaxLength(80).IsRequired();
                sp.Property(sp => sp.Discount).HasColumnType("decimal(18,2)").IsRequired(false);
                sp.Property(sp => sp.IsActive).HasDefaultValue(true).IsRequired();
                
                sp.HasOne(sp => sp.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(sp => sp.CreatedByUserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Status
            modelBuilder.Entity<Status>(st =>
            {
                st.HasKey(st => st.StatusID);
                st.Property(st => st.StatusName).IsRequired();
                st.Property(st => st.StatusDescription).IsRequired();
            });

            // SaleStatus
            modelBuilder.Entity<SaleStatus>(ss =>
            {
                ss.HasKey(ss => new { ss.SaleID, ss.ChangedByUserID, ss.DtTmStatusChanged, ss.NewStatusID });
                ss.Property(ss => ss.ChangeReason).HasMaxLength(255).IsRequired(false);
                
                ss.HasOne(ss => ss.Sale)
                    .WithMany(s => s.SaleStatuses)
                    .HasForeignKey(ss => ss.SaleID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                ss.HasOne(ss => ss.Status)
                    .WithMany(st => st.SaleStatuses)
                    .HasForeignKey(ss => ss.NewStatusID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DeliveryLeg
            modelBuilder.Entity<DeliveryLeg>(dl =>
            {
                dl.HasKey(dl => new { dl.DeliveryLegID, dl.SaleID });
                dl.Property(dl => dl.DisputedReason).HasMaxLength(255).IsRequired(false);
                dl.Property(dl => dl.Resolution).HasMaxLength(255).IsRequired(false);
                
                dl.HasOne(dl => dl.Sale)
                    .WithMany(s => s.DeliveryLegs)
                    .HasForeignKey(dl => dl.SaleID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Delivery
            modelBuilder.Entity<Delivery>(d =>
            {
                d.HasKey(d => d.DeliveryID);
            });

            // DeliveryDriverLeg
            modelBuilder.Entity<DeliveryDriverLeg>(ddl =>
            {
                ddl.HasKey(ddl => new { ddl.DeliveryDriverLegID, ddl.DeliveryLegID, ddl.SaleID, ddl.DriverID });
                ddl.Property(ddl => ddl.DriverID).HasMaxLength(450).IsRequired();
                
                ddl.HasOne(ddl => ddl.DeliveryLeg)
                    .WithMany(dl => dl.DeliveryDriverLegs)
                    .HasForeignKey(ddl => new { ddl.DeliveryLegID, ddl.SaleID })
                    .OnDelete(DeleteBehavior.Cascade);
                
                ddl.HasOne(ddl => ddl.Delivery)
                    .WithMany(d => d.DeliveryDriverLegs)
                    .HasForeignKey(ddl => ddl.DeliveryID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                ddl.HasOne(ddl => ddl.Driver)
                    .WithMany()
                    .HasForeignKey(ddl => ddl.DriverID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment
            modelBuilder.Entity<Payment>(p =>
            {
                p.HasKey(p => p.PaymentID);
                p.Property(p => p.SenderEmail).HasMaxLength(256).IsRequired(false);
                p.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired(false);
                p.Property(p => p.CurrencyCode).HasMaxLength(5).IsRequired(false);
                p.Property(p => p.PayGateBankAuthID).HasMaxLength(10).IsRequired(false);
                
                p.HasOne(p => p.Sale)
                    .WithMany(s => s.Payments)
                    .HasForeignKey(p => p.SaleID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Failed
            modelBuilder.Entity<Failed>(f =>
            {
                f.HasKey(f => f.FailedID);
                f.Property(f => f.PayGateBankAuthID).HasMaxLength(10).IsRequired(false);
                
                f.HasOne(f => f.Payment)
                    .WithMany(p => p.Failures)
                    .HasForeignKey(f => f.PaymentID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Refund
            modelBuilder.Entity<Refund>(r =>
            {
                r.HasKey(r => r.RefundID);
                r.Property(r => r.PayGateBankAuthID).HasMaxLength(10).IsRequired(false);
                r.Property(r => r.Reason).HasMaxLength(50).IsRequired(false);
                
                r.HasOne(r => r.Payment)
                    .WithMany(p => p.Refunds)
                    .HasForeignKey(r => r.PaymentID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Comment
            modelBuilder.Entity<Comment>(c =>
            {
                c.HasKey(c => c.CommentID);
                c.Property(c => c.UserID).HasMaxLength(450).IsRequired();
                c.Property(c => c.CommentText).HasMaxLength(80).IsRequired(false);
                
                c.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                c.HasOne(c => c.Saleable)
                    .WithMany(s => s.Comments)
                    .HasForeignKey(c => c.SaleableID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                c.HasOne(c => c.ResponseToComment)
                    .WithMany()
                    .HasForeignKey(c => c.ResponseToCommentID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CampaignImage configuration
            modelBuilder.Entity<CampaignImage>(ci =>
            {
                ci.HasKey(ci => ci.CampaignImageId);

                // Explicit unbounded text in Postgres for BusinessMediaHandle
                ci.Property(ci => ci.BusinessMediaHandle)
                    .HasColumnType("text")
                    .IsRequired(false);

                // CRITICAL: Unique constraint - only ONE active image per user per business
                ci.HasIndex(ci => new { ci.BusinessId, ci.UploadedByUserId, ci.IsActive })
                    .IsUnique()
                    .HasFilter("\"IsActive\" = true")  // PostgreSQL syntax
                    .HasDatabaseName("IX_CampaignImage_OnePerUser");

                // Performance indexes
                ci.HasIndex(ci => new { ci.BusinessId, ci.UploadedDateTime });
                ci.HasIndex(ci => ci.MediaHandle);

                // Index for business-owned media handle used in template headers
                ci.HasIndex(ci => ci.BusinessMediaHandle)
                    .HasFilter("\"BusinessMediaHandle\" IS NOT NULL");  // Skip nulls

                // Relationships
                ci.HasOne(ci => ci.Business)
                    .WithMany()
                    .HasForeignKey(ci => ci.BusinessId)
                    .OnDelete(DeleteBehavior.Cascade);  // Images deleted with business

                ci.HasOne(ci => ci.UploadedByUser)
                    .WithMany()
                    .HasForeignKey(ci => ci.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);  // Prevent orphaned images
            });

            // BroadcastTemplate configuration
            modelBuilder.Entity<BroadcastTemplate>(bt =>
            {
                bt.HasKey(bt => bt.TemplateId);
                
                // CRITICAL: Only ONE template per user per business
                bt.HasIndex(bt => new { bt.BusinessId, bt.CreatedByUserId })
                    .IsUnique()
                    .HasDatabaseName("IX_BroadcastTemplate_OnePerUser");
                
                // Unique constraint: one template name per business
                bt.HasIndex(bt => new { bt.BusinessId, bt.Name })
                    .IsUnique();
                
                // Index for status queries
                bt.HasIndex(bt => new { bt.BusinessId, bt.Status, bt.CreatedDateTime });
                bt.HasIndex(bt => bt.WhatsAppTemplateId);
                
                // Relationships
                bt.HasOne(bt => bt.Business)
                    .WithMany()
                    .HasForeignKey(bt => bt.BusinessId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                bt.HasOne(bt => bt.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(bt => bt.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                bt.HasOne(bt => bt.CampaignImage)
                    .WithMany()
                    .HasForeignKey(bt => bt.CampaignImageId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // BroadcastCampaign limits
            modelBuilder.Entity<BroadcastCampaign>(bc =>
            {
                bc.HasKey(bc => bc.CampaignId);
                
                // Indexes for queries
                bc.HasIndex(bc => new { bc.BusinessId, bc.Status, bc.CreatedDateTime });
                bc.HasIndex(bc => new { bc.InitiatedByUserId, bc.CreatedDateTime });
                
                // Check constraint: recipient counts can't be negative (FIXED FOR POSTGRESQL - EF Core 9)
                bc.ToTable(t => t.HasCheckConstraint("CK_BroadcastCampaign_RecipientCounts", 
                    "\"TotalRecipients\" >= 0 AND \"SentCount\" >= 0 AND \"DeliveredCount\" >= 0 AND \"ReadCount\" >= 0 AND \"FailedCount\" >= 0"));
                
                // Relationships
                bc.HasOne(bc => bc.Template)
                    .WithMany()
                    .HasForeignKey(bc => bc.TemplateId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                bc.HasOne(bc => bc.Business)
                    .WithMany()
                    .HasForeignKey(bc => bc.BusinessId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                bc.HasOne(bc => bc.InitiatedByUser)
                    .WithMany()
                    .HasForeignKey(bc => bc.InitiatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BroadcastMessage limits
            modelBuilder.Entity<BroadcastMessage>(bm =>
            {
                bm.HasKey(bm => bm.MessageId);
                
                // Critical index for worker performance
                bm.HasIndex(bm => new { bm.Status, bm.CreatedDateTime })
                    .HasFilter("\"Status\" = 'Queued'");
    
                bm.HasIndex(bm => new { bm.CampaignId, bm.Status });
                bm.HasIndex(bm => bm.WhatsAppMessageId);
                
                // Relationships
                bm.HasOne(bm => bm.Campaign)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(bm => bm.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                bm.HasOne(bm => bm.Recipient)
                    .WithMany()
                    .HasForeignKey(bm => bm.RecipientUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SignedUpWith configuration
            modelBuilder.Entity<SignedUpWith>(sw =>
            {
                sw.HasKey(sw => sw.Id);
                sw.Property(sw => sw.UserId).IsRequired();
                sw.Property(sw => sw.BusinessId).IsRequired();
                sw.Property(sw => sw.DateTimeSignedUp).IsRequired();

                // Composite unique index to prevent duplicate signup records
                sw.HasIndex(sw => new { sw.UserId, sw.BusinessId })
                    .IsUnique()
                    .HasDatabaseName("IX_SignedUpWith_UserBusiness");

                // Relationships
                sw.HasOne(sw => sw.User)
                    .WithMany(u => u.SignedUpWithBusinesses)
                    .HasForeignKey(sw => sw.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                sw.HasOne(sw => sw.Business)
                    .WithMany(b => b.UsersSignedUp)
                    .HasForeignKey(sw => sw.BusinessId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserIdImage configuration
            modelBuilder.Entity<UserIdImage>(uii =>
            {
                uii.HasKey(uii => uii.Id);
                uii.Property(uii => uii.UserId).IsRequired();
                uii.Property(uii => uii.ImageType).HasMaxLength(10).IsRequired();
                uii.Property(uii => uii.MediaHandle).IsRequired(false);
                uii.Property(uii => uii.StoragePath).IsRequired(false);
                uii.Property(uii => uii.StorageType).HasMaxLength(20).IsRequired(false);
                uii.Property(uii => uii.Status).HasMaxLength(20).IsRequired();
                uii.Property(uii => uii.ErrorMessage).IsRequired(false);
                uii.Property(uii => uii.Platform).HasMaxLength(20).IsRequired(false);
                uii.Property(uii => uii.OriginalFilename).IsRequired(false);
                uii.Property(uii => uii.FileSizeBytes).IsRequired();
                uii.Property(uii => uii.UploadedDateTime).IsRequired();
                uii.Property(uii => uii.ProcessedDateTime).IsRequired(false);

                // Composite unique index: only one front and one back per user
                uii.HasIndex(uii => new { uii.UserId, uii.ImageType })
                    .IsUnique()
                    .HasDatabaseName("IX_UserIdImage_UserType");

                // Index on status for background worker queries
                uii.HasIndex(uii => uii.Status);

                // Relationships
                uii.HasOne(uii => uii.User)
                    .WithMany()
                    .HasForeignKey(uii => uii.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
