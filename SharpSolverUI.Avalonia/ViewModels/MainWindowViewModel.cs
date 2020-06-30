using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using MsgBox;
using ReactiveUI;
using SharpSolver;
using SharpSolverUI.Avalonia.Views;

namespace SharpSolverUI.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string SourceLink = "https://github.com/spmadeira/SharpSolver";
        
        public Window MainWindow { get; set; }
        public string Text { get; set; }
        public TextBox TextBox { get; set; }
        
        public IReactiveCommand NewCommand { get; }
        public IReactiveCommand LoadCommand { get; }
        public IReactiveCommand SaveCommand { get; }
        public IReactiveCommand ExitCommand { get; }
        public IReactiveCommand SolveCommand { get; }
        public IReactiveCommand SourceCommand { get; }
        
        public MainWindowViewModel()
        {
            NewCommand = ReactiveCommand.Create(New);
            LoadCommand = ReactiveCommand.CreateFromTask(Load);
            SaveCommand = ReactiveCommand.CreateFromTask(Save);
            ExitCommand = ReactiveCommand.Create(Exit);
            SolveCommand = ReactiveCommand.CreateFromTask(Solve);
            SourceCommand = ReactiveCommand.Create(Source);
        }
        
        public async Task Solve()
        {
            if (string.IsNullOrWhiteSpace(Text)) return;
            try
            {
                var result = LinearModelSolver.Solve(LinearParser.Parse(Text));
                var resultWindow = new SolvedView(MainWindow, result){DataContext = new SolvedViewModel()};
                await resultWindow.ShowDialog(MainWindow);
            }
            catch (InvalidInputException iie)
            {
                await MessageBox.Show(MainWindow, iie.Message + $"\n Error Line: {iie.ParsedInput[iie.ErrorLine]}", "Input Error", MessageBox.MessageBoxButtons.Ok);
            }
            catch (Exception ex)
            {
                await MessageBox.Show(MainWindow, ex.Message, "Unknown Error", MessageBox.MessageBoxButtons.Ok);
            }
        }

        private void New()
        {
            Text = "";
            TextBox.Text = "";
        }

        private async Task Load()
        {
            var ofd = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter>(new[]
                {
                    new FileDialogFilter
                    {
                        Name = "Text files (*.txt)",
                        Extensions = new List<string>(new[] {"txt"})
                    }
                }),
                AllowMultiple = false
            };

            var file = await ofd.ShowAsync(MainWindow);

            if (file.Any() && !string.IsNullOrEmpty(file.First()))
            {
                var text = await File.ReadAllTextAsync(file.First());
                Text = text;
                TextBox.Text = text;
            }
        }
        
        private async Task Save()
        {
            var sfd = new SaveFileDialog
            {
                Filters = new List<FileDialogFilter>(new []
                {
                    new FileDialogFilter
                    {
                        Name = "Text files (*.txt)",
                        Extensions = new List<string>(new []{"txt"})
                    }
                }),
                InitialFileName = "model.txt"
            };
            var result = await sfd.ShowAsync(MainWindow);

            if (!string.IsNullOrEmpty(result))
            {
                await File.WriteAllTextAsync(result, Text);
            }
        }

        private void Exit()
        {
            MainWindow.Close();
        }

        private void Source()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var psi = new ProcessStartInfo("cmd", $"/c start {SourceLink}")
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                Process.Start(psi);
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                       || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                Process.Start("xdg-open", SourceLink);
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", SourceLink);
            }
        }
    }
}
