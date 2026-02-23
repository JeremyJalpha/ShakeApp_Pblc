# Web Form Submission - Registry Pattern

## Overview

The web form submission system uses a **registry pattern** to handle different form types dynamically. This makes it easy to add new form types without modifying the core command handler.

## Architecture

```
#webform {formtype}: {data}
         ↓
WebFormSubmissionCommand
         ↓
FormHandlerRegistry → Looks up handler by form type
         ↓
IFormHandler (UserSignupFormHandler, FoodPoisoningFormHandler, etc.)
         ↓
Parse data → Save to DB → Return formatted response
```

## Command Format

```
#webform {formtype}: {field1}, {field2}, [bracketed field], ...
```

### Examples

**User Signup:**
```
#webform usersignup: [123 Main St], John Doe, john@example.com, Jane Doe, 555-1234, [Diabetes]
```

**Food Poisoning:**
```
#webform foodpoisoning: [123 Restaurant St], 3, severe, [chicken burger]
```

## Components

### 1. IFormHandler Interface

```csharp
public interface IFormHandler
{
    string FormType { get; }  // e.g., "usersignup", "foodpoisoning"
    Task<string> ProcessAsync(string formData, CommandContext context, CancellationToken cancellationToken);
}
```

### 2. FormHandlerRegistry

Stores registered form handlers and provides lookup by form type.

```csharp
public class FormHandlerRegistry
{
    public void Register(IFormHandler handler);
    public IFormHandler? GetHandler(string formType);
    public IEnumerable<string> GetRegisteredFormTypes();
}
```

### 3. WebFormSubmissionCommand

Main command that:
1. Parses the form type from the message
2. Looks up the appropriate handler in the registry
3. Delegates processing to the handler

### 4. Form Handlers

Each form type has its own handler that implements `IFormHandler`:

- **UserSignupFormHandler** - Handles user signup forms
- **FoodPoisoningFormHandler** - Handles food poisoning reports

### 5. FormParsingHelpers

Shared parsing utilities for extracting bracketed fields:
- `ParseFirstBracketGroup()` - Extract `[first field]`
- `ParseLastBracketGroup()` - Extract `[last field]`

## Adding a New Form Type

### Step 1: Create a DB Model (if needed)

```csharp
// CbTsSa_Shared/DBModels/MyNewForm.cs
public class MyNewForm
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public required string UserId { get; set; }
    
    // Add your fields here
    
    public DateTime DateTimeSubmitted { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }
}
```

### Step 2: Add DbSet to AppDbContext

```csharp
// CbTsSa_Shared/DBModels/AppDbContext.cs
public DbSet<MyNewForm> MyNewForms { get; set; }

// CbTsSa_Shared/Interfaces/IAppDbContext.cs
DbSet<MyNewForm> MyNewForms { get; set; }
```

### Step 3: Create a Form Handler

```csharp
// CommandBot/Commands/FormHandlers/MyNewFormHandler.cs
public class MyNewFormHandler : IFormHandler
{
    private readonly ILogger<MyNewFormHandler> _logger;

    public MyNewFormHandler(ILogger<MyNewFormHandler> logger)
    {
        _logger = logger;
    }

    public string FormType => "mynewform";

    public async Task<string> ProcessAsync(string formData, CommandContext context, CancellationToken cancellationToken)
    {
        if (context?.ConvoContext?.User == null)
            throw new InvalidOperationException("User context is missing");

        var submission = ParseMyNewForm(formData, context.ConvoContext.User);
        
        context.AppDbContext.MyNewForms.Add(submission);
        await context.AppDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("MyNewForm submitted by user {UserId}", context.ConvoContext.User.Id);

        return $"✅ MyNewForm submitted successfully.\n\n" +
               $"... format your response here ...";
    }

    private MyNewForm ParseMyNewForm(string commandText, ApplicationUser user)
    {
        // Use FormParsingHelpers for bracket groups
        var field1 = FormParsingHelpers.ParseFirstBracketGroup(ref commandText);
        var field2 = FormParsingHelpers.ParseLastBracketGroup(ref commandText);
        
        // Parse comma-separated fields
        var parts = commandText.Trim().Trim(',').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        return new MyNewForm
        {
            UserId = user.Id,
            // Map fields...
            DateTimeSubmitted = DateTime.UtcNow
        };
    }
}
```

### Step 4: Register the Handler

```csharp
// CommandBot/Program.cs

// Add to handler registrations:
builder.Services.AddScoped<CommandBot.Commands.FormHandlers.MyNewFormHandler>();

// Add to registry configuration:
var myNewFormHandler = scope.ServiceProvider.GetRequiredService<CommandBot.Commands.FormHandlers.MyNewFormHandler>();
registry.Register(myNewFormHandler);
```

### Step 5: Create an HTML Form (Optional)

```html
<!-- ShakeApp/wwwroot/mynewform.html -->
<script>
  const payload = `#webform mynewform: [${field1}], ${field2}, ${field3}, [${lastField}]`;
  const url = `https://wa.me/?text=${encodeURIComponent(payload)}`;
  window.open(url, '_blank');
</script>
```

## Benefits of Registry Pattern

✅ **Extensibility** - Add new form types without modifying core command  
✅ **Separation of Concerns** - Each form has its own handler  
✅ **Type Safety** - Each handler knows its DB model and validation  
✅ **Testability** - Mock individual handlers in unit tests  
✅ **Discoverability** - Registry lists all available form types  
✅ **Error Handling** - Unknown form types return helpful error with available types  

## Error Messages

### Unknown Form Type
```
❌ Unknown form type 'xyz'.

Available types: usersignup, foodpoisoning
```

### Invalid Format
```
❌ Invalid form data: Expected format: [address], Name, Email, ECName, ECNumber, [Reason]
```

### Missing User Context
```
❌ User context is missing.
```

## Testing

Test individual handlers:

```csharp
var handler = new UserSignupFormHandler(logger);
var result = await handler.ProcessAsync(
    "[123 Main St], John Doe, john@example.com, Jane Doe, 555-1234, [Diabetes]",
    context,
    CancellationToken.None
);
```

Test registry lookup:

```csharp
var registry = new FormHandlerRegistry();
registry.Register(new UserSignupFormHandler(logger));

var handler = registry.GetHandler("usersignup");
Assert.NotNull(handler);
```

## Future Enhancements

- **Dynamic Field Validation** - JSON schema for form definitions
- **Multi-Language Support** - Localized error messages
- **Form Templates** - Predefined form structures
- **Conditional Fields** - Show/hide fields based on previous answers
- **File Uploads** - Handle image/document submissions
