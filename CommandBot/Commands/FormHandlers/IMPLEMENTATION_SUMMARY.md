# Web Form System - Registry Pattern Implementation

## ✅ What Was Implemented

A **highly extensible registry pattern** for handling different web form types via WhatsApp messages.

## 🏗️ Architecture

```
User fills HTML form → WhatsApp message → CommandBot
                                              ↓
                              WebFormSubmissionCommand
                                              ↓
                              FormHandlerRegistry (lookup)
                                              ↓
                           IFormHandler implementation
                                              ↓
                              Parse → Validate → Save → Response
```

## 📁 Files Created/Modified

### Core Framework
1. **`CommandBot/Commands/FormHandlers/IFormHandler.cs`**
   - Interface that all form handlers implement
   - Defines `FormType` and `ProcessAsync()` method

2. **`CommandBot/Commands/FormHandlerRegistry.cs`**
   - Central registry for form handlers
   - Provides registration and lookup by form type

3. **`CommandBot/Commands/FormHandlers/FormParsingHelpers.cs`**
   - Shared utilities for parsing bracketed fields
   - Reusable across all form handlers

### Form Handlers
4. **`CommandBot/Commands/FormHandlers/UserSignupFormHandler.cs`**
   - Handles: `#webform usersignup: [address], name, email, ecName, ecNumber, [reason]`
   - Saves to: `UserSignUp` table

5. **`CommandBot/Commands/FormHandlers/FoodPoisoningFormHandler.cs`**
   - Handles: `#webform foodpoisoning: [address], peopleAffected, severity, [foodItem]`
   - Saves to: `FoodPoisoningReport` table

### Command Handler
6. **`CommandBot/Commands/WebFormSubmissionCommand.cs`** (refactored)
   - Pattern: `#webform {formtype}: {data}`
   - Delegates to appropriate handler via registry
   - Returns helpful error for unknown form types

### HTML Forms
7. **`ShakeApp/wwwroot/signup.html`** (updated)
   - Generates: `#webform usersignup: ...`
   - Opens WhatsApp with pre-filled message

8. **`ShakeApp/wwwroot/foodpoisoning.html`** (new)
   - Generates: `#webform foodpoisoning: ...`
   - Opens WhatsApp with pre-filled message

### Configuration
9. **`CommandBot/Program.cs`** (updated)
   - Registers `FormHandlerRegistry` as singleton
   - Registers all form handlers
   - Configures registry with handlers

### Documentation
10. **`CommandBot/Commands/FormHandlers/README.md`**
    - Complete guide for adding new form types
    - Architecture explanation
    - Code examples

## 🎯 Format Examples

### User Signup
```
#webform usersignup: [123 Main St], John Doe, john@example.com, Jane Doe, 555-1234, [Diabetes]
```

### Food Poisoning
```
#webform foodpoisoning: [Joe's Restaurant], 3, severe, [chicken burger]
```

## ✨ Benefits

### 1. **Extensibility**
- Add new form types without modifying `WebFormSubmissionCommand`
- Just create a new handler and register it

### 2. **Separation of Concerns**
- Each form type has its own handler
- Parsing logic is isolated
- Database models are specific to each form

### 3. **Type Safety**
- Each handler knows its DB model
- Compile-time checking
- No casting or reflection

### 4. **Maintainability**
- Clear structure
- Easy to find code for specific form
- Shared utilities avoid duplication

### 5. **Error Handling**
- Unknown form types return list of available types
- Validation errors are form-specific
- User gets helpful error messages

### 6. **Testability**
- Mock individual handlers
- Test parsing logic independently
- Test registry lookup

## 🚀 Adding a New Form Type (5 Steps)

### 1. Create DB Model
```csharp
// CbTsSa_Shared/DBModels/ContactRequest.cs
public class ContactRequest
{
    [Key] public int Id { get; set; }
    [Required] public required string UserId { get; set; }
    [Required] public required string Name { get; set; }
    [Required] public required string Phone { get; set; }
    [Required] public required string Message { get; set; }
    public DateTime DateTimeSubmitted { get; set; }
    [ForeignKey(nameof(UserId))] public ApplicationUser? User { get; set; }
}
```

### 2. Add to DbContext
```csharp
// AppDbContext.cs & IAppDbContext.cs
public DbSet<ContactRequest> ContactRequests { get; set; }
```

### 3. Create Handler
```csharp
// CommandBot/Commands/FormHandlers/ContactRequestHandler.cs
public class ContactRequestHandler : IFormHandler
{
    public string FormType => "contact";
    
    public async Task<string> ProcessAsync(string formData, CommandContext context, CancellationToken ct)
    {
        var request = ParseContactRequest(formData, context.ConvoContext.User);
        context.AppDbContext.ContactRequests.Add(request);
        await context.AppDbContext.SaveChangesAsync(ct);
        return "✅ Contact request received!";
    }
}
```

### 4. Register in Program.cs
```csharp
builder.Services.AddScoped<ContactRequestHandler>();
// ... in registry config:
registry.Register(scope.ServiceProvider.GetRequiredService<ContactRequestHandler>());
```

### 5. Create HTML Form (optional)
```html
<!-- wwwroot/contact.html -->
const payload = `#webform contact: ${name}, ${phone}, [${message}]`;
```

## 🔍 How It Works

### Message Flow
1. User submits HTML form
2. JavaScript builds WhatsApp message: `#webform {formtype}: {data}`
3. User sends message via WhatsApp
4. CommandBot receives message
5. `WebFormSubmissionCommand` extracts form type
6. `FormHandlerRegistry` looks up handler
7. Handler parses data, validates, saves to DB
8. Returns formatted success message

### Registration
- Handlers are registered at startup in `Program.cs`
- Registry is singleton (one instance per app)
- Handlers are scoped (one instance per request)

### Parsing
- Bracketed fields: `[content]` extracted via `FormParsingHelpers`
- Comma-separated fields: standard split
- First bracket = first field (often address)
- Last bracket = last field (often description/reason)

## 📊 Current Form Types

| Form Type | Command | DB Model | Fields |
|-----------|---------|----------|--------|
| `usersignup` | `#webform usersignup: ...` | `UserSignUp` | address, name, email, ecName, ecNumber, reason |
| `foodpoisoning` | `#webform foodpoisoning: ...` | `FoodPoisoningReport` | address, peopleAffected, severity, foodItem |

## 🎨 Error Messages

### Unknown Form Type
```
❌ Unknown form type 'xyz'.

Available types: usersignup, foodpoisoning
```

### Invalid Format
```
❌ Invalid form data: Expected format: [address], Name, Email, ECName, ECNumber, [Reason]
```

## 🧪 Testing Example

```csharp
[Fact]
public async Task UserSignup_ValidData_SavesSuccessfully()
{
    // Arrange
    var handler = new UserSignupFormHandler(logger);
    var data = "[123 Main St], John Doe, john@example.com, Jane Doe, 555-1234, [Diabetes]";
    
    // Act
    var result = await handler.ProcessAsync(data, context, CancellationToken.None);
    
    // Assert
    Assert.Contains("✅", result);
    Assert.Single(context.AppDbContext.UserSignUp);
}
```

## 🔮 Future Enhancements

- [ ] JSON schema validation
- [ ] Multi-step forms
- [ ] File upload support
- [ ] Form templates
- [ ] Conditional fields
- [ ] Multi-language support
- [ ] Email notifications
- [ ] Admin dashboard for submissions

## 📝 Key Design Decisions

1. **Registry Pattern** - Chosen for maximum extensibility
2. **Scoped Handlers** - Each request gets fresh handler instance
3. **Shared Parsing** - `FormParsingHelpers` avoids code duplication
4. **WhatsApp Integration** - Reuses existing messaging infrastructure
5. **Type Safety** - Strong typing via interfaces and models

---

**Status**: ✅ Complete and Build Successful  
**Next Steps**: Add migrations and test with real WhatsApp messages
