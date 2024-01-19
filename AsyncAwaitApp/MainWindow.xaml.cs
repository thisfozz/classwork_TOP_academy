using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AsyncAwaitApp
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cts;
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private bool IsPrime(int number)
        {
            if (number < 2)
            {
                return false;
            }

            for (int i = 2; i <= Math.Sqrt(number); ++i)
            {
                if (number % i == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<int> CountPrimes(int from, int to, CancellationToken cancellationToken, IProgress<double> p)
        {
            int countNumbers = 0;
            int pcount = 0;
            int range = to - from;

            await Task.Run(() =>
            {
                for (int i = from; i <= to; ++i)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (IsPrime(i))
                    {
                        countNumbers++;
                    }
                    if(pcount++ % 100 == 0)
                    {
                        p.Report(pcount * 100.0 / range);
                    }
                }
            });

            return countNumbers;
        }   

        private Task<int> CountPrimesAsync(int from, int to, CancellationToken cancellationToken, Progress<double> p)
        {
            return Task.Run(() => CountPrimes(from, to, cancellationToken, p));
        }

        private async void calcButton_Click(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            calcButton.IsEnabled = false;
            cancelButton.IsEnabled = true;
            Result.Text = "Идут вычисления...";

            int first = int.Parse(From.Text);
            int last = int.Parse(To.Text);
            var p = new Progress<double>(value => progress.Value = value);

            try
            {
                var count = await CountPrimesAsync(first, last, cts.Token, p);
                Result.Text = $"Простых чисел: {count}";
            }
            catch (OperationCanceledException)
            {
                Result.Text = "Вычисления отменены";
            }
            finally
            {
                cts?.Dispose();
                calcButton.IsEnabled = true;
                cancelButton.IsEnabled = false;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }
    }
}