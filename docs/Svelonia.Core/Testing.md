# Testing Guide

Svelonia's fine-grained reactivity makes it uniquely suited for high-speed unit testing. Because your UI logic depends on `State<T>`, you can verify complex behaviors without ever opening a window.

---

## 1. Testing Reactive Logic

Since `Computed<T>` and `State<T>` are pure C# objects, you can test them using standard testing frameworks like xUnit or NUnit.

```csharp
[Fact]
public void FullName_Should_Update_When_FirstName_Changes()
{
    // Arrange
    var first = new State<string>("John");
    var last = new State<string>("Doe");
    var full = new Computed<string>(() => $"{first.Value} {last.Value}");

    // Act
    first.Value = "Jane";

    // Assert
    Assert.Equal("Jane Doe", full.Value);
}
```

---

## 2. Component Testing (Headless)

Svelonia components are just Avalonia controls. You can instantiate them in a test and simulate user interaction.

```csharp
[Fact]
public void CounterPage_Increment_Should_Increase_Count()
{
    // Arrange
    var store = new CounterStore();
    var page = new CounterPage(store); // CounterPage : Page

    // Act
    // Simulate a click by invoking the command or logic directly
    store.Increment(); 

    // Assert
    // Check if the TextBlock inside the page correctly reflects the state
    var textBlock = page.FindDescendant<TextBlock>();
    Assert.Contains("Count: 1", textBlock.Text);
}
```

---

## 3. Testing Async Flows (`AwaitControl`)

You can mock your services and verify that the UI transitions correctly between Loading, Success, and Error states.

```csharp
[Fact]
public async Task AwaitControl_Should_Show_Data_After_Task_Completes()
{
    var tcs = new TaskCompletionSource<string>();
    var control = new AwaitControl<string>(
        taskFactory: () => tcs.Task,
        loading: () => new TextBlock().SetText("Loading..."),
        then: (data) => new TextBlock().SetText(data)
    );

    // Initial state: should be loading
    Assert.Equal("Loading...", ((TextBlock)control.Content).Text);

    // Complete the task
    tcs.SetResult("Hello World");
    await Task.Yield(); // Give the UI thread a moment to process

    // Final state: should show data
    Assert.Equal("Hello World", ((TextBlock)control.Content).Text);
}
```

---

## 4. UI Thread Marshaling

In Svelonia, `State` updates are safe from background threads, but `StateList` and UI changes are marshaled to the Avalonia `Dispatcher`.

When testing, if your code uses `Dispatcher.UIThread`, you may need to use a headless test runner or call `Dispatcher.UIThread.RunJobs()` manually to flush the queue.

---

## 5. Best Practices

1.  **Extract Stores**: Keep complex logic in "Store" or "ViewModel" classes that take `State<T>` in their constructor. This makes logic 100% independent of Avalonia.
2.  **Mocking Services**: Use `IMediator` (from `Svelonia.Data`) to decouple your Pages from actual API calls, making mocking trivial.
3.  **Snapshot Testing**: Since Svelonia UI is built in code, you can easily verify the structure of the generated visual tree.
