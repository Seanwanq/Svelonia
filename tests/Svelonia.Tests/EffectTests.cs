using Xunit;
using Svelonia.Core;

namespace Svelonia.Tests;

public class EffectTests
{
    [Fact]
    public void Effect_Should_Run_Immediately()
    {
        int runCount = 0;
        
        using var _ = Sve.Effect(() => 
        {
            runCount++;
        });

        Assert.Equal(1, runCount);
    }

    [Fact]
    public void Effect_Should_ReRun_When_Dependency_Changes()
    {
        var count = new State<int>(0);
        int lastValue = -1;
        int runCount = 0;

        using var _ = Sve.Effect(() => 
        {
            runCount++;
            lastValue = count.Value;
        });

        Assert.Equal(1, runCount);
        Assert.Equal(0, lastValue);

        count.Value = 1;

        Assert.Equal(2, runCount);
        Assert.Equal(1, lastValue);
    }

    [Fact]
    public void Effect_Should_Stop_After_Dispose()
    {
        var count = new State<int>(0);
        int runCount = 0;

        var effect = Sve.Effect(() => 
        {
            runCount++;
            var val = count.Value;
        });

        Assert.Equal(1, runCount);

        count.Value = 1;
        Assert.Equal(2, runCount);

        effect.Dispose();

        count.Value = 2;
        Assert.Equal(2, runCount); // Should not increase
    }

    [Fact]
    public void Effect_Should_Handle_Dynamic_Dependencies()
    {
        var toggle = new State<bool>(true);
        var stateA = new State<string>("A");
        var stateB = new State<string>("B");
        string result = "";

        using var _ = Sve.Effect(() => 
        {
            if (toggle.Value)
            {
                result = stateA.Value;
            }
            else
            {
                result = stateB.Value;
            }
        });

        // Initial run (toggle=true) -> reads stateA
        Assert.Equal("A", result);

        // Update stateA -> should trigger
        stateA.Value = "A2";
        Assert.Equal("A2", result);

        // Update stateB -> should NOT trigger (not a dependency yet)
        stateB.Value = "B2";
        Assert.Equal("A2", result);

        // Switch toggle -> reads stateB
        toggle.Value = false;
        Assert.Equal("B2", result);

        // Now update stateA -> should NOT trigger (dependency dropped)
        stateA.Value = "A3";
        Assert.Equal("B2", result);

        // Update stateB -> should trigger
        stateB.Value = "B3";
        Assert.Equal("B3", result);
    }

    [Fact]
    public void Effect_Should_Handle_Multiple_Dependencies()
    {
        var a = new State<int>(1);
        var b = new State<int>(2);
        int sum = 0;

        using var _ = Sve.Effect(() => 
        {
            sum = a.Value + b.Value;
        });

        Assert.Equal(3, sum);

        a.Value = 2;
        Assert.Equal(4, sum);

        b.Value = 3;
        Assert.Equal(5, sum);
    }
}
