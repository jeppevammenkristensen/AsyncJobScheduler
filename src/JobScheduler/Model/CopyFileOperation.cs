using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobScheduler.Model
{
    public class CopyFileOperation : StreamOperation
    {
        public CopyFileOperation(CancellationToken cancellationToken, IProgress<ProgressInformation> progress, string source, string destination)
            : base(cancellationToken, progress, source, destination)
        {
        }

        public async override Task RunAsync()
        {
            if (!File.Exists(Source))
            {
                throw new InvalidOperationException("File does not exist");
            }

            var length = new FileInfo(Source).Length;
            using (var source = File.OpenRead(Source))
            {
                using (var destination = File.OpenWrite(Destination))
                {
                    await TransferFromSourceToDestination(source, destination, new byte[BufferSize], (int)length);
                }
            }
        }
    }

    public abstract class ProcessFile : PartOfOperation
    {
        protected int Length;
        protected int Counter;

        public string Source { get; set; }

        public ProcessFile(CancellationToken cancellationToken, IProgress<ProgressInformation> progress, string source)
            : base(cancellationToken, progress)
        {
            Source = source;
        }

        public async override Task RunAsync()
        {
            var info = new FileInfo(Source);
            Length = await DetermineLines();


            Func<TextReader> sourceRetriever = () => new StreamReader(Source, Encoding.Default);

            await ProcessFileStream(sourceRetriever);
        }

        private Task<int> DetermineLines()
        {
            return Task.Run(() => File.ReadLines(Source).Count());
        }

        protected abstract Task ProcessFileStream(Func<TextReader> sourceRetriever);

        protected Task<string> ReadLine(TextReader reader)
        {
            Counter++;
            //Report("", Counter, Length);
            return reader.ReadLineAsync();
        }
    }

    public class ImdbProcessMovies : ProcessFile
    {
        protected string CurrentLine = string.Empty;

        public ImdbProcessMovies(CancellationToken cancellationToken, IProgress<ProgressInformation> progress, string source)
            : base(cancellationToken, progress, source)
        {
        }

        protected async override Task ProcessFileStream(Func<TextReader> sourceRetriever)
        {
            using (var res = sourceRetriever.Invoke())
            {
                await TraverseToStart(res);

                while (res.Peek() > -1)
                {
                    var result = await GetItem(res);
                    Report(string.Format("{0}->{1}", result.Name, string.Join("-", result.Episodes.Count)), Counter, Length);

                    if (Counter != 0 && Counter % 250 == 0)
                        await Task.Delay(1);

                }

            }
        }

        private async Task<ImdbMovie> GetItem(TextReader res)
        {
            if (string.IsNullOrWhiteSpace(CurrentLine))
                CurrentLine = await res.ReadLineAsync();
            
            while (string.IsNullOrWhiteSpace(CurrentLine))
            {
                CurrentLine = await ReadLine(res);
            }

            return await GetImdbMovieWithSubsets(res);


        }

        private async Task<ImdbMovie> GetImdbMovieWithSubsets(TextReader res)
        {
            try
            {
                var result = new ImdbMovie();

                do
                {
                    var startParanthesis = CurrentLine.IndexOf('(');
                    var endParanthesis = CurrentLine.IndexOf(')');

                    result.Name = CurrentLine.Substring(0, startParanthesis).TrimEnd();
                    result.Episodes.Add("dummy");
                    CurrentLine = await ReadLine(res);

                } while (CurrentLine.StartsWith(result.Name));

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task TraverseToStart(TextReader res)
        {
            int counter = 1;

            while (counter < 15)
            {
                await ReadLine(res);
                counter++;
            }
        }
    }


    //public class ImdbProcessAlias : ProcessFile
    //{
    //    public ImdbProcessAlias(CancellationToken cancellationToken, IProgress<ProgressInformation> progress, string source)
    //        : base(cancellationToken, progress, source)
    //    {
    //    }

    //    protected async override Task ProcessFileStream(Func<TextReader> sourceRetriever)
    //    {
    //        using (var res = sourceRetriever.Invoke())
    //        {
    //            await TraverseToStart(res);

    //            while (res.Peek() > -1)
    //            {
    //                var result = await GetItem(res);
    //                Report(string.Format("{0}->{1}", result.Name, string.Join("-", result.Aliases)), Counter, Length);

    //                if (Counter != 0 && Counter % 250 == 0)
    //                    await Task.Delay(1);

    //            }
                
    //        }
    //    }

    //    private async Task<AliasName> GetItem(TextReader res)
    //    {
    //        var result = await res.ReadLineAsync();
    //        while (string.IsNullOrWhiteSpace(result))
    //        {
    //            result = await ReadLine(res);
    //        }



    //        string movie = result;
    //        var akas = new List<string>();

    //        result = await ReadLine(res);
    //        do
    //        {
    //            var startParanthesis = result.IndexOf('(');
    //            var endParanthesis = result.IndexOf(')');

    //            var startBracket = result.IndexOf('{');
    //            var endBracket = result.IndexOf('}');

    //            akas.Add(result.Replace("(aka ", string.Empty).Trim(')'));
    //            result = await ReadLine(res);

    //        } while (!string.IsNullOrWhiteSpace(result));

    //        return new AliasName(name, akas);

    //    }

    //    private async Task TraverseToStart(TextReader res)
    //    {
    //        int counter = 1;

    //        while (counter < 18)
    //        {
    //            await ReadLine(res);
    //            counter++;
    //        }
    //    }
    // }

}