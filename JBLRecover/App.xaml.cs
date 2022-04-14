using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JBLRecover
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public static Mutex mutex = new Mutex(false, "JBLRecover");
        public App()
        {
            // TimeSpan.Zero to test the mutex's signal state and
            // return immediately without blocking
            bool isAnotherInstanceOpen = !mutex.WaitOne(TimeSpan.Zero);
            if (isAnotherInstanceOpen)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}
