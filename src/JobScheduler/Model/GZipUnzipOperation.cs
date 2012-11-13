using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;

namespace JobScheduler.Model
{
    public class GZipUnzipOperation : StreamOperation
    {
        public GZipUnzipOperation(CancellationToken cancellationToken, IProgress<ProgressInformation> progress, string source, string destination)
            : base(cancellationToken, progress, source, destination)
        {
        }

        public async override Task RunAsync()
        {
            var buffer = new byte[4096];

            using (var fileStream = File.OpenRead(Source))
            {
                using (var source = new GZipInputStream(fileStream))
                {
                    var length = source.Length;

                    using (var destination = File.Create(Destination))
                    {

                        int progress = 0;
                        bool flag = true;
                        while (flag)
                        {

                            int count = await source.ReadAsync(buffer, 0, buffer.Length);
                            progress += count;


                            if (count > 0)
                            {
                                destination.Write(buffer, 0, count);
                                Report(string.Format("{0}/{1}", progress, length), progress, (int) length);
                            }
                            else
                            {
                                destination.Flush();
                                flag = false;
                            }

                            if (count != 0 && progress / count % 100 == 0)
                                await Task.Delay(10);

                            if (CancellationToken.IsCancellationRequested)
                            {
                                flag = false;
                            }
                        }
                    }
                }

            }

        }


    }
}