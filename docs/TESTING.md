# Testing Guide — MetroSolAPI

> xUnit + Moq | **21 tests passing** | Updated: 2026-05-16

---

## Test Project Structure

```
MetroSol.Tests/
├── ItemEntityTests.cs           ← Entity property and validation tests
├── RepositoryTests.cs           ← Repository mock tests
├── AssertionExamplesTests.cs    ← 16 assertion examples
├── TesteTemplate.cs             ← Copy-paste template for new tests
└── TesteTemplate.cs             ← Copy-paste template for new tests
```

---

## Running Tests

```powershell
dotnet test                                          # run all tests
dotnet test --filter "ClassName=ItemEntityTests"     # filter by class
dotnet test --verbosity detailed                     # detailed output
dotnet watch --project MetroSol.Tests test           # watch mode
dotnet test /p:CollectCoverage=true                  # with coverage
```

In Visual Studio: **Ctrl+E, T** opens Test Explorer.

---

## Writing Tests

### AAA Pattern

```csharp
[Fact]
public void DescriptiveName_WhatIsExpected_GivenCondition()
{
    // Arrange — set up data and dependencies
    var org = new Organization { Name = "Test" };

    // Act — execute the action
    var item = new Item { Tag = "CAL-001", Organization = org };

    // Assert — verify the result
    Assert.Equal("CAL-001", item.Tag);
}
```

### Theory (multiple data sets)

```csharp
[Theory]
[InlineData("TAG-001", true)]
[InlineData("TAG-002", true)]
[InlineData("", false)]
public void ValidateTag_ShouldVerify(string tag, bool expected)
{
    var isValid = !string.IsNullOrEmpty(tag);
    Assert.Equal(expected, isValid);
}
```

### Mock with Moq

```csharp
[Fact]
public async Task GetAll_ShouldReturnList()
{
    var mock = new Mock<IRepository<Item>>();
    var items = new List<Item> { new Item { Tag = "ITEM-1", Organization = new Organization { Name = "Org" } } };

    mock.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

    var result = await mock.Object.GetAllAsync();

    Assert.Single(result);
    mock.Verify(r => r.GetAllAsync(), Times.Once);
}
```

---

## Common Assertions

```csharp
Assert.Equal(expected, actual);            // equality
Assert.NotEqual(v1, v2);                   // inequality
Assert.Null(obj);  Assert.NotNull(obj);    // null check
Assert.True(c);    Assert.False(c);        // boolean
Assert.Empty(col); Assert.NotEmpty(col);   // collection emptiness
Assert.Single(col);                        // exactly 1 item
Assert.Contains(item, col);                // membership
Assert.Contains("sub", text);             // substring
Assert.StartsWith("start", text);
Assert.Throws<ArgumentNullException>(() => Code());
Assert.IsType<string>(obj);
Assert.InRange(50, 0, 100);
```

---

## Moq — Setup and Verify

```csharp
var mock = new Mock<IRepository<Item>>();

// Setup return value
mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Item { Tag = "TEST" });

// Conditional setup
mock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == Guid.Empty))).ReturnsAsync((Item?)null);

// Verification
mock.Verify(r => r.GetAllAsync(), Times.Once);
mock.Verify(r => r.GetAllAsync(), Times.Never);
mock.Verify(r => r.GetAllAsync(), Times.AtLeastOnce);
```

---

## Best Practices

**Do:** one test per behavior · descriptive names (`ShouldReturnError_WhenIdIsInvalid`) · AAA pattern · independent tests · realistic data

**Don't:** interdependent tests · access the real database · vague names (`Test1`) · `Thread.Sleep()` · complex logic inside tests

---

## Troubleshooting

**Test cannot find reference** → check `using MetroSol.Core.Entities;` and `using MetroSol.Core.Interfaces;`

**"The member required 'Organization' must be defined"** → Item requires `Organization`: `new Item { Tag = "T", Organization = new Organization { Name = "Org" } }`

**Test passes locally but fails in CI** → avoid system time dependencies, absolute paths, or real database access

---

## External References

- [xUnit docs](https://xunit.net/)
- [Moq quickstart](https://github.com/Moq/moq4/wiki/Quickstart)
- [Unit testing best practices](https://docs.microsoft.com/dotnet/core/testing/unit-testing-best-practices)
