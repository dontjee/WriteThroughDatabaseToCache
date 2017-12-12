using System;
using System.Threading.Tasks;

namespace WriteThroughDatabaseToCache
{
   internal static class InputProcessor
   {
      internal static Task StartAsync( DataRepository repository )
      {
         return Task.Run( () => Start( repository ) );
      }

      private static void Start( DataRepository repository )
      {
         Console.WriteLine( "Press 'u' to create a new user, 'c' to create a new channel for a random users, or 'm' to create media in a random channel" );
         string line;
         while ( !string.IsNullOrWhiteSpace( line = Console.ReadLine() ) )
         {
            switch ( line[0] )
            {
               case 'u':
               Console.WriteLine( "Creating User" );
               repository.CreateUser();
               break;
               case 'c':
               Console.WriteLine( "Creating Channel" );
               repository.CreateChannel();
               break;
               case 'm':
               Console.WriteLine( "Creating Media" );
               repository.CreateMedia();
               break;
               default:
               Console.WriteLine( $"Cannot process character {line[0]}" );
               break;
            }
            Console.WriteLine();
         }
      }
   }
}