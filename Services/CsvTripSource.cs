using CrewRedDemo.Models;
using CsvHelper;
using System.Globalization;

namespace CrewRedDemo.Services
{
    public class CsvTripSource : ITripSource
    {
        private static readonly TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        public async IAsyncEnumerable<(SampleCabDatum Record, string RawCsvLine)> ReadAsync(string path)
        {
            if (!File.Exists(path))
            {
                yield break;
            }

            // Assume your program will be used on much larger data files.
            // Describe in a few sentences what you would change if you knew it would be used for a 10GB CSV input file.

            //Firstly, I would definetely change the process of finding duplicates.
            //The current implementation uses a HashSet to track seen records, which can consume a lot of memory for large datasets.
            //For a 10GB I could use Linq with IQueryable to extract duplicates on db side. Or maybe just prepare data before execution.
            //Secondly, I would consider using multhithreading.
            //In the end I could just make a release version, explore it's usage of time in space in VS Performance Profiler (or tests) and optimize the bottlenecks.

            using StreamReader reader = new StreamReader(path);
            using CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                string raw = csv.Context.Parser?.RawRecord ?? string.Empty;
                string pickupText = csv.GetField("tpep_pickup_datetime")?.Trim() ?? string.Empty;
                string dropoffText = csv.GetField("tpep_dropoff_datetime")?.Trim() ?? string.Empty;
                string passengerText = csv.GetField("passenger_count")?.Trim() ?? string.Empty;
                string tripDistanceText = csv.GetField("trip_distance")?.Trim() ?? string.Empty;
                string flagText = csv.GetField("store_and_fwd_flag")?.Trim() ?? string.Empty;
                string pickupLocationText = csv.GetField("PULocationID")?.Trim() ?? string.Empty;
                string dropoffLocationText = csv.GetField("DOLocationID")?.Trim() ?? string.Empty;
                string fareText = csv.GetField("fare_amount")?.Trim() ?? string.Empty;
                string tipText = csv.GetField("tip_amount")?.Trim() ?? string.Empty;

                if (!DateTime.TryParse(pickupText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var pickupLocal))
                {
                    continue;
                }

                if (!DateTime.TryParse(dropoffText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dropoffLocal))
                {
                    continue;
                }

                // The input data is in the EST timezone. Convert it to UTC when inserting into the DB.
                DateTime pickupUtc = TimeZoneInfo.ConvertTimeToUtc(pickupLocal, easternZone);
                DateTime dropoffUtc = TimeZoneInfo.ConvertTimeToUtc(dropoffLocal, easternZone);

                if (!int.TryParse(passengerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var passenger))
                {
                    continue;
                }

                if (!double.TryParse(tripDistanceText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var tripDistance))
                {
                    tripDistance = 0.0;
                }

                if (!int.TryParse(pickupLocationText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pickupLocation))
                {
                    pickupLocation = 0;
                }

                if (!int.TryParse(dropoffLocationText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dropoffLocation))
                {
                    dropoffLocation = 0;
                }

                if (!double.TryParse(fareText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var fare))
                {
                    fare = 0.0;
                }

                if (!double.TryParse(tipText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var tip))
                {
                    tip = 0.0;
                }

                //For the `store_and_fwd_flag` column, convert any 'N' values to 'No' and any 'Y' values to 'Yes'.
                string flag = flagText switch
                {
                    "Y" => "Yes",
                    "N" => "No",
                    string s => s
                };

                SampleCabDatum rec = new SampleCabDatum
                {
                    TpepPickupDatetime = pickupUtc,
                    TpepDropoffDatetime = dropoffUtc,
                    PassengerCount = passenger,
                    TripDistance = tripDistance,
                    StoreAndFwdFlag = flag,
                    PuLocationId = pickupLocation,
                    DoLocationId = dropoffLocation,
                    FareAmount = fare,
                    TipAmount = tip
                };

                yield return (rec, raw);
            }
        }
    }
}