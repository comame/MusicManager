using Core;

namespace Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var msg = Class1.GetMessage();
        Assert.Equal("Hello, world!!", msg);
    }
}
