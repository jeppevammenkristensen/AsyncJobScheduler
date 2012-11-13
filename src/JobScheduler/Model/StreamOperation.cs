using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JobScheduler.Model
{
    public abstract class StreamOperation : PartOfOperation
    {
        public StreamOperation(CancellationToken cancellationToken, IProgress<ProgressInformation> progress, string source, string destination)
            : base(cancellationToken, progress)
        {
            Source = source;
            Destination = destination;
        }

        public string Source { get; set; }
        public string Destination { get; set; }

        /// <summary>
        /// Override to set a larger buffersize. It defaults to 4096
        /// </summary>
        protected virtual int BufferSize { get { return 4096; } }


        protected virtual async Task TransferFromSourceToDestination(Stream source, Stream destination, Byte[] buffer, int length)
        {
            try
            {

            
                using (destination)
                {
                    try
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
                                Report(string.Format("{0}/{1}", progress, length), progress, (int)length);
                            }
                            else
                            {
                                destination.Flush();
                                flag = false;
                            }

                            if (CancellationToken.IsCancellationRequested)
                            {
                                flag = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        int i = 0;

                    }
                }

            }
            finally 
            {
                source.Flush();
                source.Close();
            }

        }

    }
}