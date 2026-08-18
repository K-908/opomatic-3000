namespace OpoMatic3000.UnitTests;

public sealed class ProjectSetupTests
{
    [Fact]
    public void Test_runner_loads_the_unit_test_assembly()
    {
        Assert.NotNull(typeof(ProjectSetupTests).Assembly);
    }
}
