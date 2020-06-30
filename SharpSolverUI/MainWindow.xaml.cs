using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Win32;
using SharpSolver;

namespace SharpSolverUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string SourceLink = "https://github.com/spmadeira/SharpSolver";
        
        public static readonly ICommand NewCommand = new RoutedCommand();
        public static readonly ICommand SaveCommand = new RoutedCommand();
        public static readonly ICommand LoadCommand = new RoutedCommand();
        public static readonly ICommand ExitCommand = new RoutedCommand();
        public static readonly ICommand SolveCommand = new RoutedCommand();
        public static readonly ICommand SourceCommand = new RoutedCommand();

        private readonly Dictionary<ICommand, Action> CommandFunction;

        public bool IsDirty = false;

        public MainWindow()
        {
            InitializeComponent();
            CommandFunction = new Dictionary<ICommand, Action>
            {
                {NewCommand, New},
                {SaveCommand, Save},
                {LoadCommand, Load},
                {ExitCommand, Exit},
                {SolveCommand, Solve},
                {SourceCommand, Source}
            };
        }
        
        private void Solve()
        {
            var text = TextBox.Text;
            if (string.IsNullOrWhiteSpace(text)) return;
            try
            {
                // var result = LinearSolver.Solve(text);
                // MessageBox.Show(result.Message, "Result", MessageBoxButton.OK);
                var result = LinearModelSolver.Solve(LinearParser.Parse(text));
                var resultWindow = new LinearResultWindow(result);
                resultWindow.ShowDialog();
            }
            catch (InvalidInputException iie)
            {
                MessageBox.Show(iie.Message + $"\n Error Line: {iie.ParsedInput[iie.ErrorLine]}", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var hasFunction = CommandFunction.TryGetValue(e.Command, out var function);

            if (hasFunction)
                function();
        }

        private void New()
        {
            var operationCancelled = PreventLossOfData();
            if (operationCancelled) return;

            TextBox.Text = "";
        }

        private void Save()
        {
            var sfd = new SaveFileDialog {Filter = "Text files (*.txt)|*.txt", InitialDirectory = Environment.CurrentDirectory};
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, TextBox.Text);
            }
            IsDirty = false;
        }

        private void Load()
        {
            var operationCancelled = PreventLossOfData();
            if (operationCancelled) return;
            
            var ofd = new OpenFileDialog{Filter = "Text files (*.txt)|*.txt", InitialDirectory = Environment.CurrentDirectory};
            if (ofd.ShowDialog() == true)
            {
                TextBox.Text = File.ReadAllText(ofd.FileName);
            }
            IsDirty = false;
        }

        private void Exit()
        {
            var operationCancelled = PreventLossOfData();
            if (operationCancelled) return;
            
            Application.Current.Shutdown();
        }

        private void Source()
        {
            var psi = new ProcessStartInfo("cmd", $"/c start {SourceLink}")
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };
            Process.Start(psi);
        }
        
        private bool PreventLossOfData()
        {
            var text = TextBox.Text;
            if (string.IsNullOrWhiteSpace(text) || !IsDirty)
                return false;

            var messageBoxResult = MessageBox.Show(
                "You have unsaved changes to the current model, would you like to save them?",
                "Save",
            MessageBoxButton.YesNoCancel);

            switch (messageBoxResult)
            {
                case MessageBoxResult.Yes:
                    Save();
                    return false;
                case MessageBoxResult.No:
                    return false;
                case MessageBoxResult.Cancel:
                    return true;
                default:
                    throw new ArgumentException(messageBoxResult.ToString());
            }
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e) => IsDirty = true;
    }
}
