using System;
using System.Collections.Generic;
using System.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Scruffy.Data.Entity.Repositories.Base;

namespace Scruffy.Data.Entity
{
    /// <summary>
    /// Factory for creating repositories.
    /// </summary>
    public sealed class RepositoryFactory : IDisposable
    {
        #region Fields

        /// <summary>
        /// Internal <see cref="DbContext"/>-object
        /// </summary>
        private ScruffyDbContext _dbContext;

        /// <summary>
        /// Repositories
        /// </summary>
        private Dictionary<Type, RepositoryBase> _repositories;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public RepositoryFactory()
        {
            _dbContext = new ScruffyDbContext();
            _repositories = new Dictionary<Type, RepositoryBase>();
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Last occured error
        /// </summary>
        public Exception LastError => _dbContext.LastError;

        #endregion // Properties

        #region Static methods

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <returns>A new new instance of <see cref="RepositoryFactory"/></returns>
        public static RepositoryFactory CreateInstance()
        {
            return new RepositoryFactory();
        }

        #endregion // Static methods

        #region Methods

        /// <summary>
        /// Creates a new repository object or returns the already existing object
        /// </summary>
        /// <typeparam name="TRepository">Type of the repository to be created</typeparam>
        /// <returns><see cref="RepositoryBase"/>-object</returns>
        public TRepository GetRepository<TRepository>() where TRepository : RepositoryBase
        {
            if (_repositories.TryGetValue(typeof(TRepository), out var repository) == false)
            {
                repository = _repositories[typeof(TRepository)] = (TRepository)Activator.CreateInstance(typeof(TRepository), _dbContext);
            }

            return (TRepository)repository;
        }

        /// <summary>
        /// Begins a new transaction. This transaction is valid for all created repositories
        /// </summary>
        /// <param name="isolationLevel">Specifies the transaction locking behavior for the connection</param>
        /// <returns><see cref="IDbContextTransaction"/>-object</returns>
        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return _dbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        #endregion // Methods

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _dbContext?.Dispose();
            _dbContext = null;
        }

        #endregion // IDisposable
    }
}
