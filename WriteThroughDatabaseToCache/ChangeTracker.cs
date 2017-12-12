using System.Threading;
using System.Threading.Tasks;

namespace WriteThroughDatabaseToCache
{
   public static class ChangeTracker
   {
      public static async Task StartChangeTrackingMonitorLoopAsync( CancellationToken token )
      {
         while ( true )
         {
            if ( token.IsCancellationRequested )
            {
               break;
            }

            await Task.Delay( 5000 );
         }
      }
   }
}