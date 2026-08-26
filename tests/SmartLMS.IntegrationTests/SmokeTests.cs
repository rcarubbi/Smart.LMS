namespace SmartLMS.IntegrationTests;

public class SmokeTests
{
    [Test]
    public async Task Test_runner_is_operational()
    {
        var value = Guid.NewGuid();

        await Assert.That(value).IsNotEqualTo(Guid.Empty);
    }
}
