# About Knyaz.NUnit.AssertExpressions

The library simplifies unit tests assertion block. See the example below:

Say you have the Point class to be tested

```c#
class Point
{
    public int X {get;}
    public int Y {get;}
    public Point(x, y)
    {
        X = x;
        Y = x; //<- bug here
    }
}
```

and you want to test the constructor. Lets see how it will:

```c#
using NUnit.Framework;
using Knyaz.NUnit.AssertExpressions;

[TestFixture]
public class PointTests
{
    [Test]
    public static void ConstructorCreatesPoint() =>
        new Point(5, 6).Assert(point => point.X == 5 && point.Y == 6);
}
```

When you run the test you will see the message:

```
point.Y
  Expected: 6
  But was:  5
```

