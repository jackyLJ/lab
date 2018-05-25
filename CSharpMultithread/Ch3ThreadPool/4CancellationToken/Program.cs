﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _4CancellationToken
{
    class Program
    {
        static void AsyncOperation1(CancellationToken token)
        {
            Console.WriteLine("Starting the first task");
            for (int i = 0; i < 5; i++)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("The first task has been cancelled.");
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Console.WriteLine("The first task has completed successfully");
        }

        static void AsyncOperation2(CancellationToken token)
        {
            try
            {
                Console.WriteLine("Starting the second task");

                for (int i = 0; i < 5; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                Console.WriteLine("The second task has completed successfully");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("The second task has been cancelled.");
            }
        }

        static void AsyncOperation3(CancellationToken token)
        {
            bool cancellationFlag = false;
            token.Register(() => cancellationFlag = true);
            Console.WriteLine("Starting the third task");

            for (int i = 0; i < 5; i++)
            {
                if (cancellationFlag)
                {
                    Console.WriteLine("The third task has been cancelled.");
                    return;
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Console.WriteLine("The third task has completed successfully");
        }

        static void Main(string[] args)
        {
            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;

                ThreadPool.QueueUserWorkItem(_ => AsyncOperation1(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperation2(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperation3(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));

            Console.WriteLine("done....");
            Console.ReadKey();
        }
    }
}
