using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

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
            Progress.Report(new ProgressInformation(CalculatePercentage(fetched,total),message));
        }

        protected float CalculatePercentage(float fetched, float total)
        {
            return fetched*100/total;
        }

        public T Create<T>() where T : PartOfOperation
        {
            return (T)Activator.CreateInstance(typeof (T), CancellationToken, Progress);
        }
    }
    
    public interface IRequire<T> where T : class
    {
        void Input(T input);
    }

    public interface IProduce<T> where T : class
    {
        void Output(out T result);
    }



    public class GZipUnzipOperation : PartOfOperation
    {
        public GZipUnzipOperation(CancellationToken cancellationToken, IProgress<ProgressInformation> progress, string source, string destination) : base(cancellationToken, progress)
        {
            Source = source;
            Destination = destination;
        }

        public string Source { get; set; }
        public string Destination { get; set; }

        public async override Task RunAsync()
        {
            var buffer = new byte[4096];

            using (var fileStream =File.OpenRead(Source))
            {
                using (var gzipStream = new GZipInputStream(fileStream))
                {
                    var length = gzipStream.Length;

                    using (TarArchive archvie = TarArchive.CreateInputTarArchive(gzipStream))
                    {
                        Progress.Report(new ProgressInformation(0.5f,"Beginning to unzip"));
                        archvie.ExtractContents(Destination);
                        Progress.Report(new ProgressInformation(1.0f, "Finished to unzip"));
                    }
                    



                    using(var output = File.Create(Path.Combine(Destination, Path.GetFileNameWithoutExtension(Source))))
                    {

                        int progress = 0;
                        bool flag = true;
                        while (flag)
                        {
                            int count = await gzipStream.ReadAsync(buffer, 0, buffer.Length);
                            progress += count;
                            
                            if (count > 0)
                            {
                                output.Write(buffer, 0, count);
                                Report(string.Format("{0}/{1}", progress, length),progress,(int)length);
                            }
                            else
                            {
                                output.Flush();
                                flag = false;
                            }
                        }
                    }
                }
            }
        }
    }


    public class FtpDownloadFileOperation : PartOfOperation, IProduce<Stream>
    {
        public FtpDownloadFileOperation(CancellationToken cancellationToken, IProgress<ProgressInformation> progress, string source, string destination) : base(cancellationToken, progress)
        {
            Source = source;
            Destination = destination;
        }

        public string Source { get; set; }
        public string Destination { get; set; }

        public Stream Stream { get; set; }

        public override async Task RunAsync()
        {
            Progress.Report(new ProgressInformation(0, string.Format("Starting download of {0}", Source)));

            var request = (FtpWebRequest) WebRequest.Create(Source);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            var res = await request.GetResponseAsync();
            var size = res.ContentLength;
            Report(string.Format("Size of file is {0}", size),0,1);

            request = (FtpWebRequest) WebRequest.Create(Source);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;
            request.KeepAlive = false;

            using (var response = await request.GetResponseAsync())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    File.Delete(Destination);

                    using (var fileStream = File.OpenWrite(Destination))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead = await responseStream.ReadAsync(buffer, 0, 4096);
                        var currentProgress = bytesRead;
                        while (bytesRead > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);

                            bytesRead = await responseStream.ReadAsync(buffer, 0, 4096);
                            currentProgress += bytesRead;
                            Report(string.Format("Downloaded {0} of {1}", currentProgress, size), currentProgress, (int)size);
                        }
                    }
                }
            }
        }

        public void Output(out Stream result)
        {
            result = Stream;
        }
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
        public ProgressInformation(float progressPercentage, string information)
        {
            ProgressPercentage = progressPercentage;
            Information = information;
        }

        public float ProgressPercentage { get; set; }
        public string Information { get; set; }
    }
}