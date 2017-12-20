using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace WriteThroughDatabaseToCache
{
   internal class ChangeTrackingBatch<T> : IDisposable
   {
      private readonly string _command;
      private SqlTransaction _transaction;
      private IEnumerable<T> _items;
      private SqlConnection _connection;
      private readonly object _param;

      public ChangeTrackingBatch( string connectionString, string command, object param = null )
      {
         _connection = new SqlConnection( connectionString );
         _command = command;
         _param = param;
      }

      public async Task<IEnumerable<T>> GetItemsAsync( )
      {
         if ( _items != null )
         {
            return _items;
         }

         _connection.Open();
         _transaction = _connection.BeginTransaction( System.Data.IsolationLevel.Snapshot );
         _items = await _connection.QueryAsync<T>( _command, _param, _transaction );
         return _items;
      }

      public void Commit()
      {
         _transaction?.Commit();
         _connection?.Close();
      }

      public void Dispose()
      {
         Dispose( true );
         GC.SuppressFinalize( this );
      }

      protected virtual void Dispose( bool disposing )
      {
         if ( disposing )
         {
            _transaction?.Dispose();

            _connection?.Dispose();
         }
      }
   }
}