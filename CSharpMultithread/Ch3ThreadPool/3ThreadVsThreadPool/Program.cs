﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _3ThreadVsThreadPool
{
    class Program
    {
        static void Main(string[] args)
        {
            const int numberOfOperations = 500;
            var sw = new Stopwatch();
            sw.Start();

            UseThreads(numberOfOperations);
            sw.Stop();
            Console.WriteLine("Executing time using threads: {0}", sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            UseThreadPool(numberOfOperations);
            sw.Stop();
            Console.WriteLine("Executing time using threadpool: {0}", sw.ElapsedMilliseconds);

            Console.WriteLine("Done...");
            Console.ReadKey();
        }

        static void UseThreads(int numberOfOperations)
        {
            using (var countDown = new CountdownEvent(numberOfOperations))
            {
                Console.WriteLine("Scheduling work by creating threads");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    var thread = new Thread(() =>
                    {
                        Console.Write("{0},", Thread.CurrentThread.ManagedThreadId);
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countDown.Signal();
                    });

                    thread.Start();
                }

                countDown.Wait();

                Console.WriteLine();
            }
        }

        static void UseThreadPool(int numberOfOperations)
        {
            using (var countDown = new CountdownEvent(numberOfOperations))
            {
                Console.WriteLine("Starting work on a threadpool");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        Console.Write("{0},", Thread.CurrentThread.ManagedThreadId);
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countDown.Signal();
                    });
                }

                countDown.Wait();
                Console.WriteLine();
            }
        }
    }
}
