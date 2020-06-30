using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MsgBox;
using Numerics;
using ReactiveUI;
using SharpSolver;

namespace SharpSolverUI.Avalonia.ViewModels
{
    public class SolvedViewModel : ViewModelBase
    {
        public IResourceDictionary Resources { get; set; }
        public Window MainWindow { get; set; }
        public Grid Grid { get; set; }
        public Button BackButton { get; set; }
        public Button ForwardButton { get; set; }

        public IReactiveCommand PreviousPageCommand { get; set; }
        public IReactiveCommand NextPageCommand { get; set; }
        
        public int CurrentPageIndex = 0;
        public string MethodText { get; set; }
        public string ObjectiveFunctionText { get; set; }
        public string IterationText { get; set; }
        
        public List<SolverIterationPage> SolverIterationPages = 
            new List<SolverIterationPage>();
        
        public SolvedModel Model;
        
        public TextBlock Tableau;
        public TextBlock[] Variables;
        public TextBlock[] ObjectiveRows;
        public TextBlock[][] VariableGrid;
        public TextBlock[] DecisionVariables;
        public TextBlock Z;
        public TextBlock[] Bases;
        public TextBlock[] BaseVariables;
        
        public Rectangle[][] Rectangles;

        public void BuildGrid(SolverIteration iterationBase)
        {
            //Set grid alignment
            Grid.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.VerticalAlignment = VerticalAlignment.Top;
            
            //Clear previous grid info
            Grid.Children.Clear();
            Grid.ColumnDefinitions.Clear();
            Grid.RowDefinitions.Clear();

            //Default columns
            var baseColumn = new ColumnDefinition();
            Grid.ColumnDefinitions.Add(baseColumn);
            var objectiveColumn = new ColumnDefinition();
            Grid.ColumnDefinitions.Add(objectiveColumn);

            foreach (var _ in iterationBase.Variables)
            {
                Grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            var objectiveRow = new RowDefinition();
            Grid.RowDefinitions.Add(objectiveRow);
            var nameRow = new RowDefinition();
            Grid.RowDefinitions.Add(nameRow);
            foreach (var _ in Model.Constraints)
            {
                Grid.RowDefinitions.Add(new RowDefinition());
            }
            var decisionRow = new RowDefinition();
            Grid.RowDefinitions.Add(decisionRow);

            //Size rows and columns
            var init = Grid.IsInitialized;
            
            //var gridWidth = 774d;
            var gridWidth = Grid.Width;
            var cellWidth = gridWidth / Grid.ColumnDefinitions.Count;
            
            //var gridHeight = 375.04d;
            var gridHeight = Grid.Height;
            var cellHeight = gridHeight / Grid.RowDefinitions.Count;
            
            foreach (var columnDefinition in Grid.ColumnDefinitions)
            {
                columnDefinition.Width = new GridLength(cellWidth);
            }
            
            foreach (var rowDefinition in Grid.RowDefinitions)
            {
                rowDefinition.Height = new GridLength(cellHeight);
            }

            //Build grid rectangles
            var rects = new List<List<Rectangle>>();
            for (int i = 0; i < Grid.RowDefinitions.Count; i++)
            {
                var rectR = new List<Rectangle>();
                for (int j = 0; j < Grid.ColumnDefinitions.Count; j++)
                {
                    var rect = new Rectangle{Stroke = Brushes.Black, Fill = Brushes.Transparent, };
                    var border = new Border{ BorderBrush = Brushes.Black, BorderThickness = new Thickness(1)};
                    rectR.Add(rect);
                    Grid.SetRow(rect,i);
                    Grid.SetRow(border,i);
                    Grid.SetColumn(rect,j);
                    Grid.SetColumn(border, j);
                    Grid.Children.Add(rect);
                    Grid.Children.Add(border);
                }
                rects.Add(rectR);
            }
            Rectangles = rects.Select(r => r.ToArray()).ToArray();

            //Build default grid blocks
            Tableau = new TextBlock
            {
                Text = "Tableau",
                FontWeight = FontWeight.Bold, 
                VerticalAlignment = VerticalAlignment.Center, 
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Rectangles[0][0].Fill = new SolidColorBrush(Colors.CadetBlue);
            Grid.SetRow(Tableau, 0);
            Grid.SetColumn(Tableau, 0);
            Grid.Children.Add(Tableau);

            var baseBlock = new TextBlock
            {
                Text = "Base",
                FontWeight = FontWeight.Bold, 
                VerticalAlignment = VerticalAlignment.Center, 
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Rectangles[1][0].Fill = new SolidColorBrush(Colors.CadetBlue);
            Grid.SetRow(baseBlock, 1);
            Grid.SetColumn(baseBlock, 0);
            Grid.Children.Add(baseBlock);
            
            var b = new TextBlock
            {
                Text = "b",
                FontWeight = FontWeight.Bold, 
                VerticalAlignment = VerticalAlignment.Center, 
                HorizontalAlignment = HorizontalAlignment.Center
            };;
            Rectangles[1][1].Fill = new SolidColorBrush(Colors.CadetBlue);
            Grid.SetRow(b, 1);
            Grid.SetColumn(b, 1);
            Grid.Children.Add(b);
            
            var z = new TextBlock
            {
                Text = "Z",
                FontWeight = FontWeight.Bold, 
                VerticalAlignment = VerticalAlignment.Center, 
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Rectangles[Grid.RowDefinitions.Count-1][0].Fill = new SolidColorBrush(Colors.CadetBlue);
            Grid.SetRow(z, Grid.RowDefinitions.Count-1);
            Grid.SetColumn(z, 0);
            Grid.Children.Add(z);
            
            Z = new TextBlock
            {
                Text = "0",
                FontWeight = FontWeight.Bold, 
                VerticalAlignment = VerticalAlignment.Center, 
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(Z, Grid.RowDefinitions.Count-1);
            Grid.SetColumn(Z, 1);
            Grid.Children.Add(Z);

            var objectiveBlocks = new List<TextBlock>();
            for (int i = 2; i < Grid.ColumnDefinitions.Count; i++)
            {
                var block = new TextBlock{
                    Text = $"X{i-1}",
                    VerticalAlignment = VerticalAlignment.Center, 
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                objectiveBlocks.Add(block);
                Grid.SetRow(block,0);
                Grid.SetColumn(block, i);
                Grid.Children.Add(block);
            }

            ObjectiveRows = objectiveBlocks.ToArray();
            
            var variableBlocks = new List<TextBlock>();
            for (int i = 2; i < Grid.ColumnDefinitions.Count; i++)
            {
                var block = new TextBlock{
                    Text = $"X{i-1}",
                    FontWeight = FontWeight.Bold, 
                    VerticalAlignment = VerticalAlignment.Center, 
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                variableBlocks.Add(block);
                Rectangles[1][i].Fill = Brushes.CadetBlue;
                Grid.SetRow(block, 1);
                Grid.SetColumn(block, i);
                Grid.Children.Add(block);
            }
            Variables = variableBlocks.ToArray();
            
            var baseBlocks = new List< TextBlock>();
            for (int i = 2; i < Grid.RowDefinitions.Count-1; i++)
            {
                var block = new TextBlock{
                    Text = $"X{i-1}",
                    VerticalAlignment = VerticalAlignment.Center, 
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                baseBlocks.Add(block);
                Grid.SetRow(block,i);
                Grid.SetColumn(block,0);
                Grid.Children.Add(block);
            }
            Bases = baseBlocks.ToArray();

            var baseVarBlocks = new List<TextBlock>();
            for (int i = 2; i < Grid.RowDefinitions.Count-1; i++)
            {
                var block = new TextBlock{
                    Text = $"0",
                    VerticalAlignment = VerticalAlignment.Center, 
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                baseVarBlocks.Add(block);
                Grid.SetRow(block,i);
                Grid.SetColumn(block,1);
                Grid.Children.Add(block);
            }

            BaseVariables = baseVarBlocks.ToArray();

            var varGrid = new List<List<TextBlock>>();
            for (int i = 2; i < Grid.RowDefinitions.Count-1; i++)
            {
                var varRow = new List<TextBlock>();
                for (int j = 2; j < Grid.ColumnDefinitions.Count; j++)
                {
                    var block = new TextBlock
                    {
                        Text = $"0",
                        VerticalAlignment = VerticalAlignment.Center, 
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    varRow.Add(block);
                    Grid.SetRow(block,i);
                    Grid.SetColumn(block,j);
                    Grid.Children.Add(block);
                }
                varGrid.Add(varRow);
            }

            VariableGrid = varGrid.Select(r => r.ToArray()).ToArray();

            var decisionVars = new List<TextBlock>();
            for (int i = 2; i < Grid.ColumnDefinitions.Count; i++)
            {
                var block = new TextBlock
                {
                    Text = $"0",
                    VerticalAlignment = VerticalAlignment.Center, 
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                decisionVars.Add(block);
                Grid.SetColumn(block,i);
                Grid.SetRow(block,Grid.RowDefinitions.Count-1);
                Grid.Children.Add(block);
            }
            DecisionVariables = decisionVars.ToArray();
        }
        
        public void LoadPage(int index)
        {
            var page = SolverIterationPages[index];

            if (Grid.ColumnDefinitions.Count != page.Iteration.Variables.Length + 2 ||
                Grid.RowDefinitions.Count != page.Iteration.Bases.Length + 3)
            {
                Console.WriteLine("Rebuilding Grid");
                BuildGrid(page.Iteration);
            }

            MethodText = page.Method.ToString();
            var objectiveText = "";
            foreach (var objectiveVariable in Model.Objective)
            {
                //Don't show vars with coef = 0
                if (objectiveVariable.Coefficient > 0)
                {
                    objectiveText += $"+ {objectiveVariable.Coefficient}{objectiveVariable.Variable.Name}";
                } else if (objectiveVariable.Coefficient < 0)
                {
                    objectiveText += $"- {objectiveVariable.Coefficient}{objectiveVariable.Variable.Name}";
                }
            }
            if (objectiveText.StartsWith("+"))
                objectiveText = new string(objectiveText.Skip(2).ToArray());
            
            ObjectiveFunctionText = $"{Model.ObjectiveOrientation.ToString()} Z: {objectiveText}";
            IterationText = $"{index+1}";

            Tableau.Text = $"Tableau {index+1}";

            for (int i = 0; i < page.Iteration.Variables.Length; i++)
            {
                Variables[i].Text = page.Iteration.Variables[i].Name;
                ObjectiveRows[i].Text = page.Iteration.ObjectiveVariables[i].ToFractionString();
                DecisionVariables[i].Text = page.Iteration.DecisionVariables[i].ToFractionString();
            }

            for (int i = 0; i < page.Iteration.Bases.Length; i++)
            {
                Bases[i].Text = page.Iteration.Bases[i].Name;
                BaseVariables[i].Text = page.Iteration.BaseVariables[i].ToFractionString();
            }

            for (int i = 0; i < page.Iteration.Bases.Length; i++)
            {
                for (int j = 0; j < page.Iteration.Variables.Length; j++)
                {
                    VariableGrid[i][j].Text = page.Iteration.InputGrid[i][j].ToFractionString();
                }
            }

            Z.Text = page.Iteration.Z.ToFractionString();

            if (index <= 0)
            {
                BackButton.Content = Resources["BackArrowDisabled"];
                BackButton.IsEnabled = false;
            }
            else
            {
                BackButton.Content = Resources["BackArrowEnabled"];
                BackButton.IsEnabled = true;
            }

            ForwardButton.IsEnabled = true;
            
            if (index >= SolverIterationPages.Count - 1)
            {
                ForwardButton.Content = Resources["PageResult"];
            }
            else
            {
                ForwardButton.Content = Resources["ForwardArrowEnabled"];
            }

            for (int i = 0; i < VariableGrid.Length; i++)
            {
                for (int j = 0; j < VariableGrid[0].Length; j++)
                {
                    Rectangles[i + 2][j + 2].Fill = null;
                    VariableGrid[i][j].FontWeight = FontWeight.Normal;
                }
            }
            
            if (page.Iteration.SelectedGridPositions != (-1, -1))
            {
                var pos = page.Iteration.SelectedGridPositions;
                Rectangles[pos.x+2][pos.y+2].Fill = Brushes.Chartreuse;
                VariableGrid[pos.x][pos.y].FontWeight = FontWeight.Bold;
            }
        }

        public void PreviousPage()
        {
            if (CurrentPageIndex > 0)
            {
                CurrentPageIndex--;
                LoadPage(CurrentPageIndex);
            }
        }
        
        public async Task NextPage()
        {
            if (CurrentPageIndex < SolverIterationPages.Count - 1)
            {
                CurrentPageIndex++;
                LoadPage(CurrentPageIndex);
            }
            else
            {
                var result = Model.Result.ToString();
                if (Model.Result == SolvedResult.Optimized)
                {
                    foreach (var kvp in Model.ObjectiveResult
                        .Where(kvp => kvp.Key.Type == VariableType.Input))
                    {
                        result += $"\n{kvp.Key.Name}: {kvp.Value.ToFractionString()}";
                    }
        
                    result += $"\nZ: {Model.Z.ToFractionString()}";
                }
                await MessageBox.Show(MainWindow, result, "Result", MessageBox.MessageBoxButtons.Ok);
            }
        }
        
        public class SolverIterationPage
        {
            public SolverIteration Iteration;
            public SolverMethod Method;
        }

        public enum SolverMethod
        {
            TwoPhases,
            Simplex
        }
    }
}
