using System;
using System.Threading;

namespace JobScheduler.Model
{
    public interface ICancellationAndProgessContainer
    {
        CancellationToken CancellationToken { get; }
        IProgress<ProgressInformation> Progress { get; }
    }
}