namespace CrewRedDemo.Models;

public partial class SampleCabDatum
{
    public long Id { get; set; }

    public DateTime TpepPickupDatetime { get; set; }

    public DateTime TpepDropoffDatetime { get; set; }

    public int PassengerCount { get; set; }

    public double TripDistance { get; set; }

    public string StoreAndFwdFlag { get; set; } = null!;

    public int PuLocationId { get; set; }

    public int DoLocationId { get; set; }

    public double FareAmount { get; set; }

    public double TipAmount { get; set; }

    public int? TripDurationSeconds { get; set; }
}
