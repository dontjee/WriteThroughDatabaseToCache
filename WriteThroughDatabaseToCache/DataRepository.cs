using System;
using System.Data.SqlClient;

namespace WriteThroughDatabaseToCache
{
   internal class DataRepository
   {
      private string _connectionString;

      public DataRepository( string connectionString )
      {
         _connectionString = connectionString;
      }

      public void CreateMedia()
      {
         using ( var command = new SqlCommand() )
         {
            command.CommandText = @"
INSERT INTO [dbo].[Media]
           ([ChannelId]
           ,[Title]
           ,[Width]
           ,[Height]
           ,[Duration]
           ,[ContentUrl])
     VALUES
           (( SELECT TOP 1 ChannelId FROM dbo.Channel ORDER BY ModifyDate DESC)
           ,@title
           ,10
           ,10
           ,60
           ,'https://upload.wikimedia.org/wikipedia/en/6/64/Movie_poster_Anchorman_The_Legend_of_Ron_Burgundy.jpg')
";
            command.Parameters.Add( new SqlParameter( "title", Guid.NewGuid().ToString() ) );
            RunCommandInDatabase( command );
         }
      }

      public void CreateChannel()
      {
         using ( var command = new SqlCommand() )
         {
            command.CommandText = @"
INSERT INTO [dbo].[Channel]
           ([Title]
           ,[UserAccountId]
           ,[ModifyDate])
     VALUES
           (@title
           ,( SELECT TOP 1 UserAccountId FROM dbo.UserAccount ORDER BY UserAccountId DESC)
           ,GETUTCDATE())
";
            command.Parameters.Add( new SqlParameter( "title", Guid.NewGuid().ToString() ) );
            RunCommandInDatabase( command );
         }
      }

      public void CreateUser()
      {
         using ( var command = new SqlCommand() )
         {
            command.CommandText = @"
INSERT INTO [dbo].[UserAccount]
           ([DisplayName]
           ,[Email]
           ,[CreateDate])
     VALUES
           (@displayName
           ,@email
           ,GETUTCDATE())
";
            command.Parameters.Add( new SqlParameter( "displayName", Guid.NewGuid().ToString().Substring( 0, 19 ) ) );
            command.Parameters.Add( new SqlParameter( "email", $"{Guid.NewGuid().ToString()}@email.test" ) );
            RunCommandInDatabase( command );
         }
      }

      private void RunCommandInDatabase( SqlCommand command )
      {
         using ( var connection = new SqlConnection( _connectionString ) )
         {
            connection.Open();
            command.Connection = connection;
            command.ExecuteNonQuery();
         }
      }
   }
}