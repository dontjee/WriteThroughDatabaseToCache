namespace WriteThroughDatabaseToCache
{
   public class MediaChangeModel
   {
      public int MediaId
      {
         get; set;
      }

      public int? ChannelId
      {
         get; set;
      }

      public string Title
      {
         get; set;
      }

      public int? Height
      {
         get; set;
      }

      public int? Width
      {
         get; set;
      }

      public int? Duration
      {
         get; set;
      }

      public string ContentUrl
      {
         get; set;
      }

      public string OperationType
      {
         get; set;
      }
   }
}