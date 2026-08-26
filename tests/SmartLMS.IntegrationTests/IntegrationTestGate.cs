namespace SmartLMS.IntegrationTests;

/// <summary>
/// API-hosting tests feed configuration through process-wide environment variables,
/// so they must not overlap. This gate serializes them across test classes.
/// </summary>
internal static class IntegrationTestGate
{
    public static readonly SemaphoreSlim Instance = new(1, 1);
}
