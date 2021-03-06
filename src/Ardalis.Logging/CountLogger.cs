﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ardalis.Logging
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class CountLogger
    {
        private static readonly Dictionary<string, MethodStats> _countLog = new Dictionary<string, MethodStats>();

        public static void AddDuration(long duration,
            [CallerMemberName] string methodName = "Unknown Method")
        {
            if (!_countLog.ContainsKey(methodName))
            {
                var stats = new MethodStats(methodName);
                stats.AddCall(duration);
                _countLog.Add(methodName, stats);
            }
            else
            {
                var stats = _countLog[methodName];
                stats.AddCall(duration);
            }
        }

        public static void AddDuration(TimeSpan duration,
            [CallerMemberName] string methodName = "Unknown Method")
        {
            AddDuration((long)duration.TotalMilliseconds, methodName);
        }

        private class MethodStats
        {
            public MethodStats(string methodName)
            {
                MethodName = methodName;
            }
            public string MethodName { get; }
            public long CallCount { get; set; }
            public long DurationTotal { get; set; }

            public void AddCall(long duration)
            {
                CallCount++;
                DurationTotal += duration;
            }

            public override string ToString()
            {
                return $"{MethodName} duration {DurationTotal} ms, Count {CallCount}";
            }
        }

        public static void DumpResults(Action<string> outputAction)
        {

            foreach (var keyvaluepair in _countLog.OrderBy(c => c.Key))
            {
                var stats = keyvaluepair.Value;
                outputAction(stats.ToString());
            }
        }

        public class DisposableStopwatch : IDisposable
        {
            private readonly Stopwatch _sw;
            private readonly string _methodName;
            private readonly Action<TimeSpan, string> f;

            public DisposableStopwatch(Action<TimeSpan, string> f, 
                [CallerMemberName] string methodName = "Unkown Method")
            {
                _methodName = methodName;
                this.f = f;
                _sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _sw.Stop();
                f(_sw.Elapsed, _methodName);
            }
        }
    }
}
