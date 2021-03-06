// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace UnitTest.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Bond.Comm;
    using NUnit.Framework;

    [TestFixture]
    public class LogTests
    {
        private class TestLogHandler : LogHandler
        {
            public LogSeverity? LastMessageSeverity;
            public string LastMessage;
            public Exception LastException;
            public int MessagesHandled;

            public void Handle(LogSeverity severity, Exception exception, string format, params object[] args)
            {
                LastMessageSeverity = severity;
                LastMessage = string.Format(format, args);
                LastException = exception;
                MessagesHandled++;
            }

            public void Clear()
            {
                LastMessageSeverity = null;
                LastMessage = null;
                LastException = null;
            }
        }
        private TestLogHandler handler = new TestLogHandler();

        /// <summary>
        /// Carries a <see cref="LogSeverity"/> so we can tell which invocation produced it.
        /// </summary>
        private class TestException : Exception
        {
            public readonly LogSeverity Severity;

            public TestException(LogSeverity severity)
            {
                Severity = severity;
            }
        }

        private readonly List<LogSeverity> allSeverities = Enum.GetValues(typeof(LogSeverity)).Cast<LogSeverity>().ToList();

        private readonly Dictionary<LogSeverity, Action<string, object[]>> levelLoggers =
            new Dictionary<LogSeverity, Action<string, object[]>>
            {
                { LogSeverity.Debug, Log.Debug },
                { LogSeverity.Information, Log.Information },
                { LogSeverity.Warning, Log.Warning },
                { LogSeverity.Error, Log.Error },
                { LogSeverity.Fatal, Log.Fatal },
        };

        private readonly Dictionary<LogSeverity, Action<Exception, string, object[]>> exceptionLevelLoggers =
            new Dictionary<LogSeverity, Action<Exception, string, object[]>>
            {
                { LogSeverity.Debug, Log.Debug },
                { LogSeverity.Information, Log.Information },
                { LogSeverity.Warning, Log.Warning },
                { LogSeverity.Error, Log.Error },
                { LogSeverity.Fatal, Log.Fatal },
        };

        private static Tuple<string, object[]> MakeMessage(LogSeverity severity, bool withException)
        {
            var args = new object[] {severity, withException ? "" : "out" };
            return new Tuple<string, object[]>("logged at {0} with{1} an Exception", args);
        }

        [SetUp]
        public void SetUp()
        {
            handler = new TestLogHandler();
            Log.SetHandler(handler);
        }

        [TearDown]
        public void TearDown()
        {
            Log.RemoveHandler();
        }

        [Test]
        public void SeveritiesAreSorted()
        {
            var severitiesAscending = new List<LogSeverity>(new[]
            {
                LogSeverity.Debug,
                LogSeverity.Information,
                LogSeverity.Warning,
                LogSeverity.Error,
                LogSeverity.Fatal
            });
            var numSeverities = severitiesAscending.Count;
            // Make sure this list is complete.
            CollectionAssert.AreEquivalent(allSeverities, severitiesAscending);

            var lower = severitiesAscending.GetRange(0, numSeverities - 1);
            var higher = severitiesAscending.GetRange(1, numSeverities - 1);
            var pairs = lower.Zip(higher, (l, h) => new Tuple<LogSeverity, LogSeverity>(l, h));
            foreach (var pair in pairs)
            {
                Assert.Less(pair.Item1, pair.Item2);
            }
        }

        [Test]
        public void DuplicateHandlersAreRejected()
        {
            Assert.Throws<InvalidOperationException>(() => Log.SetHandler(handler));
        }

        [Test]
        public void NullHandlersAreRejected()
        {
            Assert.Throws<ArgumentNullException>(() => Log.SetHandler(null));
        }

        [Test]
        public void LoggingWithNullHandlerIsANoOp()
        {
            // Clear the handler registered by SetUp().
            Log.RemoveHandler();
            Log.Information("no-op");
        }

        [Test]
        public void Levels()
        {
            // Make sure we're testing all severities.
            CollectionAssert.AreEquivalent(allSeverities, levelLoggers.Keys);
            CollectionAssert.AreEquivalent(allSeverities, exceptionLevelLoggers.Keys);

            var messagesLogged = 0;

            foreach (var severity in allSeverities)
            {
                var logger = levelLoggers[severity];
                var exceptionLogger = exceptionLevelLoggers[severity];
                var exception = new TestException(severity);

                var formatArgs = MakeMessage(severity, withException: false);
                var format = formatArgs.Item1;
                var args = formatArgs.Item2;
                var formatted = string.Format(format, args);
                logger(format, args);
                Assert.AreEqual(severity, handler.LastMessageSeverity);
                Assert.AreEqual(formatted, handler.LastMessage);
                Assert.IsNull(handler.LastException);
                Assert.AreEqual(messagesLogged + 1, handler.MessagesHandled);
                messagesLogged++;
                handler.Clear();

                formatArgs = MakeMessage(severity, withException: true);
                format = formatArgs.Item1;
                args = formatArgs.Item2;
                formatted = string.Format(format, args);
                exceptionLogger(exception, format, args);
                Assert.AreEqual(severity, handler.LastMessageSeverity);
                Assert.AreEqual(formatted, handler.LastMessage);
                Assert.NotNull(handler.LastException);
                Assert.AreEqual(severity, ((TestException) handler.LastException).Severity);
                Assert.AreEqual(messagesLogged + 1, handler.MessagesHandled);
                messagesLogged++;
                handler.Clear();
            }
        }

        [Test]
        public void FatalWithFormatted()
        {
            var messagesLogged = 0;
            var exception = new TestException(LogSeverity.Fatal);

            var formatArgs = MakeMessage(LogSeverity.Fatal, withException: false);
            var format = formatArgs.Item1;
            var args = formatArgs.Item2;
            var formatted = string.Format(format, args);
            var messageReturned = LogUtil.FatalAndReturnFormatted(format, args);
            Assert.AreEqual(LogSeverity.Fatal, handler.LastMessageSeverity);
            Assert.AreEqual(formatted, handler.LastMessage);
            Assert.AreEqual(messageReturned, handler.LastMessage);
            Assert.IsNull(handler.LastException);
            Assert.AreEqual(messagesLogged + 1, handler.MessagesHandled);
            messagesLogged++;
            handler.Clear();

            formatArgs = MakeMessage(LogSeverity.Fatal, withException: true);
            format = formatArgs.Item1;
            args = formatArgs.Item2;
            formatted = string.Format(format, args);
            messageReturned = LogUtil.FatalAndReturnFormatted(exception, format, args);
            Assert.AreEqual(LogSeverity.Fatal, handler.LastMessageSeverity);
            Assert.AreEqual(formatted, handler.LastMessage);
            Assert.AreEqual(messageReturned, handler.LastMessage);
            Assert.NotNull(handler.LastException);
            Assert.AreEqual(LogSeverity.Fatal, ((TestException)handler.LastException).Severity);
            Assert.AreEqual(messagesLogged + 1, handler.MessagesHandled);
        }
    }
}
