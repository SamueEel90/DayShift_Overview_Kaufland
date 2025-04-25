using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DayShift_Overview_Kaufland.Models
{
   public class Date
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public DateTime ShiftDate { get; set; }

        public bool IsClosed { get; set; }
    }
}
