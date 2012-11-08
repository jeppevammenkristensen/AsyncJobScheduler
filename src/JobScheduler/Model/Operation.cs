using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobScheduler.Model
{
    public interface ICancellationAndProgessContainer
    {
        CancellationToken CancellationToken { get; }
        IProgress<ProgressInformation> Progress { get; }
    }



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
        

        public T Create<T>() where T : PartOfOperation
        {
            return (T)Activator.CreateInstance(typeof (T), CancellationToken, Progress);
        }
    }

    

    public class CancelAll
    {
        
    }

    public class UpdateMessage
    {
        public string Message { get; set; }
    }

    public class GetPrimeNumbers : PartOfOperation
    {
        public GetPrimeNumbers(CancellationToken cancellationToken, IProgress<ProgressInformation> progress) : base(cancellationToken, progress)
        {
        }

        public int EndIndex { get; set; }


        public override async Task RunAsync()
        {
            int startIndex = 1;

            while (startIndex < EndIndex)
            {
                if (CancellationToken.IsCancellationRequested)
                    return;
                await Task.Delay(100);
                
                startIndex++;
                Progress.Report(new ProgressInformation(startIndex * 100 / EndIndex, startIndex.ToString()));
            }
        }
    }

    public class ProgressInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ProgressInformation(int progressPercentage, string information)
        {
            ProgressPercentage = progressPercentage;
            Information = information;
        }

        public int ProgressPercentage { get; set; }
        public string Information { get; set; }
    }
}