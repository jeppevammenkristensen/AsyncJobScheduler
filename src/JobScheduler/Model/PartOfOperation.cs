using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobScheduler.Model
{
    public abstract class PartOfOperation : ICancellationAndProgessContainer
    {
        public CancellationToken CancellationToken { get; private set; }
        public IProgress<ProgressInformation> Progress { get; private set; }

        public PartOfOperation(CancellationToken cancellationToken, IProgress<ProgressInformation> progress)
        {
            CancellationToken = cancellationToken;
            Progress = progress;
        }

        public abstract Task RunAsync();

        public void Report(string message, float fetched, int total)
        {
            Progress.Report(new ProgressInformation(CalculatePercentage(fetched, total), message));
        }

        protected float CalculatePercentage(float fetched, float total)
        {
            return fetched * 100 / total;
        }

        public T Create<T>() where T : PartOfOperation
        {
            return (T)Activator.CreateInstance(typeof(T), CancellationToken, Progress);
        }
    }
}