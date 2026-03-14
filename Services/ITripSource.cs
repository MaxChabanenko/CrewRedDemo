using CrewRedDemo.Models;

namespace CrewRedDemo.Services
{
    public interface ITripSource
    {
        IAsyncEnumerable<(SampleCabDatum Record, string RawCsvLine)> ReadAsync(string path);
    }
}