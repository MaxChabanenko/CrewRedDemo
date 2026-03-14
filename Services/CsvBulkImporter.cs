using CrewRedDemo.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CrewRedDemo.Services
{
    public record TripDuplicateKey(DateTime Pickup, DateTime Dropoff, int PassengerCount);

    public class CsvBulkImporter
    {
        private readonly string _connectionString;
        private readonly ITripSource _source;
        private readonly int _batchSize;
        private readonly string _destinationTableName;
        private readonly string _duplicatesPath;

        public CsvBulkImporter(string connectionString, ITripSource source, int batchSize, string destinationTableName, string duplicatesPath)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _batchSize = batchSize > 0 ? batchSize : 5000;
            _destinationTableName = destinationTableName ?? throw new ArgumentNullException(nameof(destinationTableName));
            _duplicatesPath = duplicatesPath ?? throw new ArgumentNullException(nameof(duplicatesPath));
        }

        public async Task ImportAsync(string csvPath)
        {
            HashSet<TripDuplicateKey> seen = new HashSet<TripDuplicateKey>();
            List<SampleCabDatum> batch = new List<SampleCabDatum>(_batchSize);

            await using StreamWriter duplicatesWriter = new StreamWriter(_duplicatesPath, append: false);
            await ClearTableAsync();

            await foreach ((SampleCabDatum record, string raw) in _source.ReadAsync(csvPath))
            {
                TripDuplicateKey key = new TripDuplicateKey(record.TpepPickupDatetime, record.TpepDropoffDatetime, record.PassengerCount);

                if (!seen.Add(key))
                {
                    await duplicatesWriter.WriteAsync(raw);
                    continue;
                }

                batch.Add(record);

                if (batch.Count >= _batchSize)
                {
                    await BulkInsertAsync(batch);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await BulkInsertAsync(batch);
                batch.Clear();
            }

            await duplicatesWriter.FlushAsync();
        }

        #region private methods
        private async Task BulkInsertAsync(List<SampleCabDatum> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                return;
            }

            DataTable table = new DataTable();

            table.Columns.Add("TpepPickupDatetime", typeof(DateTime));
            table.Columns.Add("TpepDropoffDatetime", typeof(DateTime));
            table.Columns.Add("PassengerCount", typeof(int));
            table.Columns.Add("TripDistance", typeof(double));
            table.Columns.Add("StoreAndFwdFlag", typeof(string));
            table.Columns.Add("PuLocationId", typeof(int));
            table.Columns.Add("DoLocationId", typeof(int));
            table.Columns.Add("FareAmount", typeof(double));
            table.Columns.Add("TipAmount", typeof(double));

            foreach (SampleCabDatum r in rows)
            {
                table.Rows.Add(
                    r.TpepPickupDatetime,
                    r.TpepDropoffDatetime,
                    r.PassengerCount,
                    r.TripDistance,
                    r.StoreAndFwdFlag?.Trim(),
                    r.PuLocationId,
                    r.DoLocationId,
                    r.FareAmount,
                    r.TipAmount
                );
            }

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = _destinationTableName,
                BatchSize = _batchSize
            };

            bulkCopy.ColumnMappings.Add("TpepPickupDatetime", "TpepPickupDatetime");
            bulkCopy.ColumnMappings.Add("TpepDropoffDatetime", "TpepDropoffDatetime");
            bulkCopy.ColumnMappings.Add("PassengerCount", "PassengerCount");
            bulkCopy.ColumnMappings.Add("TripDistance", "TripDistance");
            bulkCopy.ColumnMappings.Add("StoreAndFwdFlag", "StoreAndFwdFlag");
            bulkCopy.ColumnMappings.Add("PuLocationId", "PuLocationId");
            bulkCopy.ColumnMappings.Add("DoLocationId", "DoLocationId");
            bulkCopy.ColumnMappings.Add("FareAmount", "FareAmount");
            bulkCopy.ColumnMappings.Add("TipAmount", "TipAmount");

            await bulkCopy.WriteToServerAsync(table);
        }

        //Not stated in task, but I assume we dont need to append same rows every time
        private async Task ClearTableAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand($"TRUNCATE TABLE {_destinationTableName}", connection);
            await command.ExecuteNonQueryAsync();
        }
        #endregion
    }
}
