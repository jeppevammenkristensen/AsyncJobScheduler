using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Tar;

namespace JobScheduler.Model
{
    public interface IRequire<T> where T : class
    {
        void Input(T input);
    }

    public interface IProduce<T> where T : class
    {
        void Output(out T result);
    }


    public class UpdateMessage
    {
        public string Message { get; set; }
    }

    public class GetPrimeNumbers : PartOfOperation
    {
        public GetPrimeNumbers(CancellationToken cancellationToken, IProgress<ProgressInformation> progress)
            : base(cancellationToken, progress)
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
        public ProgressInformation(float progressPercentage, string information)
        {
            ProgressPercentage = progressPercentage;
            Information = information;
        }

        public float ProgressPercentage { get; set; }
        public string Information { get; set; }
    }
}