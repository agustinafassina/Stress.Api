using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System;
using System.IO;
using StressApi.Services.Interfaces;

namespace TemplateApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StressController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IMapper _mapper;

        public StressController(IItemService itemService, IMapper mapper)
        {
            _itemService = itemService;
            _mapper = mapper;
        }

        [HttpGet("version")]
        public async Task<IActionResult> GetVersion()
        {
            return Ok("v.1.0.0");
        }

        // 1. CPU load
        [HttpGet("cpu")]
        public IActionResult CpuStress()
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10_000_000; i++)
            {
                Math.Sqrt(i);
            }
            sw.Stop();
            return Ok(new { message = "CPU Load Test", durationMs = sw.ElapsedMilliseconds });
        }

        // 2. Memory usage
        [HttpGet("memory")]
        public IActionResult MemoryStress()
        {
            var buffer = new byte[100_000_000]; // 100MB
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(i % 256);
            return Ok(new { message = "Memory allocated", sizeMB = buffer.Length / 1024 / 1024 });
        }

        // 3. Request flood
        [HttpGet("request")]
        public IActionResult RequestFlood()
        {
            var tasks = Enumerable.Range(0, 50).Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                    Math.Sqrt(i);
            }));
            Task.WaitAll(tasks.ToArray());
            return Ok(new { message = "Multiple requests executed" });
        }

        // 4. Latency test
        [HttpGet("latency")]
        public IActionResult LatencyTest()
        {
            var sw = Stopwatch.StartNew();
            // Simula carga
            for (int i = 0; i < 1_000_000; i++) { }
            sw.Stop();
            return Ok(new { message = "Latency test completed", durationMs = sw.ElapsedMilliseconds });
        }

        // 5. Force garbage collection
        [HttpGet("gc")]
        public IActionResult ForceGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return Ok(new { message = "Garbage collection triggered" });
        }

        // 6. Multithreading load
        [HttpGet("thread")]
        public IActionResult MultiThreadLoad()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < 100_000; j++)
                    {
                        Math.Sqrt(j);
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
            return Ok(new { message = "Multithread load executed" });
        }

        // 7. CPU intensity prolongada
        [HttpGet("cpu-intensity")]
        public IActionResult CpuLongLoad()
        {
            var sw = Stopwatch.StartNew();
            var endTime = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < endTime)
            {
                Math.Log(DateTime.UtcNow.Ticks);
            }
            sw.Stop();
            return Ok(new { message = "Long CPU load completed", durationMs = sw.ElapsedMilliseconds });
        }

        // 8. Load combinada
        [HttpGet("load")]
        public IActionResult CombinedLoad()
        {
            // Combining CPU, memoria y requests
            Task.Delay(500).Wait();
            var cpuSw = Stopwatch.StartNew();
            for (int i = 0; i < 1_000_000; i++) Math.Sqrt(i);
            cpuSw.Stop();
            return Ok(new
            {
                message = "Combined load executed",
                cpuDurationMs = cpuSw.ElapsedMilliseconds,
                memSizeMB = 10
            });
        }
    }
}