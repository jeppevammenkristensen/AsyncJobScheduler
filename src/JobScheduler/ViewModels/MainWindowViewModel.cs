using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using JobScheduler.Model;

namespace JobScheduler.ViewModels
{
    public class PropertyChangeBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MainWindowViewModel : PropertyChangeBase
    {
        private CancellationTokenSource source;

        private bool _isActive;
        private float _percentage;
        private bool _canCancel;
        private string _job;

        public string Job
        {
            get { return _job; }
            set { _job = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value;  OnPropertyChanged();}
        }

        public async Task StartJobAsync()
        {
            if (source != null)
                return;

            source = new CancellationTokenSource();
            source.Token.ThrowIfCancellationRequested();
            OnPropertyChanged("CanCancel");

            Message = new ObservableCollection<string>();
            var progress = new Progress<ProgressInformation>();
            progress.ProgressChanged += (o, e) =>
                                                  {
                                                      Percentage = e.ProgressPercentage;
                                                      UpdateInformation(e.Information);
                                                  };

            //var operation = new FtpDownloadFileOperation(source.Token, progress,
            //                                          "ftp://ftp.sunet.se/pub/tv+movies/imdb/movies.list.gz",
            //@"c:\temp\downloadtempdownload.gz");
            //Job = "Job 1";
            //await operation.RunAsync();



            //var anotherOperation = new GZipUnzipOperation(source.Token, progress, @"c:\temp\downloadtempdownload.gz", @"c:\temp\fol.txt");
            //Job = "Job 2";
            //await anotherOperation.RunAsync();

            //var thirdOperation = new CopyFileOperation(source.Token, progress, anotherOperation.Destination,
            //                                            @"c:\temp\fol_copy.txt");
            //Job = "Job 3";
            //await thirdOperation.RunAsync();


            var forthOperation = new ImdbProcessMovies(source.Token, progress, @"c:\temp\fol_copy.txt");
            Job = "Extracting movies";
            await forthOperation.RunAsync();


            source = null;
            OnPropertyChanged("CanCancel");

        }

        public bool CanCancel
        {
            get { return source != null; }
        }

        private void UpdateInformation(string information)
        {
            if (String.IsNullOrWhiteSpace(information))
                return;

            Message.Add(information);
            if (Message.Count > 10 )
                Message.RemoveAt(0);

            OnPropertyChanged("Message");
        }

        public ObservableCollection<string> Message { get; set; }

        

        public float Percentage
        {
            get { return _percentage; }
            set { _percentage = value; OnPropertyChanged(); OnPropertyChanged("ProgessColor");}
        }

        public SolidColorBrush ProgessColor
        {
            get
            {
                var result = (byte) Percentage;
                if (result == 0)
                    return new SolidColorBrush(Colors.NavajoWhite);

                var red = Colors.Purple;
                var green = Colors.Green;

                var rSum = (byte)((green.R - red.R) * Percentage / 100);
                var gDifference = (byte)((green.G - red.G) * Percentage /100);
                var bDiffernce = (byte)((green.B - red.B) * Percentage / 100);

                return new SolidColorBrush(Color.FromRgb(rSum, gDifference, bDiffernce));
            }
        }

        public void Cancel()
        {
            if (source != null)
            {
                source.Cancel();
                OnPropertyChanged("CanCancel");
            }
        }
    }
}