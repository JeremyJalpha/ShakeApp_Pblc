using CbTsSa_Shared.DBModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CbTsSa_Shared.Interfaces
{
    public interface IAppDbContext : IDisposable
    {
        // Application-specific DbSets
        DbSet<ApplicationUser> Users { get; }
        DbSet<Driver> Drivers { get; }
        DbSet<Business> Businesses { get; }
        DbSet<Catalog> Catalogs { get; }
        DbSet<CatalogItem> CatalogItems { get; }
        
        // Sale-related tables
        DbSet<Sale> Sales { get; }
        DbSet<SaleBasket> SaleBaskets { get; }
        DbSet<SaleStatus> SaleStatuses { get; }
        DbSet<Status> Statuses { get; }
        
        // Basket structure
        DbSet<EffectiveBasket> EffectiveBaskets { get; }
        DbSet<GatheredBasket> GatheredBaskets { get; }
        DbSet<Bundle> Bundles { get; }
        DbSet<Item> Items { get; }
        DbSet<AdditionalDiscount> AdditionalDiscounts { get; }
        
        // Products and services
        DbSet<Purchasable> Purchasables { get; }
        DbSet<Saleable> Saleables { get; }
        DbSet<Product> Products { get; }
        DbSet<Good> Goods { get; }
        DbSet<Service> Services { get; }
        DbSet<Offer> Offers { get; }
        DbSet<OfferType> OfferTypes { get; }
        DbSet<Special> Specials { get; }
        
        // Delivery
        DbSet<Delivery> Deliveries { get; }
        DbSet<DeliveryLeg> DeliveryLegs { get; }
        DbSet<DeliveryDriverLeg> DeliveryDriverLegs { get; }
        
        // Payment
        DbSet<Payment> Payments { get; }
        DbSet<Failed> FailedPayments { get; }
        DbSet<Refund> Refunds { get; }
        
        // Comments
        DbSet<Comment> Comments { get; }

        // Identity Framework DbSets
        DbSet<IdentityRole> Roles { get; }
        DbSet<IdentityUserRole<string>> UserRoles { get; }
        DbSet<IdentityUserClaim<string>> UserClaims { get; }
        DbSet<IdentityUserLogin<string>> UserLogins { get; }
        DbSet<IdentityUserToken<string>> UserTokens { get; }
        DbSet<IdentityRoleClaim<string>> RoleClaims { get; }

        // WebForm properties
        DbSet<FoodPoisoningReport> FoodPoisoningReports { get; set; }
        DbSet<UserSignUp> UserSignUps { get; set; }
        DbSet<SignedUpWith> SignedUpWith { get; set; }
        DbSet<UserIdImage> UserIdImages { get; set; }
        DbSet<BroadcastTemplate> BroadcastTemplates { get; }
        DbSet<CampaignImage> CampaignImages { get; set; }
        DbSet<BroadcastCampaign> BroadcastCampaigns { get; set; }
        DbSet<BroadcastMessage> BroadcastMessages { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
    }
}
