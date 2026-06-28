using System.Linq.Expressions;

namespace NaarNoor.Application.Tests.Common.Fixtures;

/// <summary>
/// Wraps an in-memory collection in an IQueryable that also implements IAsyncEnumerable,
/// allowing EF Core ToListAsync / FirstOrDefaultAsync calls to work in unit tests
/// without a real database or InMemory provider.
/// </summary>
internal sealed class TestAsyncQueryable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncQueryable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncQueryable(Expression expression) : base(expression) { }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}

internal sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

internal sealed class TestAsyncQueryProvider<TEntity> : IQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression)
        => new TestAsyncQueryable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new TestAsyncQueryable<TElement>(expression);

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
}

/// <summary>
/// Extension helpers for creating async-capable queryables in tests.
/// </summary>
internal static class TestAsyncQueryableExtensions
{
    public static IQueryable<T> AsAsyncTestQueryable<T>(this IEnumerable<T> source)
        => new TestAsyncQueryable<T>(source);
}
