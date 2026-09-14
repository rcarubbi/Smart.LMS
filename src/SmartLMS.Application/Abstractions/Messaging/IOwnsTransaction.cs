namespace SmartLMS.Application.Abstractions.Messaging;

/// <summary>
/// Opt-out of the decorator's single ambient transaction: the command manages
/// its own transaction boundaries (e.g. commit-per-batch for long fan-out jobs).
/// </summary>
public interface IOwnsTransaction;
