using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using RaspberryIRDotNet.RX;
using RaspberryIRDotNet.RX.PulseSpaceSource;
using RaspberryIRDotNet.RX.Filters;
using Moq;

namespace RaspberryIRDotNet.Tests.Unit.RX.PulseSpaceSource
{
    class FilterPulseSpaceSourceTests
    {
        private static ReceivedPulseSpaceBurstEventArgs CreateReceivedPulseSpaceBurstEventArgs()
        {
            var buffer = new PulseSpaceDurationList()
            {
                500,
                200,
                100,
                100
            };
            return new ReceivedPulseSpaceBurstEventArgs(buffer);
        }

        [Test]
        public void Constructor_NoFilters()
        {
            // ARRANGE
            var sourceMock = new Mock<IPulseSpaceSource>();
            sourceMock.Setup(x => x.Capture(It.IsAny<ReadCancellationToken>()));

            // ACT
            var subject = new FilterPulseSpaceSource(sourceMock.Object);

            // ASSERT
            Assert.That(subject.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters.Count, Is.Zero);
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Never);

            // Now verify that the source has been set by the constructor....

            // ACT
            subject.Capture(null);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Never);

            // ACT
            var cancelToken = new ReadCancellationToken();
            subject.Capture(cancelToken);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Exactly(2));
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == cancelToken)), Times.Once);
        }

        [Test]
        public void Constructor_NoFilters_NullSource()
        {
            Assert.That(() => new FilterPulseSpaceSource(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_ParamsFilters()
        {
            // ARRANGE
            var sourceMock = new Mock<IPulseSpaceSource>();
            sourceMock.Setup(x => x.Capture(It.IsAny<ReadCancellationToken>()));

            var filter1 = new DurationFilter()
            {
                Minimum = 100
            };
            var filter2 = new DurationFilter()
            {
                Minimum = 200
            };

            // ACT
            var subject = new FilterPulseSpaceSource(sourceMock.Object, filter1, filter2);

            // ASSERT
            Assert.That(subject.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters.Count, Is.EqualTo(2));
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Never);

            // Now verify that the source has been set by the constructor....

            // ACT
            subject.Capture(null);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Never);

            // ACT
            var cancelToken = new ReadCancellationToken();
            subject.Capture(cancelToken);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Exactly(2));
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == cancelToken)), Times.Once);
        }

        [Test]
        public void Constructor_ParamsFilters_NullSource()
        {
            var filter1 = new DurationFilter()
            {
                Minimum = 100
            };
            var filter2 = new DurationFilter()
            {
                Minimum = 200
            };

            Assert.That(() => new FilterPulseSpaceSource(null, filter1, filter2), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_ArrayFilters()
        {
            // ARRANGE
            var sourceMock = new Mock<IPulseSpaceSource>();
            sourceMock.Setup(x => x.Capture(It.IsAny<ReadCancellationToken>()));

            var filters = new DurationFilter[]
            {
                new DurationFilter()
                {
                    Minimum = 100
                },
                new DurationFilter()
                {
                    Minimum = 200
                }
            };

            // ACT
            var subject = new FilterPulseSpaceSource(sourceMock.Object, filters);

            // ASSERT
            Assert.That(subject.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters.Count, Is.EqualTo(2));
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Never);

            // Now verify that the source has been set by the constructor....

            // ACT
            subject.Capture(null);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Never);

            // ACT
            var cancelToken = new ReadCancellationToken();
            subject.Capture(cancelToken);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Exactly(2));
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == cancelToken)), Times.Once);
        }

        [Test]
        public void Constructor_ArrayFilters_NullSource()
        {
            var filters = new DurationFilter[]
            {
                new DurationFilter()
                {
                    Minimum = 100
                },
                new DurationFilter()
                {
                    Minimum = 200
                }
            };

            Assert.That(() => new FilterPulseSpaceSource(null, filters), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_ArrayFilters_NullFilters()
        {
            // ARRANGE
            var sourceMock = new Mock<IPulseSpaceSource>();
            sourceMock.Setup(x => x.Capture(It.IsAny<ReadCancellationToken>()));

            DurationFilter[] filters = null;

            // ACT
            var subject = new FilterPulseSpaceSource(sourceMock.Object, filters);

            // ASSERT
            Assert.That(subject.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters.Count, Is.Zero);
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Never);

            // Now verify that the source has been set by the constructor....

            // ACT
            subject.Capture(null);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Never);

            // ACT
            var cancelToken = new ReadCancellationToken();
            subject.Capture(cancelToken);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Exactly(2));
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == cancelToken)), Times.Once);
        }

        [Test]
        public void Constructor_ListFilters()
        {
            // ARRANGE
            var sourceMock = new Mock<IPulseSpaceSource>();
            sourceMock.Setup(x => x.Capture(It.IsAny<ReadCancellationToken>()));

            var filters = new List<DurationFilter>()
            {
                new DurationFilter()
                {
                    Minimum = 100
                },
                new DurationFilter()
                {
                    Minimum = 200
                }
            };

            // ACT
            var subject = new FilterPulseSpaceSource(sourceMock.Object, filters);

            // ASSERT
            Assert.That(subject.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters.Count, Is.EqualTo(2));
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Never);

            // Now verify that the source has been set by the constructor....

            // ACT
            subject.Capture(null);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Never);

            // ACT
            var cancelToken = new ReadCancellationToken();
            subject.Capture(cancelToken);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Exactly(2));
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == cancelToken)), Times.Once);
        }

        [Test]
        public void Constructor_ListFilters_NullSource()
        {
            var filters = new List<DurationFilter>()
            {
                new DurationFilter()
                {
                    Minimum = 100
                },
                new DurationFilter()
                {
                    Minimum = 200
                }
            };

            Assert.That(() => new FilterPulseSpaceSource(null, filters), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_ListFilters_NullFilters()
        {
            // ARRANGE
            var sourceMock = new Mock<IPulseSpaceSource>();
            sourceMock.Setup(x => x.Capture(It.IsAny<ReadCancellationToken>()));

            List<DurationFilter> filters = null;

            // ACT
            var subject = new FilterPulseSpaceSource(sourceMock.Object, filters);

            // ASSERT
            Assert.That(subject.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters, Is.Not.Null);
            Assert.That(subject.Filters.Filters.Count, Is.Zero);
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Never);

            // Now verify that the source has been set by the constructor....

            // ACT
            subject.Capture(null);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Never);

            // ACT
            var cancelToken = new ReadCancellationToken();
            subject.Capture(cancelToken);
            // ASSERT
            sourceMock.Verify(x => x.Capture(It.IsAny<ReadCancellationToken>()), Times.Exactly(2));
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == null)), Times.Once);
            sourceMock.Verify(x => x.Capture(It.IsNotNull<ReadCancellationToken>()), Times.Once);
            sourceMock.Verify(x => x.Capture(It.Is<ReadCancellationToken>(arg => arg == cancelToken)), Times.Once);
        }

        [Test]
        public void Constructor_ListFilters_NullFilterItem()
        {
            // ARRANGE
            var sourceMock = new Mock<IPulseSpaceSource>();
            sourceMock.Setup(x => x.Capture(It.IsAny<ReadCancellationToken>()));

            var filters = new List<DurationFilter>()
            {
                new DurationFilter()
                {
                    Minimum = 100
                },
                null
            };

            // ACT, ASSERT
            Assert.That(() => new FilterPulseSpaceSource(sourceMock.Object, filters), Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RealTime(bool value)
        {
            var sourceMock = new Mock<IPulseSpaceSource>(MockBehavior.Strict);
            sourceMock.SetupGet(x => x.RealTime).Returns(value);
            var subject = new FilterPulseSpaceSource(sourceMock.Object);

            Assert.That(subject.RealTime, Is.EqualTo(value));
        }

        [Test]
        public void NoEventSubscribers()
        {
            // ARRANGE
            var sourceMock = new Mock<IPulseSpaceSource>(MockBehavior.Strict);
            var subject = new FilterPulseSpaceSource(sourceMock.Object);

            // ACT, ASSERT    Make sure the event does not raise a null reference exception or anything like that.
            sourceMock.Raise(x => x.ReceivedPulseSpaceBurst += null, CreateReceivedPulseSpaceBurstEventArgs());
        }

        [Test]
        public void Filtering_NoFilters()
        {
            // ARRANGE
            var sourceMock = new Mock<IPulseSpaceSource>(MockBehavior.Strict);

            int eventCount = 0;
            IReadOnlyPulseSpaceDurationList lastBuffer = null;
            object lastSender = null;
            var subject = new FilterPulseSpaceSource(sourceMock.Object);

            // ACT, ASSERT
            subject.ReceivedPulseSpaceBurst += (s, e) =>
            {
                lastSender = s;
                lastBuffer = e.Buffer;
                eventCount++;
            };
            sourceMock.Raise(x => x.ReceivedPulseSpaceBurst += null, CreateReceivedPulseSpaceBurstEventArgs());

            // ASSERT
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(lastSender, Is.SameAs(subject));
            Assert.That(lastBuffer, Is.Not.Null);
            Assert.That(lastBuffer.Count, Is.EqualTo(4));
            Assert.That(lastBuffer[0], Is.EqualTo(500));
            Assert.That(lastBuffer[1], Is.EqualTo(200));
            Assert.That(lastBuffer[2], Is.EqualTo(100));
            Assert.That(lastBuffer[3], Is.EqualTo(100));
        }

        [Test]
        public void Filtering_FilterPass()
        {
            // ARRANGE
            var sourceMock = new Mock<IPulseSpaceSource>(MockBehavior.Strict);

            var filter1 = new DurationFilter()
            {
                Minimum = 70
            };
            var filter2 = new DurationFilter()
            {
                Maximum = 900
            };

            int eventCount = 0;
            IReadOnlyPulseSpaceDurationList lastBuffer = null;
            object lastSender = null;
            var subject = new FilterPulseSpaceSource(sourceMock.Object, filter1, filter2);

            // ACT, ASSERT
            subject.ReceivedPulseSpaceBurst += (s, e) =>
            {
                lastSender = s;
                lastBuffer = e.Buffer;
                eventCount++;
            };
            sourceMock.Raise(x => x.ReceivedPulseSpaceBurst += null, CreateReceivedPulseSpaceBurstEventArgs());

            // ASSERT
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(lastSender, Is.SameAs(subject));
            Assert.That(lastBuffer, Is.Not.Null);
            Assert.That(lastBuffer.Count, Is.EqualTo(4));
            Assert.That(lastBuffer[0], Is.EqualTo(500));
            Assert.That(lastBuffer[1], Is.EqualTo(200));
            Assert.That(lastBuffer[2], Is.EqualTo(100));
            Assert.That(lastBuffer[3], Is.EqualTo(100));
        }

        [Test]
        public void Filtering_FilterBlock()
        {
            // ARRANGE
            var sourceMock = new Mock<IPulseSpaceSource>(MockBehavior.Strict);

            var filter1 = new DurationFilter()
            {
                Minimum = 70
            };
            var filter2 = new DurationFilter()
            {
                Maximum = 200
            };

            int eventCount = 0;
            var subject = new FilterPulseSpaceSource(sourceMock.Object, filter1, filter2);

            // ACT, ASSERT
            subject.ReceivedPulseSpaceBurst += (s, e) =>
            {
                eventCount++;
            };
            sourceMock.Raise(x => x.ReceivedPulseSpaceBurst += null, CreateReceivedPulseSpaceBurstEventArgs());

            // ASSERT
            Assert.That(eventCount, Is.Zero);
        }
    }
}
