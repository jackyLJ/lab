﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _7BackgroundWorker
{
    class Program
    {
        static void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("DoWork thread pool thread id: {0}", Thread.CurrentThread.ManagedThreadId);

            var bw = (BackgroundWorker) sender;
            for (int i = 1; i <= 100; i++)
            {
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (i % 10 == 0)
                {
                    bw.ReportProgress(i);
                }

                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }

            e.Result = 42;
        }

        static void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine("{0}% completed. Progress thread pool thread id: {1}", 
                e.ProgressPercentage, Thread.CurrentThread.ManagedThreadId);
        }

        static void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("Completed thread pool thread id: {0}", Thread.CurrentThread.ManagedThreadId);

            if (e.Error != null)
            {
                Console.WriteLine("Exception {0} has occurred.", e.Error.Message);
            }
            else if (e.Cancelled)
            {
                Console.WriteLine("Operation has been cancelled.");
            }
            else
            {
                Console.WriteLine("The answser is: {0}", e.Result);
            }
        }

        static void Main(string[] args)
        {
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            bw.DoWork += Worker_DoWork;
            bw.ProgressChanged += Worker_ProgressChanged;
            bw.RunWorkerCompleted += Worker_Completed;

            bw.RunWorkerAsync();

            Console.WriteLine("Press c to cancel work");
            do
            {
                if (Console.ReadKey(true).KeyChar == 'c')
                {
                    bw.CancelAsync();
                }
            } while (bw.IsBusy);
        }
    }
}
