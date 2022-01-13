using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Scruffy.Data.Entity.Keyless;
using Scruffy.Data.Entity.Repositories.Base;

namespace Scruffy.Data.Entity;

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

    /// <summary>
    /// Execution of raw sql
    /// </summary>
    /// <param name="sql">SQL-Script</param>
    /// <param name="parameters">Parameters</param>
    /// <returns>Number of rows affected</returns>
    public int? ExecuteSqlCommand(string sql, params object[] parameters)
    {
        int? value = null;

        try
        {
            value = _dbContext.Database.ExecuteSqlRaw(sql, parameters);
        }
        catch (Exception ex)
        {
            _dbContext.LastError = ex;
        }

        return value;
    }

    /// <summary>
    /// Execution of raw sql
    /// </summary>
    /// <param name="sql">SQL-Script</param>
    /// <param name="parameters">Parameters</param>
    /// <returns>Number of rows affected</returns>
    public async Task<int?> ExecuteSqlRawAsync(string sql, params object[] parameters)
    {
        int? value = null;

        try
        {
            value = await _dbContext.Database
                                    .ExecuteSqlRawAsync(sql, parameters)
                                    .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _dbContext.LastError = ex;
        }

        return value;
    }

    #region Query

    /// <summary>
    /// Select date range as query
    /// </summary>
    /// <param name="from">From</param>
    /// <param name="to">To</param>
    /// <returns>Date-Range-Query</returns>
    public IQueryable<DateValue> SelectDateRange(DateTime from, DateTime to)
    {
        return _dbContext.Set<DateValue>()
                         .FromSqlRaw(@"SELECT [Value] FROM GetDateRange(@from, @to)",
                                                      new SqlParameter("@from", from),
                                                      new SqlParameter("@to", to));
    }

    #endregion // Query

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