[← Back to Svelonia.Data](./README.md)

# Mediator Pattern

The Mediator pattern is excellent for keeping your UI components (Pages, Controls) thin and focused purely on presentation. Business logic is moved to **Handlers**.

## Real-World Example: User Login

Let's walk through a complete example of handling a User Login feature using the Mediator pattern.

### 1. Define the Command
First, define a record representing the login action. It returns a `bool` indicating success (or you could return a complex `LoginResult`).

```csharp
using Svelonia.Data;

public record LoginCommand(string Username, string Password) : IRequest<bool>;
```

### 2. Implement the Handler
Create a handler that contains the business logic. You can inject other services (like an API client or Authentication service) via the constructor.

```csharp
public class LoginHandler : IRequestHandler<LoginCommand, bool>
{
    private readonly IAuthService _authService;

    // Dependency Injection works automatically
    public LoginHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(LoginCommand request, CancellationToken token)
    {
        // Your actual business logic here
        return await _authService.LoginAsync(request.Username, request.Password);
    }
}
```

### 3. Usage in a Page
Inject `IMediator` into your Page and call `Send`.

```csharp
using Svelonia.Core;
using Svelonia.Fluent;
using Svelonia.Kit;
using Svelonia.Data;

public class LoginPage : Page
{
    private readonly IMediator _mediator;
    
    // UI State
    private State<string> _username = new("");
    private State<string> _password = new("");
    private State<string> _status = new("");

    // Inject Mediator via constructor
    public LoginPage(IMediator mediator)
    {
        _mediator = mediator;

        Content = new StackPanel().Spacing(15).Children(
            new TextBlock().Text("Welcome Back").FontSize(24),
            
            new TextBox().BindText(_username).Watermark("Username"),
            new TextBox().BindText(_password).Watermark("Password").PasswordChar('*'),

            new Button()
                .Content("Login")
                .OnClick(OnLoginClick),

            new TextBlock().BindTextContent(_status).Foreground(Tw.Red500)
        );
    }

    private async void OnLoginClick(Avalonia.Interactivity.RoutedEventArgs e)
    {
        _status.Value = "Logging in...";

        // Send the command to the handler
        var success = await _mediator.Send(new LoginCommand(_username.Value, _password.Value));

        if (success)
        {
            _status.Value = "Success!";
            // Navigate to dashboard
            // Router.Navigate("/dashboard"); 
        }
        else
        {
            _status.Value = "Invalid credentials";
        }
    }
}
```

## AOT Support

Svelonia.Data is designed to be AOT-compatible. It uses a Source Generator to create a static registry of handlers, avoiding runtime reflection which is slow and unsafe in AOT environments.

**Important**: Always use the AOT-safe registration method in your `App.cs`:

```csharp
services.AddSveloniaDataAot(); // Good for AOT
// services.AddSveloniaData(assembly); // Avoid in AOT
```
