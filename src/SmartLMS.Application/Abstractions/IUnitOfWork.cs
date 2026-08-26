namespace SmartLMS.Application.Abstractions;

/// <summary>Unit-of-work port used by the transaction decorator to wrap commands.</summary>
public interface IUnitOfWork
{
    /// <summary>Returns true when an ambient transaction is already active (nested sends reuse it).</summary>
    bool HasActiveTransaction { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken);

    Task CommitAsync(CancellationToken cancellationToken);

    Task RollbackAsync(CancellationToken cancellationToken);
}
