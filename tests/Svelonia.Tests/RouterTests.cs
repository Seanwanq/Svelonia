using Xunit;
using Svelonia.Kit;
using Svelonia.Core;
using Avalonia.Controls;

namespace Svelonia.Tests;

public class RouterTests
{
    // Helper to avoid actual component creation issues
    public class TestPage : Page 
    {
        public RouteParams? LastParams { get; private set; }
        public override Task OnLoadAsync(RouteParams routeParams)
        {
            LastParams = routeParams;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void Router_Should_Match_Simple_Path()
    {
        var router = new Router();
        bool factoryCalled = false;

        router.Register("/", async _ => 
        {
            factoryCalled = true;
            return new TestPage();
        });

        router.Navigate("/");
        
        Assert.True(factoryCalled);
        Assert.IsType<TestPage>(router.CurrentView.Value);
    }

    [Fact]
    public void Router_Should_Extract_Parameters()
    {
        var router = new Router();
        TestPage? page = null;

        router.Register("/user/{id}", async _ => 
        {
            page = new TestPage();
            return page;
        });

        router.Navigate("/user/123");
        
        Assert.NotNull(page);
        Assert.NotNull(page.LastParams);
        Assert.Equal("123", page.LastParams["id"]);
    }

    [Fact]
    public void Router_Should_Handle_Query_String()
    {
        var router = new Router();
        TestPage? page = null;

        router.Register("/search", async _ => 
        {
            page = new TestPage();
            return page;
        });

        router.Navigate("/search?q=avalonia&page=1");
        
        Assert.NotNull(page);
        Assert.Equal("avalonia", page.LastParams?["q"]);
        Assert.Equal("1", page.LastParams?["page"]);
    }

    [Fact]
    public void Router_Should_Handle_404()
    {
        var router = new Router();
        router.Register("/", async _ => new TestPage());

        router.Navigate("/unknown");
        
        // Router sets a TextBlock with "404"
        Assert.IsType<TextBlock>(router.CurrentView.Value);
        var tb = (TextBlock)router.CurrentView.Value!;
        Assert.Contains("404", tb.Text);
    }

    [Fact]
    public void Router_Guard_Should_Prevent_Navigation()
    {
        var router = new Router();
        bool visited = false;

        router.Register("/admin", async _ => 
        {
            visited = true;
            return new TestPage();
        });

        // Add a guard that blocks everything
        router.AddGuard(_ => Task.FromResult(false));

        router.Navigate("/admin");
        
        Assert.False(visited);
        Assert.Null(router.CurrentView.Value); // Should remain at default (null)
    }

    [Fact]
    public async Task Router_Should_Respect_KeepAlive()
    {
        var router = new Router();
        TestPage? firstInstance = null;

        router.Register("/cached", async _ => 
        {
            firstInstance = new TestPage();
            return firstInstance;
        }, keepAlive: true);

        // 1. Visit
        router.Navigate("/cached");
        var instance1 = router.CurrentView.Value;
        Assert.NotNull(instance1);

        // 2. Navigate away
        router.Navigate("/other"); // 404

        // 3. Visit again
        router.Navigate("/cached");
        var instance2 = router.CurrentView.Value;

        // Should be the same instance
        Assert.Same(instance1, instance2);
    }
}
