using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace StressApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StressController : ControllerBase
    {
        // Upper limits to avoid accidentally killing the host.
        private const long MaxIterations = 1_000_000_000;
        private const int MaxMemoryMb = 2048;
        private const int MaxTasks = 1_000;
        private const int MaxSeconds = 300;
        private const int MaxDelayMs = 60_000;

        [HttpGet("version")]
        public IActionResult GetVersion() => Ok("v.1.1.0");

        // 1. CPU load
        [HttpGet("cpu")]
        public IActionResult CpuStress([FromQuery] long iterations = 10_000_000)
        {
            if (!InRange(iterations, 1, MaxIterations, out var error)) return error;

            var sw = Stopwatch.StartNew();
            var checksum = SqrtSum(iterations);
            sw.Stop();

            return Ok(new { message = "CPU Load Test", iterations, durationMs = sw.ElapsedMilliseconds, checksum, metrics = Metrics() });
        }

        // 2. Memory usage
        [HttpGet("memory")]
        public IActionResult MemoryStress([FromQuery] int sizeMb = 100)
        {
            if (!InRange(sizeMb, 1, MaxMemoryMb, out var error)) return error;

            var sw = Stopwatch.StartNew();
            var buffer = Allocate(sizeMb);
            sw.Stop();

            var response = new { message = "Memory allocated", sizeMB = buffer.Length / 1024 / 1024, durationMs = sw.ElapsedMilliseconds, metrics = Metrics() };
            GC.KeepAlive(buffer);
            return Ok(response);
        }

        // 3. Request flood (many short tasks in parallel)
        [HttpGet("request")]
        public async Task<IActionResult> RequestFlood([FromQuery] int tasks = 50, [FromQuery] long iterations = 100)
        {
            if (!InRange(tasks, 1, MaxTasks, out var error)) return error;
            if (!InRange(iterations, 1, MaxIterations, out error)) return error;

            var sw = Stopwatch.StartNew();
            var results = await Task.WhenAll(Enumerable.Range(0, tasks).Select(_ => Task.Run(() => SqrtSum(iterations))));
            sw.Stop();

            return Ok(new { message = "Multiple requests executed", tasks, durationMs = sw.ElapsedMilliseconds, checksum = results.Sum(), metrics = Metrics() });
        }

        // 4. Latency test (optional artificial delay to simulate a slow dependency)
        [HttpGet("latency")]
        public async Task<IActionResult> LatencyTest([FromQuery] int delayMs = 0)
        {
            if (!InRange(delayMs, 0, MaxDelayMs, out var error)) return error;

            var sw = Stopwatch.StartNew();
            if (delayMs > 0) await Task.Delay(delayMs);
            var checksum = SqrtSum(1_000_000);
            sw.Stop();

            return Ok(new { message = "Latency test completed", delayMs, durationMs = sw.ElapsedMilliseconds, checksum });
        }

        // 5. Force garbage collection
        [HttpGet("gc")]
        public IActionResult ForceGC()
        {
            var before = Metrics();
            var sw = Stopwatch.StartNew();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            sw.Stop();

            return Ok(new { message = "Garbage collection triggered", durationMs = sw.ElapsedMilliseconds, before, after = Metrics() });
        }

        // 6. Multithreading load
        [HttpGet("thread")]
        public async Task<IActionResult> MultiThreadLoad([FromQuery] int count = 20, [FromQuery] long iterations = 100_000)
        {
            if (!InRange(count, 1, MaxTasks, out var error)) return error;
            if (!InRange(iterations, 1, MaxIterations, out error)) return error;

            var sw = Stopwatch.StartNew();
            var threads = Enumerable.Range(0, count)
                .Select(_ => Task.Factory.StartNew(() => SqrtSum(iterations), TaskCreationOptions.LongRunning));
            var results = await Task.WhenAll(threads);
            sw.Stop();

            return Ok(new { message = "Multithread load executed", count, durationMs = sw.ElapsedMilliseconds, checksum = results.Sum(), metrics = Metrics() });
        }

        // 7. Sustained CPU load
        [HttpGet("cpu-intensity")]
        public IActionResult CpuLongLoad([FromQuery] int seconds = 10)
        {
            if (!InRange(seconds, 1, MaxSeconds, out var error)) return error;

            var sw = Stopwatch.StartNew();
            var limit = TimeSpan.FromSeconds(seconds);
            double checksum = 0;
            while (sw.Elapsed < limit)
            {
                checksum += Math.Log(sw.ElapsedTicks + 1);
            }
            sw.Stop();

            return Ok(new { message = "Long CPU load completed", seconds, durationMs = sw.ElapsedMilliseconds, checksum, metrics = Metrics() });
        }

        // 8. Combined load (delay + CPU + memory)
        [HttpGet("load")]
        public async Task<IActionResult> CombinedLoad([FromQuery] int delayMs = 500, [FromQuery] long iterations = 1_000_000, [FromQuery] int sizeMb = 10)
        {
            if (!InRange(delayMs, 0, MaxDelayMs, out var error)) return error;
            if (!InRange(iterations, 1, MaxIterations, out error)) return error;
            if (!InRange(sizeMb, 0, MaxMemoryMb, out error)) return error;

            var total = Stopwatch.StartNew();
            if (delayMs > 0) await Task.Delay(delayMs);

            var cpuSw = Stopwatch.StartNew();
            var checksum = SqrtSum(iterations);
            cpuSw.Stop();

            var buffer = Allocate(sizeMb);
            total.Stop();

            var response = new
            {
                message = "Combined load executed",
                delayMs,
                cpuDurationMs = cpuSw.ElapsedMilliseconds,
                memSizeMB = buffer.Length / 1024 / 1024,
                totalDurationMs = total.ElapsedMilliseconds,
                checksum,
                metrics = Metrics()
            };
            GC.KeepAlive(buffer);
            return Ok(response);
        }

        // The result is returned so the JIT cannot optimize the loop away.
        private static double SqrtSum(long iterations)
        {
            double sum = 0;
            for (long i = 0; i < iterations; i++)
                sum += Math.Sqrt(i);
            return sum;
        }

        // Touches every byte so the memory is actually committed.
        private static byte[] Allocate(int sizeMb)
        {
            var buffer = new byte[sizeMb * 1024 * 1024];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(i % 256);
            return buffer;
        }

        private static object Metrics()
        {
            using var process = Process.GetCurrentProcess();
            return new
            {
                managedMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
                workingSetMB = process.WorkingSet64 / 1024 / 1024,
                threads = process.Threads.Count,
                gen0Collections = GC.CollectionCount(0),
                gen1Collections = GC.CollectionCount(1),
                gen2Collections = GC.CollectionCount(2)
            };
        }

        private bool InRange(long value, long min, long max, out IActionResult error)
        {
            error = BadRequest(new { message = $"Value must be between {min} and {max}.", value });
            return value >= min && value <= max;
        }
    }
}
