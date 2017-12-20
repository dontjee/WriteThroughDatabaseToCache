namespace WriteThroughDatabaseToCache
{
   internal class ChangeTrackingRepository
   {
      private string _connectionString;

      public ChangeTrackingRepository( string connectionString )
      {
         _connectionString = connectionString;
      }

      internal ChangeTrackingBatch<UserAccountChangeModel> GetLatestUserChanges()
      {
         string cmd = @"
DECLARE @last_synchronization_version BIGINT = (SELECT LastSynchronizationVersion FROM dbo.CacheChangeTrackingHistory WHERE TableName = 'dbo.UserAccount')

DECLARE @current_synchronization_version BIGINT = CHANGE_TRACKING_CURRENT_VERSION(); 
SELECT ct.UserAccountId, ua.Email, ua.DisplayName, ua.CreateDate
		, CASE WHEN ct.SYS_CHANGE_OPERATION = 'I' THEN 'Insert' WHEN ct.SYS_CHANGE_OPERATION = 'U' THEN 'Update' ELSE 'Delete' END AS OperationType
FROM dbo.UserAccount AS ua
	RIGHT OUTER JOIN CHANGETABLE(CHANGES dbo.UserAccount, @last_synchronization_version) AS ct ON ua.UserAccountId = ct.UserAccountId

UPDATE dbo.CacheChangeTrackingHistory
SET LastSynchronizationVersion = @current_synchronization_version
WHERE TableName = 'dbo.UserAccount'
";
         return new ChangeTrackingBatch<UserAccountChangeModel>( _connectionString, cmd );
      }

      internal ChangeTrackingBatch<ChannelChangeModel> GetLatestChannelChanges()
      {
         string cmd = @"
DECLARE @last_synchronization_version BIGINT = (SELECT LastSynchronizationVersion FROM dbo.CacheChangeTrackingHistory WHERE TableName = 'dbo.Channel')

DECLARE @current_synchronization_version BIGINT = CHANGE_TRACKING_CURRENT_VERSION(); 
SELECT  
    ct.ChannelId, c.Title, c.UserAccountId, c.ModifyDate
	, CASE WHEN ct.SYS_CHANGE_OPERATION = 'I' THEN 'Insert' WHEN ct.SYS_CHANGE_OPERATION = 'U' THEN 'Update' ELSE 'Delete' END AS OperationType
FROM  dbo.Channel AS c
	RIGHT OUTER JOIN CHANGETABLE(CHANGES dbo.Channel, @last_synchronization_version) AS ct ON c.ChannelId = ct.ChannelId

UPDATE dbo.CacheChangeTrackingHistory
SET LastSynchronizationVersion = @current_synchronization_version
WHERE TableName = 'dbo.Channel'
";
         return new ChangeTrackingBatch<ChannelChangeModel>( _connectionString, cmd );
      }

      internal ChangeTrackingBatch<MediaChangeModel> GetLatestMediaChanges()
      {
         string cmd = @"
DECLARE @last_synchronization_version BIGINT = (SELECT LastSynchronizationVersion FROM dbo.CacheChangeTrackingHistory WHERE TableName = 'dbo.Media')

DECLARE @current_synchronization_version BIGINT = CHANGE_TRACKING_CURRENT_VERSION(); 
SELECT  
    ct.MediaId, m.ChannelId, m.Title, m.Height, m.Width, m.Duration, m.ContentUrl
	, CASE WHEN ct.SYS_CHANGE_OPERATION = 'I' THEN 'Insert' WHEN ct.SYS_CHANGE_OPERATION = 'U' THEN 'Update' ELSE 'Delete' END AS OperationType
FROM  dbo.Media AS m
	RIGHT OUTER JOIN CHANGETABLE(CHANGES dbo.Media, @last_synchronization_version) AS ct ON m.MediaId = ct.MediaId

UPDATE dbo.CacheChangeTrackingHistory
SET LastSynchronizationVersion = @current_synchronization_version
WHERE TableName = 'dbo.Media'
";
         return new ChangeTrackingBatch<MediaChangeModel>( _connectionString, cmd );
      }
   }
}