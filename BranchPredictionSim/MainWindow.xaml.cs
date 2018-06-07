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
            this.DataContext = this;
            InitializeComponent();
            AsmCodeFile.IsEnabled = false;

        }

        private Executor executor;
        private string filename;

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

            RunButton.IsEnabled = true;
            StepButton.IsEnabled = true;
            ChooseFile.IsEnabled = false;
            AsmCode.IsEnabled = false;

            var codeLines = AsmCode.Text.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

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
                    throw new Exception("Error!");
                    break;
            }

            //foreach(var label in executor.labelDict)
            //{
            LabelTable.ItemsSource = executor.labelDict;
            Update_Stats(executor);
            //}
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            executor.RunProgram();
            UpdateResults();
            Update_Stats(executor);
        }
        // todo: выводить свойство executor.stats в datagrid с апдейтами
        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(LineNumJump.Text, out int jumpLine) && jumpLine >= 0)
                executor.Step(ref jumpLine);
            else
                executor.Step();
            UpdateResults();
            Update_Stats(executor);
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

        private void ClearResults()
        {

        }
            private void UpdateResults()
        {
            if (executor == null)
            {
                ClearResults();
                return;
            }

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


            var codeLines = File.ReadAllLines(filename);
            AsmCode.Text = String.Join("\n", codeLines);
            AsmCodeFile.Text = filename;

            
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateResults();
            AsmCode.IsEnabled = true;
            RunButton.IsEnabled = false;
            StepButton.IsEnabled = false;
            ChooseFile.IsEnabled = true;
        }

        private void AsmCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AsmCode.Text.Equals(""))
            {
                StartButton.IsEnabled = false;
            }
            else StartButton.IsEnabled = true;
        }

        private void Update_Stats(Executor executor)
        {
            regEax.Text = executor.stats[0].regFlag;
            valueEax.Text = executor.stats[0].value.ToString();
            regEbx.Text = executor.stats[1].regFlag;
            valueEbx.Text = executor.stats[1].value.ToString();
            regEcx.Text = executor.stats[2].regFlag;
            valueEcx.Text = executor.stats[2].value.ToString();
            regEdx.Text = executor.stats[3].regFlag;
            valueEdx.Text = executor.stats[3].value.ToString();
            ZF.Text = executor.stats[4].regFlag;
            valueZF.Text = executor.stats[4].value.ToString();
            SF.Text = executor.stats[5].regFlag;
            valueSF.Text = executor.stats[5].value.ToString();
            PF.Text = executor.stats[6].regFlag;
            valuePF.Text = executor.stats[6].value.ToString();
        }
    }
}
