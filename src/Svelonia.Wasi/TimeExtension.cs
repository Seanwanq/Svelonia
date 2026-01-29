using System;
using Svelonia.Wasi;
using Wasmtime;

namespace Svelonia.Wasi
{
    public class TimeInfo
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public int Millisecond { get; set; }
    }

    [WasiModule("time")]
    public partial class TimeExtension : IWasiExtension
    {
        public string Namespace => "time";

        public void Register(Linker linker, Store store)
        {
            RegisterGenerated(linker, store);
        }

        [WasiFunction("now")]
        public TimeInfo Now(string unused)
        {
            var dt = DateTime.Now;
            return new TimeInfo
            {
                Year = dt.Year,
                Month = dt.Month,
                Day = dt.Day,
                Hour = dt.Hour,
                Minute = dt.Minute,
                Second = dt.Second,
                Millisecond = dt.Millisecond
            };
        }
    }
}
