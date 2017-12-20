using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WriteThroughDatabaseToCache
{
   public static class ChangeTracker
   {
      internal static async Task StartChangeTrackingMonitorLoopAsync( ChangeTrackingRepository changeTrackingRepository, CancellationToken token )
      {
         while ( true )
         {
            if ( token.IsCancellationRequested )
            {
               break;
            }

            using ( ChangeTrackingBatch<UserAccountChangeModel> userAccountChangesBatch = changeTrackingRepository.GetLatestUserChanges() )
            {
               UserAccountChangeModel[] userAccountChanges = ( await userAccountChangesBatch.GetItemsAsync() ).ToArray();
               if ( userAccountChanges.Length > 0 )
               {
                  Console.Write( $"\n\nFound {userAccountChanges.Length} User Account Changes: {JsonConvert.SerializeObject( userAccountChanges, Formatting.Indented)}\n\n" );
               }
               userAccountChangesBatch.Commit();
            }
            using ( ChangeTrackingBatch<ChannelChangeModel> channelChangesBatch = changeTrackingRepository.GetLatestChannelChanges() )
            {
               ChannelChangeModel[] channelChanges = ( await channelChangesBatch.GetItemsAsync() ).ToArray();
               if ( channelChanges.Length > 0 )
               {
                  Console.Write( $"\n\nFound {channelChanges.Length} Channel Changes: {JsonConvert.SerializeObject( channelChanges, Formatting.Indented)}\n\n" );
               }
               channelChangesBatch.Commit();
            }
            using ( ChangeTrackingBatch<MediaChangeModel> mediaChangesBatch = changeTrackingRepository.GetLatestMediaChanges() )
            {
               MediaChangeModel[] mediaChanges = ( await mediaChangesBatch.GetItemsAsync() ).ToArray();
               if ( mediaChanges.Length > 0 )
               {
                  Console.Write( $"\n\nFound {mediaChanges.Length} Media Changes: {JsonConvert.SerializeObject( mediaChanges, Formatting.Indented)}\n\n" );
               }
               mediaChangesBatch.Commit();
            }

            await Task.Delay( 1000 );
         }
      }
   }
}