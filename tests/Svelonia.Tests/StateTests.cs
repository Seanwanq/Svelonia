using Xunit;
using Svelonia.Core;

namespace Svelonia.Tests;

public class StateTests
{
    [Fact]
    public void State_Should_Notify_Change()
    {
        var state = new State<int>(0);
        int receivedValue = -1;
        
        state.OnChange += val => receivedValue = val;

        state.Value = 10;

        Assert.Equal(10, receivedValue);
        Assert.Equal(10, state.Value);
    }

    [Fact]
    public void Computed_Should_Update_When_Dependency_Changes()
    {
        var count = new State<int>(1);
        var doubled = new Computed<int>(() => count.Value * 2);

        Assert.Equal(2, doubled.Value);

        count.Value = 5;

        Assert.Equal(10, doubled.Value);
    }
}
