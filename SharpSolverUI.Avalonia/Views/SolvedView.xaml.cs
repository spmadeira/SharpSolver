using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using SharpSolver;
using SharpSolverUI.Avalonia.ViewModels;

namespace SharpSolverUI.Avalonia.Views
{
    public class SolvedView : Window
    {
        public SolvedView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        
        public SolvedView(WindowBase owner, SolvedModel model)
        {
            Owner = owner;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContextChanged += (sender, args) =>
            {
                var viewModel = DataContext as SolvedViewModel;
                viewModel.Model = model;
                viewModel.Resources = Resources;
                viewModel.MainWindow = this;
                viewModel.PreviousPageCommand = ReactiveCommand.Create(viewModel.PreviousPage);
                viewModel.NextPageCommand = ReactiveCommand.CreateFromTask(viewModel.NextPage);
                viewModel.Grid = this.FindControl<Grid>("Grid");
                viewModel.BackButton = this.FindControl<Button>("BackButton");
                viewModel.ForwardButton = this.FindControl<Button>("ForwardButton");
                var twoPhasesPages = viewModel.Model.TwoPhasesIterations
                    .Select(i => new SolvedViewModel.SolverIterationPage
                        {Iteration = i, Method = SolvedViewModel.SolverMethod.TwoPhases});
            
                var simplexPages = viewModel.Model.SimplexIterations
                    .Select(i => new SolvedViewModel.SolverIterationPage
                        {Iteration = i, Method = SolvedViewModel.SolverMethod.Simplex});
            
                viewModel.SolverIterationPages = twoPhasesPages.Concat(simplexPages).ToList();
                viewModel.BuildGrid(viewModel.SolverIterationPages[0].Iteration);
                viewModel.CurrentPageIndex = 0;
                viewModel.LoadPage(viewModel.CurrentPageIndex);
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
