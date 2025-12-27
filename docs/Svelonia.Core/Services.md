[← Back to Svelonia.Core](./README.md)

# Services

Svelonia defines core interfaces for common application services.

## Dialog Service

The `IDialogService` provides an abstraction for showing alerts and confirmation dialogs, decoupling your ViewModels or business logic from specific UI implementations.

### Interface

```csharp
public interface IDialogService
{
    Task ShowAlertAsync(string title, string message);
    Task<DialogResult> ShowConfirmAsync(string title, string message, string okText = "Yes", string cancelText = "No", string? altText = null);
}
```

### Usage

You typically inject `IDialogService` into your pages or logic classes.

```csharp
public class MyPage : Page
{
    private readonly IDialogService _dialogs;

    public MyPage(IDialogService dialogs)
    {
        _dialogs = dialogs;
    }

    public async Task DeleteItem()
    {
        var result = await _dialogs.ShowConfirmAsync(
            "Delete Item", 
            "Are you sure you want to delete this item?",
            okText: "Delete",
            cancelText: "Cancel"
        );

        if (result == DialogResult.Ok || result == DialogResult.Yes)
        {
            // Perform delete
        }
    }
}
```

*> Note: Svelonia.Core provides the interface definition. You must register an implementation in your DI container (e.g., `Svelonia.Data`'s generic host builder or your own setup).*
