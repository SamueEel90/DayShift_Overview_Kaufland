using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DayShift_Overview_Kaufland.Models
{
    public class Shift
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public DateTime ShiftDate { get; set; }
        public DateTime ShiftStart { get; set; }
        public DateTime ShiftEnd { get; set; }
        public string? Position { get; set; }
        public bool ArrivalConfirmed { get; set; }
        public bool DepartureConfirmed { get; set; }
    }
}