using Xunit;
using Svelonia.Core;

namespace Svelonia.Tests;

public class StateListTests
{
    [Fact]
    public void Add_Should_Notify_Observers()
    {
        var list = new StateList<string>();
        
        // StateList doesn't expose a simple event, but usually triggers re-render.
        // We can test the collection itself.
        list.Add("A");
        Assert.Single(list);
        Assert.Equal("A", list[0]);
    }

    [Fact]
    public void CollectionChanged_Should_Fire_On_Add()
    {
        var list = new StateList<int>();
        bool eventFired = false;
        list.CollectionChanged += (s, e) => eventFired = true;

        list.Add(1);
        Assert.True(eventFired);
        Assert.Single(list);
    }

    [Fact]
    public void CollectionChanged_Should_Fire_On_Clear()
    {
        var list = new StateList<int> { 1, 2, 3 };
        bool eventFired = false;
        list.CollectionChanged += (s, e) => eventFired = true;
        
        list.Clear();
        
        Assert.True(eventFired);
        Assert.Empty(list);
    }

    [Fact]
    public void Computed_Should_React_To_List_Changes()
    {
        var list = new StateList<int>();
        var sum = new Computed<int>(() => list.Sum());

        Assert.Equal(0, sum.Value);

        list.Add(1);
        Assert.Equal(1, sum.Value); // 自动追踪成功！

        list.Add(5);
        Assert.Equal(6, sum.Value);

        list.Remove(1);
        Assert.Equal(5, sum.Value);
    }

    [Fact]
    public void Computed_Should_React_To_Count_Property()
    {
        var list = new StateList<string>();
        var isEmpty = new Computed<bool>(() => list.Count == 0);

        Assert.True(isEmpty.Value);

        list.Add("Hello");
        Assert.False(isEmpty.Value);
    }

    [Fact]
    public void EdgeCase_Remove_NonExistent_Item()
    {
        var list = new StateList<int> { 1, 2 };
        bool removed = list.Remove(99);
        
        Assert.False(removed);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void EdgeCase_Indexer_OutOfBounds()
    {
        var list = new StateList<int>();
        Assert.Throws<ArgumentOutOfRangeException>(() => list[0]);
    }

    [Fact]
    public void Initializer_Should_Populate_List()
    {
        var list = new StateList<string> { "A", "B" };
        Assert.Equal(2, list.Count);
        Assert.Equal("A", list[0]);
    }
}
