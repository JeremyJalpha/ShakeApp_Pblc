using CbTsSa_Shared.DBModels;
using CommandBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CommandBot.Commands.FormHandlers
{
    /// <summary>
    /// Handles user signup form submissions
    /// Format: JSON payload with UserSignUp properties
    /// </summary>
    public class UserSignupFormHandler : IFormHandler
    {
        private readonly ILogger<UserSignupFormHandler> _logger;

        public UserSignupFormHandler(ILogger<UserSignupFormHandler> logger)
        {
            _logger = logger;
        }

        public string FormType => "usersignup";

        public async Task<string> ProcessAsync(string formData, CommandContext context, CancellationToken cancellationToken)
        {
            if (context?.ConvoContext?.User == null)
                throw new InvalidOperationException("User context is missing");

            // Check if user has already signed up
            var existingSignup = await context.AppDbContext.UserSignUps
                .FirstOrDefaultAsync(s => s.UserId == context.ConvoContext.User.Id, cancellationToken);

            if (existingSignup != null)
            {
                _logger.LogInformation(
                    "User {UserId} attempted duplicate signup (already signed up on {Date})",
                    context.ConvoContext.User.Id,
                    existingSignup.DateTimeSubmitted);

                return $"⚠️ You have already signed up on {existingSignup.DateTimeSubmitted:yyyy-MM-dd}.\n\n" +
                       $"If you need to update your information, please contact support.";
            }

            var submission = ParseUserSignup(formData, context.ConvoContext.User);

            context.AppDbContext.UserSignUps.Add(submission);

            // Update AspNetUsers table with user details from signup
            var user = context.ConvoContext.User;
            user.UserName = submission.Name;
            user.Email = submission.Email;
            user.NormalizedEmail = submission.Email.ToUpperInvariant();

            context.AppDbContext.Users.Update(user);

            await context.AppDbContext.SaveChangesAsync(cancellationToken);

            // Record the signup source if provided
            if (!string.IsNullOrWhiteSpace(submission.FirstSignedUpWith))
            {
                var business = await context.AppDbContext.Businesses
                    .FirstOrDefaultAsync(b => b.BusinessName == submission.FirstSignedUpWith, cancellationToken);

                if (business != null)
                {
                    var existingBusinessSignup = await context.AppDbContext.SignedUpWith
                        .FirstOrDefaultAsync(s => s.UserId == context.ConvoContext.User.Id && s.BusinessId == business.BusinessID, cancellationToken);

                    if (existingBusinessSignup == null)
                    {
                        var signedUpWith = new SignedUpWith
                        {
                            UserId = context.ConvoContext.User.Id,
                            BusinessId = business.BusinessID,
                            DateTimeSignedUp = DateTime.UtcNow
                        };

                        context.AppDbContext.SignedUpWith.Add(signedUpWith);
                        await context.AppDbContext.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation(
                            "User {UserId} signed up through business {BusinessName} (ID: {BusinessId})",
                            context.ConvoContext.User.Id,
                            submission.FirstSignedUpWith,
                            business.BusinessID);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "User {UserId} already associated with business {BusinessName} (ID: {BusinessId})",
                            context.ConvoContext.User.Id,
                            submission.FirstSignedUpWith,
                            business.BusinessID);
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Business '{BusinessName}' not found for user signup",
                        submission.FirstSignedUpWith);
                }
            }

            _logger.LogInformation(
                "User signup submitted by user {UserId} for address {Address}",
                context.ConvoContext.User.Id,
                submission.Address);

            return $"✅ User signup submitted successfully.\n\n" +
                   $"📍 Address: {submission.Address}\n" +
                   $"👤 Name: {submission.Name}\n" +
                   $"✉️ Email: {submission.Email}\n" +
                   $"📞 Emergency Contact: {submission.EmergencyContactName} ({submission.EmergencyContactNumber})\n" +
                   $"📝 Reason: {submission.Reason}\n" +
                   $"📅 Submitted: {submission.DateTimeSubmitted:yyyy-MM-dd HH:mm}";
        }

        private UserSignUp ParseUserSignup(string commandText, ApplicationUser user)
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var jsonDoc = JsonDocument.Parse(commandText);
                var root = jsonDoc.RootElement;

                // Extract and validate required fields
                var address = root.TryGetProperty("Address", out var addr) 
                    ? addr.GetString()?.Trim() ?? string.Empty 
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(address))
                    throw new ArgumentException("Address is required");

                var name = root.TryGetProperty("Name", out var nm) 
                    ? nm.GetString()?.Trim() ?? string.Empty 
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name is required");

                var email = root.TryGetProperty("Email", out var em) 
                    ? em.GetString()?.Trim() ?? string.Empty 
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email is required");

                var emergencyContactName = root.TryGetProperty("EmergencyContactName", out var ecn) 
                    ? ecn.GetString()?.Trim() ?? string.Empty 
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(emergencyContactName))
                    throw new ArgumentException("EmergencyContactName is required");

                var emergencyContactNumber = root.TryGetProperty("EmergencyContactNumber", out var ecnum) 
                    ? ecnum.GetString()?.Trim() ?? string.Empty 
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(emergencyContactNumber))
                    throw new ArgumentException("EmergencyContactNumber is required");

                var reason = root.TryGetProperty("Reason", out var rsn) 
                    ? rsn.GetString()?.Trim() ?? string.Empty 
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(reason))
                    throw new ArgumentException("Reason is required");

                var submission = new UserSignUp
                {
                    UserId = user.Id,
                    DateTimeSubmitted = DateTime.UtcNow,
                    Address = address,
                    Name = name,
                    Email = email,
                    EmergencyContactName = emergencyContactName,
                    EmergencyContactNumber = emergencyContactNumber,
                    Reason = reason,
                    FirstSignedUpWith = root.TryGetProperty("FirstSignedUpWith", out var firstSignup) ? firstSignup.GetString() : null
                };

                return submission;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format: {ex.Message}", ex);
            }
        }
    }
}
