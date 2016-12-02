using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace TrackerService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<Resovle>(s =>
                {
                    s.ConstructUsing(name => new Resovle());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Tracker Service monitor iis log");
                x.SetDisplayName("TrackerService");
                x.SetServiceName("TrackerService");
            });
        }
    }
}
