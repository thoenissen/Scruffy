using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Scruffy.Data.Entity.Queryable.Base
{
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

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return InternalQueryable.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)InternalQueryable).GetEnumerator();
        }

        #endregion // IEnumerable

        #region IQueryable

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable" /> is executed.
        /// </summary>
        /// <returns>A <see cref="T:System.Type" /> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.</returns>
        public Type ElementType => InternalQueryable.ElementType;

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.
        /// </summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.Expression" /> that is associated with this instance of <see cref="T:System.Linq.IQueryable" />.</returns>
        public Expression Expression => InternalQueryable.Expression;

        /// <summary>Gets the query provider that is associated with this data source.</summary>
        /// <returns>The <see cref="T:System.Linq.IQueryProvider" /> that is associated with this data source.</returns>
        public IQueryProvider Provider => InternalQueryable.Provider;

        #endregion // IQueryable
    }
}
