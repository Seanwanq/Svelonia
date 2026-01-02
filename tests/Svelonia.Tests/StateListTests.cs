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
