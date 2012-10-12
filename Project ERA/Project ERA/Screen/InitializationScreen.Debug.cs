using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectERA.Data;
using System.Threading;
using System.Diagnostics;
using ProjectERA.Data.Enum;
using ERAUtils.Logger;

namespace ProjectERA.Screen
{
    internal partial class InitializationScreen : LoadingScreen
    {
        /// <summary>
        /// Tests AvatarSynths
        /// </summary>
        private void TestCase1()
        {
            AvatarSynths synths = new AvatarSynths();
            List<ElementType> types = new List<ElementType>();

            String[] names = Enum.GetNames(typeof(ElementType));
            foreach (String name in names)
            {
                ElementType type = (ElementType)Enum.Parse(typeof(ElementType), name);
                try
                {
                    synths.Store(type, 2);
                }
                catch (ArgumentException)
                {
                    Logger.Debug("Argumenterror " + type);
                    continue;
                }
                catch (NotSupportedException)
                {
                    continue;
                }
                types.Add(type);
            }
          
            synths.Clear();

            // Consuming
            Task task = Task.Factory.StartNew(() =>
            {
                foreach (ElementType elem in types)
                {
                    TestCase1Element(synths, elem, types.Count * 2);
                }
            });

            Task.WaitAll(task);

            Thread.MemoryBarrier();

            foreach (ElementType elem in types)
            {
                
                Debug.Assert(synths.Get(elem) == 0, "Threading fault " + elem + " " + synths.Get(elem));
            }

        }

        private void TestCase1Element(AvatarSynths synths, ElementType elem, Int32 progressElements)
        {
            Int32 rounds = 10000000;
            Int32 consume_r = 10;
            Int32 otherr = rounds/consume_r;
            Int32 split = 5;
            Int32 spincounter = 0;

             Task.Factory.StartNew(() =>
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

                 ProgressBy(1, progressElements);
                 Logger.Debug("Stored all for type" + element);
             }, TaskCreationOptions.AttachedToParent); //.ContinueWith((a) =>

             Task.Factory.StartNew(() =>
             {
                 SpinWait spinner = new SpinWait();

                 ElementType element = elem;

                 Int32 tasksToCreate = split;
                 Task[] tasks = new Task[split];

                 while (tasksToCreate-- > 0)
                     tasks[tasksToCreate] = Task.Factory.StartNew(() =>
                         {
                             Int32 counter = otherr/split;
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

                 ProgressBy(1, progressElements);

                 Logger.Debug("Consumed all for type" + element + " with " + spincounter + " yields");
             },TaskCreationOptions.AttachedToParent);// TaskContinuationOptions.AttachedToParent);
        }
    }
}
