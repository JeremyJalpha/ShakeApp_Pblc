using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CbTsSa_Shared.Models;

namespace CbTsSa_Shared.DBModels
{
    // Extending IdentityUser with additional fields
    public class ApplicationUser : IdentityUser
    {
        public string? SocialMedia { get; set; }
        public bool? POPIConsent { get; set; }
        public DateTime? DtTmJoined { get; set; }
        public bool IsVerified { get; set; } = false;

        [Obsolete("Use CellNumber instead. PhoneNumber is not reliable for messaging.")]
        public new string? PhoneNumber { get; set; }

        /// <summary>
        /// SYSTEM USE ONLY: Verified cell number for internal messaging/authentication.
        /// Users should NOT access this field directly. Use UserIndicatedCell for user-facing operations.
        /// This field is set by the system during user creation/authentication and should not be null in production.
        /// 
        /// IMPORTANT: When exposing user data via APIs, use UserInfoDto which excludes this sensitive field.
        /// This field is serialized for internal system communication (e.g., RabbitMQ messages).
        /// </summary>
        [MaxLength(18)]
        [Obsolete("For system use only. Use UserIndicatedCell for user-facing operations.", error: false)]
        public string CellNumber { get; set; } = "-1"; // Removed [JsonIgnore] to allow internal serialization

        /// <summary>
        /// User-editable cell number that users can update via commands.
        /// This is what users see and can modify.
        /// </summary>
        [MaxLength(18)]
        public string? UserIndicatedCell { get; set; }

        // Navigation properties
        public List<SignedUpWith>? SignedUpWithBusinesses { get; set; }

        // Store current order as JSON
        public string? CurrentOrderJson { get; set; }

        // Helper property to work with the order as a list of OrderItems
        public List<OrderItems> CurrentOrder
        {
            get => string.IsNullOrWhiteSpace(CurrentOrderJson)
                ? new List<OrderItems>()
                : JsonSerializer.Deserialize<List<OrderItems>>(CurrentOrderJson) ?? new List<OrderItems>();
            set => CurrentOrderJson = value.Count > 0
                ? JsonSerializer.Serialize(value)
                : null;
        }

        public string GetUserInfoAsAString()
        {
            string dateTimeJoined = DtTmJoined.HasValue ? DtTmJoined.Value.ToString("yyyy-MM-dd HH:mm:ss")
                : "Not specified";

            string cellDisplay = !string.IsNullOrWhiteSpace(UserIndicatedCell) 
                ? UserIndicatedCell 
                : "Not provided";

            string info = string.Format(@"Date Time Joined: {0}
Your Name: {1}
Your Email: {2}
Social: {3}
Cell: {4}
IsVerified: {5}
Consent: {6}
(needed to store & process your personal data)",
                dateTimeJoined, UserName, Email, SocialMedia, cellDisplay, IsVerified, POPIConsent);

            return info;
        }
    }
}