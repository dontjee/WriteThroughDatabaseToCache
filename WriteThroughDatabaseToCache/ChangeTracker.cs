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

            Task<IEnumerable<UserAccountChangeModel>> userAccountChangesTask = changeTrackingRepository.GetLatestUserChangesAsync();
            Task<IEnumerable<ChannelChangeModel>> channelChangesTask = changeTrackingRepository.GetLatestChannelChangesAsync();
            Task<IEnumerable<MediaChangeModel>> mediaChangesTask = changeTrackingRepository.GetLatestMediaChangesAsync();

            await Task.WhenAll( userAccountChangesTask, channelChangesTask, mediaChangesTask );

            UserAccountChangeModel[] userAccountChanges = userAccountChangesTask.Result.ToArray();
            ChannelChangeModel[] channelChanges = channelChangesTask.Result.ToArray();
            MediaChangeModel[] mediaChanges = mediaChangesTask.Result.ToArray();

            if ( userAccountChanges.Length > 0 )
            {
               Console.Write( $"\n\nFound {userAccountChanges.Length} User Account Changes: {JsonConvert.SerializeObject( userAccountChanges, Formatting.Indented)}\n\n" );
            }

            if ( channelChanges.Length > 0 )
            {
               Console.Write( $"\n\nFound {channelChanges.Length} Channel Changes: {JsonConvert.SerializeObject( channelChanges, Formatting.Indented)}\n\n" );
            }

            if ( mediaChanges.Length > 0 )
            {
               Console.Write( $"\n\nFound {mediaChanges.Length} Media Changes: {JsonConvert.SerializeObject( mediaChanges, Formatting.Indented)}\n\n" );
            }

            await Task.Delay( 5000 );
         }
      }
   }
}