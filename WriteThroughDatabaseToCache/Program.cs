using System;
using System.Threading;
using System.Threading.Tasks;

namespace WriteThroughDatabaseToCache
{
   class Program
   {
      private const string _connectionString = "Server=localhost;Database=ChangeTrackingTest;Integrated Security=true;";

      private static void Main( string[] args )
      {
         MainAsync( args ).Wait();
      }

      private static async Task MainAsync( string[] args )
      {
         Setup.CreateSchemaAndEnableChangeTracking( _connectionString );

         var repository = new DataRepository( _connectionString );

         var cancellationTokenSource = new CancellationTokenSource();
         var changeTrackerTask = ChangeTracker.StartChangeTrackingMonitorLoopAsync( cancellationTokenSource.Token );

         await InputProcessor.StartAsync( repository );
         Console.WriteLine( "Shutting down..." );
         cancellationTokenSource.Cancel();
         await changeTrackerTask;
      }
   }
}
