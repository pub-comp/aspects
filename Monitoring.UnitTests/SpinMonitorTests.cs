using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PubComp.Aspects.Monitoring.UnitTests
{
    [TestClass]
    public class SpinMonitorTests
    {
        [TestMethod]
        public void Test_EnterExit_WithSingleTask()
        {
            const int numberOfIterations = 200;
            int cnt = 0;
            var monitor = new SpinMonitor();

            for (int i = 0; i < numberOfIterations; i++)
            {
                monitor.Enter();
                cnt++;
                monitor.Exit();
            }

            Assert.AreEqual(numberOfIterations, cnt);
        }

        [TestMethod]
        public void Test_InMonitor_WithSingleTask()
        {
            const int numberOfIterations = 200;
            int cnt = 0;
            var monitor = new SpinMonitor();

            for (int i = 0; i < numberOfIterations; i++)
                monitor.InMonitor(() => cnt++);

            Assert.AreEqual(numberOfIterations, cnt);
        }

        [TestMethod]
        public void Test_InMonitor_WithDoubleEntriesSingleTask()
        {
            const int numberOfIterations = 200;
            int cnt = 0;
            var monitor = new SpinMonitor();

            for (int i = 0; i < numberOfIterations; i++)
            {
                monitor.InMonitor(() =>
                {
                    cnt++;

                    monitor.InMonitor(() =>
                    {
                        cnt++;
                    });
                });
            }

            Assert.AreEqual(numberOfIterations * 2, cnt);
        }

        [TestMethod]
        public void Test_InMonitor_WithDoubleEntriesSingleTask_ExceptionInInner()
        {
            const int numberOfIterations = 200;
            int cnt = 0;
            var monitor = new SpinMonitor();

            for (int i = 0; i < numberOfIterations; i++)
            {
                try
                {
                    monitor.InMonitor(() =>
                    {
                        cnt++;

                        monitor.InMonitor(() =>
                        {
                            cnt++;
                            throw new ApplicationException();
                        });
                    });
                }
                catch (ApplicationException)
                {
                }
            }

            Assert.AreEqual(numberOfIterations * 2, cnt);
        }

        [TestMethod]
        public void Test_InMonitor_WithDoubleEntriesSingleTask_ExceptionInOuter()
        {
            const int numberOfIterations = 200;
            int cnt = 0;
            var monitor = new SpinMonitor();

            for (int i = 0; i < numberOfIterations; i++)
            {
                try
                {
                    monitor.InMonitor(() =>
                    {
                        cnt++;

                        monitor.InMonitor(() =>
                        {
                            cnt++;
                        });

                        throw new ApplicationException();
                    });
                }
                catch (ApplicationException)
                {
                }
            }

            Assert.AreEqual(numberOfIterations * 2, cnt);
        }

        [TestMethod]
        public void Test_EnterExit_WithMultipleTasks()
        {
            const int numberOfIterations = 200;
            int cnt = 0;
            var monitor = new SpinMonitor();

            Parallel.For(0, numberOfIterations, i =>
            {
                monitor.Enter();
                cnt++;
                monitor.Exit();
            });

            Assert.AreEqual(numberOfIterations, cnt);
        }

        [TestMethod]
        public void Test_InMonitor_WithMultipleTasks()
        {
            const int numberOfIterations = 200;
            int cnt = 0;
            var monitor = new SpinMonitor();

            Parallel.For(0, numberOfIterations, i =>
                monitor.InMonitor(() => cnt++));

            Assert.AreEqual(numberOfIterations, cnt);
        }

        [TestMethod]
        public void Test_InMonitor_WithDoubleEntriesMultipleTasks()
        {
            const int numberOfIterations = 200;
            int cnt = 0;
            var monitor = new SpinMonitor();

            Parallel.For(0, numberOfIterations, i =>
                monitor.InMonitor(() =>
                {
                    cnt++;

                    monitor.InMonitor(() =>
                    {
                        cnt++;
                    });
                }));

            Assert.AreEqual(numberOfIterations * 2, cnt);
        }

        [TestMethod]
        public void Test_InMonitor_WithDoubleEntriesMultipleTasks_ExceptionInInner()
        {
            const int numberOfIterations = 200;
            int cnt = 0;
            var monitor = new SpinMonitor();

            Parallel.For(0, numberOfIterations, i =>
            {
                try
                {
                    monitor.InMonitor(() =>
                    {
                        cnt++;

                        monitor.InMonitor(() =>
                        {
                            cnt++;
                            throw new ApplicationException();
                        });
                    });
                }
                catch (ApplicationException)
                {
                }
            });

            Assert.AreEqual(numberOfIterations * 2, cnt);
        }

        [TestMethod]
        public void Test_InMonitor_WithDoubleEntriesMultipleTasks_ExceptionInOuter()
        {
            const int numberOfIterations = 200;
            int cnt = 0;
            var monitor = new SpinMonitor();

            Parallel.For(0, numberOfIterations, i =>
            {
                try
                {
                    monitor.InMonitor(() =>
                    {
                        cnt++;

                        monitor.InMonitor(() =>
                        {
                            cnt++;
                        });

                        throw new ApplicationException();
                    });
                }
                catch (ApplicationException)
                {
                }
            });

            Assert.AreEqual(numberOfIterations * 2, cnt);
        }
    }
}
