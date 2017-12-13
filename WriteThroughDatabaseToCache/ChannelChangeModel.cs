using System;

namespace WriteThroughDatabaseToCache
{
   public class ChannelChangeModel
   {
      public int ChannelId
      {
         get; set;
      }

      public int? UserAccountId
      {
         get; set;
      }

      public string Title
      {
         get; set;
      }

      public DateTime? ModifyDate
      {
         get; set;
      }

      public string OperationType
      {
         get; set;
      }
   }
}