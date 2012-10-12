using ProjectERA.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectERA.Data.Enum;
using System;
using System.Threading.Tasks;
using System.Threading;
namespace ERATest
{
    
    
    /// <summary>
    ///This is a test class for AvatarSynthsTest and is intended
    ///to contain all AvatarSynthsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AvatarSynthsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod]
        public void TestConcurrencySafety()
        {
            AvatarSynths synths = new AvatarSynths();

            ElementType[] elements = {ElementType.Agility, ElementType.Air, ElementType.Dark, ElementType.Earth,
                                ElementType.Endurance, ElementType.Fire, ElementType.Light, ElementType.Mind,
                                ElementType.Spirit, ElementType.Strength, ElementType.Water};

            synths.Clear();

            Task[] tasks = new Task[elements.Length];
            Int32 i = 0;

            foreach (ElementType elem in elements)
            {
                ElementType element = elem;

                tasks[i++] = Task.Factory.StartNew(() => TestCase1Element(synths, ElementType.Agility));
            }

            Task.WaitAll(tasks);

            foreach (ElementType elem in elements)
            {
                Assert.AreEqual(0, synths.Agility);
            }
        }

        private void TestCase1Element(AvatarSynths synths, ElementType elem)
        {
            Int32 rounds = 10000000;
            Int32 consume_r = 10;
            Int32 otherr = rounds / consume_r;
            Int32 split = 5;
            Int32 spincounter = 0;

            Task task = Task.Factory.StartNew(() =>
            {
                ElementType element = elem;
                Int32 tasksToCreate = split;
                Task[] tasks = new Task[split];

                while (tasksToCreate-- > 0)
                    tasks[tasksToCreate] = Task.Factory.StartNew(() =>
                    {
                        Int32 counter = rounds / split;
                        while (counter-- > 0)
                        {
                            synths.Store(element, 1);
                        }
                    }, TaskCreationOptions.AttachedToParent);


                Task.WaitAll(tasks);

            }, TaskCreationOptions.AttachedToParent); //.ContinueWith((a) =>

            Task othertask = Task.Factory.StartNew(() =>
            {
                SpinWait spinner = new SpinWait();

                ElementType element = elem;

                Int32 tasksToCreate = split;
                Task[] tasks = new Task[split];

                while (tasksToCreate-- > 0)
                    tasks[tasksToCreate] = Task.Factory.StartNew(() =>
                    {
                        Int32 counter = otherr / split;
                        while (counter-- > 0)
                        {
                            while (true)
                            {
                                if (synths.TryConsumeWhile(element, consume_r))
                                    break;

                                spinner.SpinOnce();

                                if (spinner.NextSpinWillYield)
                                    while (true)
                                    {
                                        Int32 snapshot = spincounter;
                                        Int32 newvalue = Interlocked.Increment(ref spincounter);
                                        if (snapshot + 1 == newvalue)
                                            break;
                                        Thread.Sleep(1);
                                    }
                            }
                        }
                    }, TaskCreationOptions.AttachedToParent);

                Task.WaitAll(tasks);

            }, TaskCreationOptions.AttachedToParent);// TaskContinuationOptions.AttachedToParent);

            Task.WaitAll(task, othertask);
        }
    }
}
