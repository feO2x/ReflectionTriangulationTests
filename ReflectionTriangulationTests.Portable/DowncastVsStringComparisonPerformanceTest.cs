using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ReflectionTriangulationTests.Portable
{
    public class DowncastVsStringComparisonPerformanceTest
    {
        private readonly ITestOutputHelper _output;

        public DowncastVsStringComparisonPerformanceTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void NameofReturnsOnlySimpleTypeName()
        {
            nameof(ConcreteType).Should().Be(typeof(ConcreteType).Name);
        }

        [Fact(DisplayName = "Downcasts are faster than string comparisons")]
        public void DowncastVsStringComparison()
        {
            Abstraction instance = new ConcreteType();

            var stringComparisonDuration = MeasureStringComparisonPerformance(instance);
            var downcastDuration = MeasureDowncastPerformance(instance);

            _output.WriteLine($"Downcast duration:          {downcastDuration.TotalMilliseconds:N}ms");
            _output.WriteLine($"String comparison duration: {stringComparisonDuration.TotalMilliseconds}ms");
            stringComparisonDuration.Should().BeGreaterThan(downcastDuration);
        }

        private TimeSpan MeasureStringComparisonPerformance(Abstraction instance)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (long i = 0; i < 100000000; ++i)
            {
                var foo = instance.TypeName == nameof(ConcreteType);
            }
            stopwatch.Stop();

            return stopwatch.Elapsed;
        }

        private static TimeSpan MeasureDowncastPerformance(Abstraction instance)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (long i = 0; i < 100000000; ++i)
            {
                var foo = instance is ConcreteType;
            }
            stopwatch.Stop();

            return stopwatch.Elapsed;
        }

        public abstract class Abstraction
        {
            public readonly string TypeName;
            protected Abstraction()
            {
                TypeName = GetType().Name;
            }
        }

        public class ConcreteType : Abstraction { }
    }
}
