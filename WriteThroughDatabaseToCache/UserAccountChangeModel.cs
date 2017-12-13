using System;

namespace WriteThroughDatabaseToCache
{
   public class UserAccountChangeModel
   {
      public int UserAccountId
      {
         get; set;
      }

      public string DisplayName
      {
         get; set;
      }

      public string Email
      {
         get; set;
      }

      public DateTime? CreateDate
      {
         get; set;
      }

      public string OperationType
      {
         get; set;
      }
   }
}