using System.Threading;

using Quartz;

namespace ProblemWithGetTriggersOfJob
{
    public class MySimpleJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Thread.Sleep(500);
        }
    }
}