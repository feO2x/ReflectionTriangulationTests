using System;
using System.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ReflectionTriangulationTests.Portable
{
    public sealed class SubtypeCheckingPerformance
    {
        private readonly ITestOutputHelper _output;

        public SubtypeCheckingPerformance(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void NameofReturnsOnlySimpleTypeName()
        {
            nameof(ConcreteType).Should().Be(typeof(ConcreteType).Name);
        }

        [Fact(DisplayName = "Downcasts are faster than string comparisons.")]
        public void DowncastVsStringComparison()
        {
            Abstraction instance = new ConcreteType();

            var stringComparisonDuration = MeasureStringComparisonPerformance(instance);
            var downcastDuration = MeasureIsOperatorPerformance(instance);

            _output.WriteLine($"Downcast duration:          {downcastDuration.TotalMilliseconds:N}ms");
            _output.WriteLine($"String comparison duration: {stringComparisonDuration.TotalMilliseconds:N}ms");
            stringComparisonDuration.Should().BeGreaterThan(downcastDuration);
        }

        private TimeSpan MeasureStringComparisonPerformance(Abstraction instance)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (long i = 0; i < 100000000; ++i)
            {
                // ReSharper disable once UnusedVariable
                var foo = instance.TypeName == nameof(ConcreteType);
            }
            stopwatch.Stop();

            return stopwatch.Elapsed;
        }

        private static TimeSpan MeasureIsOperatorPerformance(Abstraction instance)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (long i = 0; i < 100000000; ++i)
            {
                // ReSharper disable once UnusedVariable
                var foo = instance is ConcreteType;
            }
            stopwatch.Stop();

            return stopwatch.Elapsed;
        }

        [Fact(DisplayName = "Downcasts are faster than typeof comparisons.")]
        public void DowncastVsTypeofComparison()
        {
            Abstraction instance = new ConcreteType();

            var downcastDuration = MeasureIsOperatorPerformance(instance);
            var typeofDuration = MeasureTypeofPerformance(instance);

            _output.WriteLine($"Downcast duration: {downcastDuration.TotalMilliseconds:N}ms");
            _output.WriteLine($"typeof duration:   {typeofDuration.TotalMilliseconds:N}ms");
            typeofDuration.Should().BeGreaterThan(downcastDuration);
        }

        private static TimeSpan MeasureTypeofPerformance(Abstraction instance)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (long i = 0; i < 100000000; ++i)
            {
                // ReSharper disable once UnusedVariable
                var foo = instance.GetType() == typeof(ConcreteType);
            }
            stopwatch.Stop();

            return stopwatch.Elapsed;
        }

        [Fact(DisplayName = "Is operator is basically as fast as as operator with additional null check.")]
        public void IsOperatorVsAsOperatorComparison()
        {
            Abstraction instance = new ConcreteType();

            var asDuration = MeasureAsOperatorPerformance(instance);
            var isDuration = MeasureIsOperatorPerformance(instance);

            _output.WriteLine($"Is duration: {isDuration.TotalMilliseconds:N}ms");
            _output.WriteLine($"As duration: {asDuration.TotalMilliseconds:N}ms");

            var greaterValue = Math.Max(asDuration.TotalMilliseconds, isDuration.TotalMilliseconds);
            var smallerValue = Math.Min(asDuration.TotalMilliseconds, isDuration.TotalMilliseconds);
            var differenceInPerCent = 1.0 - smallerValue / greaterValue;
            _output.WriteLine($"Difference: {differenceInPerCent:P}");

            differenceInPerCent.Should().BeLessThan(0.1);
        }

        private static TimeSpan MeasureAsOperatorPerformance(Abstraction instance)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (long i = 0; i < 100000000; ++i)
            {
                // ReSharper disable once UnusedVariable
                // ReSharper disable once TryCastAndCheckForNull.1
                var foo = instance as ConcreteType != null;
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