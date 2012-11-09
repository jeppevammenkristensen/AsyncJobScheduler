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

            Message = new ObservableCollection<string>();
            var progress = new Progress<ProgressInformation>();
            progress.ProgressChanged += (o, e) =>
                                                  {
                                                      Percentage = e.ProgressPercentage;
                                                      UpdateInformation(e.Information);
                                                  };

            var operation = new FtpDownloadFileOperation(source.Token, progress,
                                                      "ftp://ftp.sunet.se/pub/tv+movies/imdb/directors.list.gz",
            @"c:\temp\downloadtempdownload.gz");
            await operation.RunAsync();

            var anotherOperation = new GZipUnzipOperation(source.Token, progress, operation.Destination, @"c:\temp\fol");
            await anotherOperation.RunAsync();

        }

        private void UpdateInformation(string information)
        {
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
            source.Cancel();
            source = null;
        }
    }
}