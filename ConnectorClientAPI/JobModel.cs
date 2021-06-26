using System;

namespace ConnectorClientAPI
{
    public class JobModel
    {
        public object User { get; set; }
        public int PaxWeight { get; set; }
        public int Id { get; set; }
        public string DepartureICAO { get; set; }
        public string ArrivalICAO { get; set; }
        public string DepartureDesc { get; set; }
        public string ArrivalDesc { get; set; }
        public string AlternativeICAO { get; set; }
        public long Dist { get; set; }
        public long Pax { get; set; }
        public long Cargo { get; set; }
        public long Payload { get; set; }
        public long PayloadDisplay { get; set; }
        public string FlightTime { get; set; }
        public int FlightTimeHours { get; set; }
        public long Pay { get; set; }
        public bool FirstClass { get; set; }
        public bool IsDone { get; set; }
        public bool IsActivated { get; set; }
        public bool InProgress { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ModelName { get; set; }
        public string ModelDescription { get; set; }
        public long StartFuelWeight { get; set; }
        public long StartFuelWeightDisplay { get; set; }
        public long FinishFuelWeight { get; set; }
        public int AviationType { get; set; }
        public long UsedFuelWeight { get; set; }
        public long UsedFuelWeightDisplay { get; set; }
        public string Month { get; set; }
        public string VideoUrl { get; set; }
        public string VideoDescription { get; set; }
        public string ChallengeCreatorUserId { get; set; }
        public bool IsChallenge { get; set; }
        public bool IsChallengeFromCurrentUser { get; set; }
        public string ChallengeExpirationDate { get; set; }
        public int ChallengeType { get; set; }
        public string WeightUnit { get; set; }
    }
}