using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace JobScheduler.Model
{
    public class FtpDownloadFileOperation : StreamOperation
    {
        public FtpDownloadFileOperation(CancellationToken cancellationToken, IProgress<ProgressInformation> progress, string source, string destination)
            : base(cancellationToken, progress, source, destination)
        {
        }

        public override async Task RunAsync()
        {
            Progress.Report(new ProgressInformation(0, string.Format("Starting download of {0}", Source)));

            var request = (FtpWebRequest)WebRequest.Create(Source);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            var res = await request.GetResponseAsync();
            var size = res.ContentLength;
            Report(string.Format("Size of file is {0}", size), 0, 1);

            request = (FtpWebRequest)WebRequest.Create(Source);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;
            request.KeepAlive = false;

            var response = await request.GetResponseAsync();


            var responseStream = response.GetResponseStream();
            File.Delete(Destination);

            var fileStream = File.OpenWrite(Destination);

            var buffer = new byte[BufferSize];
            await base.TransferFromSourceToDestination(responseStream, fileStream, buffer, (int)size);
            
        }
    }
}