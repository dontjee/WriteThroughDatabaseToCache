using System.Data.SqlClient;

namespace WriteThroughDatabaseToCache
{
   public static class Setup
   {
      public static void CreateSchemaAndEnableChangeTracking( string connectionString )
      {
         using ( var connection = new SqlConnection( connectionString ) )
         {
            connection.Open();
            using ( var createSchemaCommand = connection.CreateCommand() )
            {
               createSchemaCommand.CommandText = @"
IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'UserAccount'))
BEGIN
   CREATE TABLE [dbo].[UserAccount] (
       [UserAccountId]                  INT           IDENTITY (1, 1) NOT NULL,
       [DisplayName]                    NVARCHAR (20)    NULL,
       [Email]                          NVARCHAR (129)   NOT NULL,
       [CreateDate]                     DATETIME         NOT NULL
   );
   ALTER TABLE [dbo].[UserAccount]
       ADD CONSTRAINT [PK_UserAccount] PRIMARY KEY CLUSTERED ([UserAccountId] ASC) WITH (FILLFACTOR = 80, ALLOW_PAGE_LOCKS = OFF, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);

   CREATE TABLE [dbo].[Channel] (
       [ChannelId]                      INT           IDENTITY (1, 1) NOT NULL,
       [Title]                          NVARCHAR (128)   NOT NULL,
       [UserAccountId]                  INT           NOT NULL,
       [ModifyDate]                     DATETIME         NOT NULL
   );
   ALTER TABLE [dbo].[Channel]
       ADD CONSTRAINT [PK_Channel] PRIMARY KEY CLUSTERED ([ChannelId] ASC) WITH (FILLFACTOR = 80, ALLOW_PAGE_LOCKS = OFF, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);
   ALTER TABLE [dbo].[Channel]
       ADD CONSTRAINT [FK_Channel_UserAccountId_UserAccount_UserAccountId] FOREIGN KEY ([UserAccountId]) REFERENCES [dbo].[UserAccount] ([UserAccountId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

   CREATE TABLE [dbo].[Media] (
       [MediaId]                           INT           IDENTITY (1, 1) NOT NULL,
       [ChannelId]                         INT           NOT NULL,
       [Title]                             NVARCHAR (128)   NULL,
       [Width]                             INT              NOT NULL,
       [Height]                            INT              NOT NULL,
       [Duration]                          INT           NOT NULL,
       [ContentUrl]                        NVARCHAR (MAX)   NULL,
   );
   ALTER TABLE [dbo].[Media]
       ADD CONSTRAINT [PK_Media] PRIMARY KEY CLUSTERED ([MediaId] ASC) WITH (FILLFACTOR = 80, ALLOW_PAGE_LOCKS = OFF, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);
   ALTER TABLE [dbo].[Media]
       ADD CONSTRAINT [FK_Media_ChannelId_Channel_ChannelId] FOREIGN KEY ([ChannelId]) REFERENCES [dbo].[Channel] ([ChannelId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

   CREATE TABLE [dbo].[ChangeTrackingHistory] (
       [ChangeTrackingHistoryId]               INT   IDENTITY (1, 1) NOT NULL,
       [TableName]                             NVARCHAR (512)   NOT NULL,
      [LastSynchronizationVersion]            BIGINT   NOT NULL,
   );
   ALTER TABLE [dbo].[ChangeTrackingHistory]
       ADD CONSTRAINT [PK_ChangeTrackingHistory] PRIMARY KEY CLUSTERED ([ChangeTrackingHistoryId] ASC) WITH (FILLFACTOR = 80, ALLOW_PAGE_LOCKS = OFF, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);

   -- Add default values for last sync version
   INSERT INTO dbo.ChangeTrackingHistory( TableName, LastSynchronizationVersion )
   VALUES ('dbo.UserAccount', CHANGE_TRACKING_MIN_VALID_VERSION(Object_ID('dbo.UserAccount')))
   INSERT INTO dbo.ChangeTrackingHistory( TableName, LastSynchronizationVersion )
   VALUES ('dbo.Channel', CHANGE_TRACKING_MIN_VALID_VERSION(Object_ID('dbo.Channel')))
   INSERT INTO dbo.ChangeTrackingHistory( TableName, LastSynchronizationVersion )
   VALUES ('dbo.Media', CHANGE_TRACKING_MIN_VALID_VERSION(Object_ID('dbo.Media')))
END
";
               createSchemaCommand.ExecuteScalar();
            }

            using ( var enableChangeTrackingCommand = connection.CreateCommand() )
            {
               enableChangeTrackingCommand.CommandText = @"
IF ( ( SELECT snapshot_isolation_state_desc
     FROM sys.databases 
     WHERE NAME = '" + connection.Database + @"' ) = 'OFF' )
BEGIN
   ALTER DATABASE " + connection.Database + @"  
       SET ALLOW_SNAPSHOT_ISOLATION ON; 

   ALTER DATABASE " + connection.Database + @"
   SET CHANGE_TRACKING = ON  
   (CHANGE_RETENTION = 7 DAYS, AUTO_CLEANUP = ON)  

   ALTER TABLE dbo.UserAccount
   ENABLE CHANGE_TRACKING  
   WITH (TRACK_COLUMNS_UPDATED = ON)

   ALTER TABLE dbo.Channel
   ENABLE CHANGE_TRACKING  
   WITH (TRACK_COLUMNS_UPDATED = ON)

   ALTER TABLE dbo.Media
   ENABLE CHANGE_TRACKING  
   WITH (TRACK_COLUMNS_UPDATED = ON)
END
";
               enableChangeTrackingCommand.ExecuteScalar();
            }
         }
      }
   }
}
