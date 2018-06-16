﻿using System;
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
using BranchPredictionSim.Exceptions;
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
            //notifier = new IBranchNotifier
        }

        private Executor executor;
        private string filename;
        private int lastHighlighted = -1;

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StopButton.IsEnabled = true;
            RunButton.IsEnabled = true;
            StepButton.IsEnabled = true;
            ChooseFile.IsEnabled = false;
            AsmCode.IsEnabled = false;
            StartButton.IsEnabled = false;
            PredictorType.IsEnabled = false;
            FakeAsm.Visibility = Visibility.Visible;
            AsmCode.Visibility = Visibility.Hidden;
            lastHighlighted = -1;

            var codeLines = AsmCode.Text.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            //fill fakeasm
            FakeAsm.Inlines.Clear();
            foreach (var codeLine in codeLines)
            {
                FakeAsm.Inlines.Add(codeLine + Environment.NewLine);
            }

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
            LabelTable.ItemsSource = executor.FakeLabelDict;
            //Update_Stats(executor);
            UpdateResults();
            //}
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                executor.RunProgram(int.Parse(MaxIter.Text));
            } catch (EndOfCodeException)
            {
                MessageBoxResult result = MessageBox.Show("Достигнут конец кода",
                          "Конец кода",
                          MessageBoxButton.OK);
            } catch (FormatException ex)
            {
                MessageBoxResult result = MessageBox.Show(ex.ToString() + "\nСтрока " + executor.currentLineNum,
                          "Ошибка параметра",
                          MessageBoxButton.OK);
            }
            UpdateResults();
            //Update_Stats(executor);
        }
        // todo: выводить свойство executor.stats в datagrid с апдейтами
        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            if (lastHighlighted >= 0)
                FakeAsm.Inlines.ElementAt(lastHighlighted).Background = new SolidColorBrush(Color.FromArgb(0 ,128, 128, 64));
            try
            {
                FakeAsm.Inlines.ElementAt(executor.currentLineNum).Background = new SolidColorBrush(Color.FromRgb(255, 0, 255));
                lastHighlighted = executor.currentLineNum;
                executor.Step();
            }
            catch (EndOfCodeException)
            {
                MessageBoxResult result = MessageBox.Show("Достигнут конец кода",
                                          "Конец кода",
                                          MessageBoxButton.OK);
            }
            catch (FormatException ex)
            {
                MessageBoxResult result = MessageBox.Show(ex.Message + "\nСтрока " + executor.currentLineNum,
                          "Ошибка параметра",
                          MessageBoxButton.OK);
            }
            UpdateResults();
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
            //registers
            RegistersFlags.ItemsSource = executor.regDict;
            RegistersFlags.Items.Refresh();

            //flags
            Flags.ItemsSource = executor.flagDict;
            Flags.Items.Refresh();

            //stack
            StackValues.ItemsSource = executor.stack.ToList();
            StackValues.Items.Refresh();

            //eip
            EIPValues.ItemsSource = executor.FakeEIP.ToList();
            EIPValues.Items.Refresh();

            //predictions
            PredictionHistory.ItemsSource = executor.predictorStats;
            PredictionHistory.Items.Refresh();


            //prediction stats
            int successPredictions = 0;
            int predictionsCount = executor.predictorStats.Count;
            foreach (var singlePrediction in executor.predictorStats)
            {
                if (singlePrediction.Value.Key == singlePrediction.Value.Value)
                    successPredictions++;
            }
            PredictionStats.Text = "Статистика предсказаний: " 
                + (successPredictions/(double) predictionsCount) * 100 
                + "% успешных предсказаний";

            //nextCommandAddr
            NextCmdAddr.Text = string.Format("Адрес следующей команды: {0:X}", executor.Adresses[executor.currentLineNum]);
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
            executor = null;
            UpdateResults();
            AsmCode.IsEnabled = true;
            PredictorType.IsEnabled = true;
            RunButton.IsEnabled = false;
            StepButton.IsEnabled = false;
            ChooseFile.IsEnabled = true;
            StopButton.IsEnabled = false;
            StartButton.IsEnabled = true;
            FakeAsm.Visibility = Visibility.Hidden;
            AsmCode.Visibility = Visibility.Visible;
        }

        private void AsmCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AsmCode.Text.Equals(""))
            {
                StartButton.IsEnabled = false;
            }
            else StartButton.IsEnabled = true;
        }
    }
}
