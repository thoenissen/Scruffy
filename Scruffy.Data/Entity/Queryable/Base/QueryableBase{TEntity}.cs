using System.Collections;
using System.Linq.Expressions;

namespace Scruffy.Data.Entity.Queryable.Base;

/// <summary>
/// Base class for creating queryable objects
/// </summary>
/// <typeparam name="TEntity">Type of the entity</typeparam>
public class QueryableBase<TEntity> : IQueryable<TEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public QueryableBase(IQueryable<TEntity> queryable)
    {
        InternalQueryable = queryable;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Internal queryable
    /// </summary>
    protected IQueryable<TEntity> InternalQueryable { get; private set; }

    #endregion // Properties

    #region IEnumerable

    /// <summary>
    /// Returns an enumerator that iterates through the collection
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection</returns>
    public IEnumerator<TEntity> GetEnumerator()
    {
        return InternalQueryable.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection
    /// </summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)InternalQueryable).GetEnumerator();
    }

    #endregion // IEnumerable

    #region IQueryable

    /// <inheritdoc/>
    public Type ElementType => InternalQueryable.ElementType;

    /// <inheritdoc/>
    public Expression Expression => InternalQueryable.Expression;

    /// <inheritdoc/>
    public IQueryProvider Provider => InternalQueryable.Provider;

    #endregion // IQueryable
}