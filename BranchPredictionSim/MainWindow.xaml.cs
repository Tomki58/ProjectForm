using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BranchPredictionSim.Predictors;

namespace BranchPredictionSim
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AsmCode.IsEnabled = false;
        }

        private Executor executor;
        private string filename;

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //todo clear stats
            RunButton.IsEnabled = true;
            StepButton.IsEnabled = true;
            var codeLines = File.ReadAllLines(filename);
            switch (PredictorType.SelectedIndex)
            {
                case 0:
                    executor = new Executor(codeLines, new AlwaysPredictJump());
                    break;
                case 1:
                    executor = new Executor(codeLines, new BTFNT());
                    break;
                case 2:
                    executor = new Executor(codeLines, new NBitPredictor(1));
                    break;
                case 3:
                    executor = new Executor(codeLines, new NBitPredictor(2));
                    break;
                case 4:
                    executor = new Executor(codeLines, new NBitPredictor(int.Parse(Params.Text)));
                    break;
                default:
                    throw new Exception("Это ваще как кек");
                    break;
            }
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            executor.RunProgram();
            UpdateResults(executor);
        }

        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(LineNumJump.Text, out int jumpLine) && jumpLine >= 0)
                executor.Step(ref jumpLine);
            else
                executor.Step();
            UpdateResults(executor);
        }

        private void PredictorType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Params == null)
                return;
            if (PredictorType.SelectedIndex == 4)
                Params.IsEnabled = true;
            else
                Params.IsEnabled = false;
        }

        private void UpdateResults(Executor executor)
        {
            int successPredictions = 0;
            int predictionsCount = 0;
            foreach (var linePredictions in executor.predictorStats)
            {
                foreach (var predictVsReal in linePredictions.Value)
                {
                    predictionsCount++;
                    if (predictVsReal.Key == predictVsReal.Value)
                        successPredictions++;
                }
            }
            PredictionStats.Text = "Статистика предсказаний: " 
                + (successPredictions/(double) predictionsCount) * 100 
                + "% успешных предсказаний";
        }

        // Open file
        private void ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document";
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";

            Nullable<bool> result = dlg.ShowDialog(this);
            if (result == true)
            {
                filename = dlg.FileName;
            }

            AsmCode.Text = filename;
        }
    }
}
