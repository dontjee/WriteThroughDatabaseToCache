using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace WriteThroughDatabaseToCache
{
   internal class ChangeTrackingRepository
   {
      private string _connectionString;

      public ChangeTrackingRepository( string connectionString )
      {
         _connectionString = connectionString;
      }

      internal Task<IEnumerable<UserAccountChangeModel>> GetLatestUserChangesAsync()
      {
         string cmd = @"
DECLARE @last_synchronization_version BIGINT = (SELECT LastSynchronizationVersion FROM dbo.ChangeTrackingHistory WHERE TableName = 'dbo.UserAccount')

DECLARE @current_synchronization_version BIGINT = CHANGE_TRACKING_CURRENT_VERSION(); 
SELECT ua.UserAccountId, ua.Email, ua.DisplayName, ua.CreateDate
		, CASE WHEN ct.SYS_CHANGE_OPERATION = 'I' THEN 'Insert' WHEN ct.SYS_CHANGE_OPERATION = 'U' THEN 'Update' ELSE 'Delete' END AS OperationType
FROM dbo.UserAccount AS ua
	RIGHT OUTER JOIN CHANGETABLE(CHANGES dbo.UserAccount, @last_synchronization_version) AS ct ON ua.UserAccountId = ct.UserAccountId

UPDATE dbo.ChangeTrackingHistory
SET LastSynchronizationVersion = @current_synchronization_version
WHERE TableName = 'dbo.UserAccount'
";
         return RunCommandInDatabaseWithSnapshotIsolationModeAsync<UserAccountChangeModel>( cmd );
      }

      internal Task<IEnumerable<ChannelChangeModel>> GetLatestChannelChangesAsync()
      {
         string cmd = @"
DECLARE @last_synchronization_version BIGINT = (SELECT LastSynchronizationVersion FROM dbo.ChangeTrackingHistory WHERE TableName = 'dbo.Channel')

DECLARE @current_synchronization_version BIGINT = CHANGE_TRACKING_CURRENT_VERSION(); 
SELECT  
    c.ChannelId, c.Title, c.UserAccountId, c.ModifyDate
	, CASE WHEN ct.SYS_CHANGE_OPERATION = 'I' THEN 'Insert' WHEN ct.SYS_CHANGE_OPERATION = 'U' THEN 'Update' ELSE 'Delete' END AS OperationType
FROM  dbo.Channel AS c
	RIGHT OUTER JOIN CHANGETABLE(CHANGES dbo.Channel, @last_synchronization_version) AS ct ON c.ChannelId = ct.ChannelId

UPDATE dbo.ChangeTrackingHistory
SET LastSynchronizationVersion = @current_synchronization_version
WHERE TableName = 'dbo.Channel'
";
         return RunCommandInDatabaseWithSnapshotIsolationModeAsync<ChannelChangeModel>( cmd );
      }

      internal Task<IEnumerable<MediaChangeModel>> GetLatestMediaChangesAsync()
      {
         string cmd = @"
DECLARE @last_synchronization_version BIGINT = (SELECT LastSynchronizationVersion FROM dbo.ChangeTrackingHistory WHERE TableName = 'dbo.Media')

DECLARE @current_synchronization_version BIGINT = CHANGE_TRACKING_CURRENT_VERSION(); 
SELECT  
    m.MediaId, m.ChannelId, m.Title, m.Height, m.Width, m.Duration, m.ContentUrl
	, CASE WHEN ct.SYS_CHANGE_OPERATION = 'I' THEN 'Insert' WHEN ct.SYS_CHANGE_OPERATION = 'U' THEN 'Update' ELSE 'Delete' END AS OperationType
FROM  dbo.Media AS m
	RIGHT OUTER JOIN CHANGETABLE(CHANGES dbo.Media, @last_synchronization_version) AS ct ON m.MediaId = ct.MediaId

UPDATE dbo.ChangeTrackingHistory
SET LastSynchronizationVersion = @current_synchronization_version
WHERE TableName = 'dbo.Media'
";
         return RunCommandInDatabaseWithSnapshotIsolationModeAsync<MediaChangeModel>( cmd );
      }

      private async Task<IEnumerable<T>> RunCommandInDatabaseWithSnapshotIsolationModeAsync<T>( string sql, object param = null )
      {
         using ( var connection = new SqlConnection( _connectionString ) )
         {
            connection.Open();
            using ( var transaction = connection.BeginTransaction( System.Data.IsolationLevel.Snapshot ) )
            {
               IEnumerable<T> returnValue = await connection.QueryAsync<T>( sql, param, transaction );

               transaction.Commit();

               return returnValue;
            }
         }
      }
   }
}