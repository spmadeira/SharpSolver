using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Numerics;
using SharpSolver;

namespace SharpSolverUI
{
    /// <summary>
    /// Interaction logic for LinearResultWindow.xaml
    /// </summary>
    public partial class LinearResultWindow : Window
    {
        private SolvedModel BuildSolvedModel()
        {
            var x1 = new Variable { Name = "X1", Type = VariableType.Input };
            var x2 = new Variable { Name = "X2", Type = VariableType.Input };
            var c1 = new Constraint
            {
                Name = "C1",
                ConstraintType = ConstraintType.GreaterThanOrEqualTo,
                Variables = new List<ValueVariable>(new[]
                {
                    new ValueVariable {Variable = x1, Coefficient = 2},
                    new ValueVariable {Variable = x2, Coefficient = 1},
                }),
                ConstraintValue = 4
            };
            var c2 = new Constraint
            {
                Name = "C2",
                ConstraintType = ConstraintType.GreaterThanOrEqualTo,
                Variables = new List<ValueVariable>(new[]
                {
                    new ValueVariable {Variable = x1, Coefficient = 1},
                    new ValueVariable {Variable = x2, Coefficient = 7},
                }),
                ConstraintValue = 7
            };
            var objectiveFunction = new List<ValueVariable>(new[]
            {
                new ValueVariable {Variable = x1, Coefficient = 1},
                new ValueVariable {Variable = x2, Coefficient = 1},
            });
            
            var linearModel = new LinearModel
            {
                Objective = Objective.MIN,
                Variables = new List<Variable>(new[] { x1, x2 }),
                Constraints = new List<Constraint>(new[] { c1, c2 }),
                ObjectiveFunction = objectiveFunction
            };
            
            var res = LinearModelSolver.Solve(linearModel);
            
            return res;
        }

        public SolvedModel Model;
        private int currentPageIndex;
        public List<SolverIterationPage> SolverIterationPages = new List<SolverIterationPage>();

        public TextBlock Tableau;
        public TextBlock[] Variables;
        public TextBlock[] ObjectiveRows;
        public TextBlock[][] VariableGrid;
        public TextBlock[] DecisionVariables;
        public TextBlock Z;
        public TextBlock[] Bases;
        public TextBlock[] BaseVariables;

        public Rectangle[][] Rectangles;

        public LinearResultWindow()
        {
            InitializeComponent();
            Model = BuildSolvedModel();
            Grid.Loaded += Grid_Loaded;
        }

        public LinearResultWindow(SolvedModel model)
        {
            InitializeComponent();
            Model = model;
            
            Grid.Loaded += Grid_Loaded;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var twoPhasesPages = Model.TwoPhasesIterations.Select(i =>
            {
                return new SolverIterationPage
                {
                    Iteration = i,
                    Method = SolverMethod.TwoPhases
                };
            });
            var simplexPages = Model.SimplexIterations.Select(i =>
            {
                return new SolverIterationPage
                {
                    Iteration = i,
                    Method = SolverMethod.Simplex
                };
            });
            SolverIterationPages = twoPhasesPages.Concat(simplexPages).ToList();
            BuildGrid(SolverIterationPages[0].Iteration);
            currentPageIndex = 0;
            LoadPage(currentPageIndex);
        }

        private void BuildGrid(SolverIteration iterationBase)
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
            // var gridWidth = Grid.Width;
            // var cellWidth = gridWidth / Grid.ColumnDefinitions.Count;
            //
            //
            // var gridHeight = Grid.Height;
            // var cellHeight = gridHeight / Grid.RowDefinitions.Count;
            //
            //
            // foreach (var columnDefinition in Grid.ColumnDefinitions)
            // {
            //     columnDefinition.Width = new GridLength(cellWidth);
            // }
            //
            // foreach (var rowDefinition in Grid.RowDefinitions)
            // {
            //     rowDefinition.Height = new GridLength(cellHeight);
            // }

            //Build grid rectangles
            var rects = new List<List<Rectangle>>();
            for (int i = 0; i < Grid.RowDefinitions.Count; i++)
            {
                var rectR = new List<Rectangle>();
                for (int j = 0; j < Grid.ColumnDefinitions.Count; j++)
                {
                    var rect = new Rectangle {Stroke = Brushes.Black, Fill = Brushes.Transparent};
                    rectR.Add(rect);
                    Grid.SetRow(rect,i);
                    Grid.SetColumn(rect,j);
                    Grid.Children.Add(rect);
                }
                rects.Add(rectR);
            }
            Rectangles = rects.Select(r => r.ToArray()).ToArray();
            
            //Build default grid blocks
            Tableau = new TextBlock
            {
                Text = "Tableau",
                FontWeight = FontWeights.Bold, 
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
                FontWeight = FontWeights.Bold, 
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
                FontWeight = FontWeights.Bold, 
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
                FontWeight = FontWeights.Bold, 
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
                FontWeight = FontWeights.Bold, 
                VerticalAlignment = VerticalAlignment.Center, 
                HorizontalAlignment = HorizontalAlignment.Center
            };;
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
                    FontWeight = FontWeights.Bold, 
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
            
            var baseBlocks = new List<TextBlock>();
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

            //BackButton.Content = Resources["BackArrowEnabled"];
            //https://stackoverflow.com/questions/3789256/wpf-gridlines-changing-style
            //https://www.c-sharpcorner.com/UploadFile/mahesh/grid-in-wpf/#:~:text=The%20ColumnDefinitions%20property%20is%20used,three%20rows%20to%20a%20grid.&text=Any%20control%20in%20WPF%20can,Row%20and%20Grid.
        }

        private void LoadPage(int index)
        {
            var page = SolverIterationPages[index];

            if (Grid.ColumnDefinitions.Count != page.Iteration.Variables.Length + 2 ||
                Grid.RowDefinitions.Count != page.Iteration.Bases.Length + 3)
            {
                Console.WriteLine("Rebuilding Grid");
                BuildGrid(page.Iteration);
            }

            MethodText.Text = page.Method.ToString();
            var objectiveText = "";
            foreach (var objectiveVariable in Model.Objective)
            {
                //Don't show vars with coef = 0
                if (objectiveVariable.Coefficient > 0)
                {
                    objectiveText += $"+ {objectiveVariable.Coefficient.ToFractionString()}{objectiveVariable.Variable.Name} ";
                } else if (objectiveVariable.Coefficient < 0)
                {
                    //Space between - and coef
                    objectiveText += $"- {(objectiveVariable.Coefficient*-1).ToFractionString()}{objectiveVariable.Variable.Name} ";
                }
            }
            if (objectiveText.StartsWith("+"))
                objectiveText = new string(objectiveText.Skip(2).ToArray());
            
            ObjectiveFunctionText.Text = $"{Model.ObjectiveOrientation.ToString()} Z: {objectiveText}";
            IterationText.Text = $"{index+1}";

            Tableau.Text = $"Tableau {index+1}";

            for (int i = 0; i < page.Iteration.Variables.Length; i++)
            {
                Variables[i].Text = page.Iteration.Variables[i].Name;
                // ObjectiveRows[i].Text = page.Iteration.ObjectiveVariables[i].ToString("0.####", CultureInfo.CurrentCulture);
                // DecisionVariables[i].Text = page.Iteration.DecisionVariables[i].ToString("0.####", CultureInfo.CurrentCulture);
                ObjectiveRows[i].Text = page.Iteration.ObjectiveVariables[i].ToFractionString();
                DecisionVariables[i].Text = page.Iteration.DecisionVariables[i].ToFractionString();
            }

            for (int i = 0; i < page.Iteration.Bases.Length; i++)
            {
                Bases[i].Text = page.Iteration.Bases[i].Name;
                //BaseVariables[i].Text = page.Iteration.BaseVariables[i].ToString("0.####", CultureInfo.CurrentCulture);
                BaseVariables[i].Text = page.Iteration.BaseVariables[i].ToFractionString();
            }

            for (int i = 0; i < page.Iteration.Bases.Length; i++)
            {
                for (int j = 0; j < page.Iteration.Variables.Length; j++)
                {
                    //VariableGrid[i][j].Text = page.Iteration.InputGrid[i][j].ToString("0.####",CultureInfo.CurrentCulture);
                    VariableGrid[i][j].Text = page.Iteration.InputGrid[i][j].ToFractionString();
                }
            }

            //Z.Text = page.Iteration.Z.ToString("0.####", CultureInfo.CurrentCulture);
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
                    VariableGrid[i][j].FontWeight = FontWeights.Normal;
                }
            }
            
            if (page.Iteration.SelectedGridPositions != (-1, -1))
            {
                var pos = page.Iteration.SelectedGridPositions;
                Rectangles[pos.x+2][pos.y+2].Fill = Brushes.Chartreuse;
                VariableGrid[pos.x][pos.y].FontWeight = FontWeights.Bold;
            }
        }

        private void PreviousPage(object args, RoutedEventArgs e)
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                LoadPage(currentPageIndex);
            }
        }
        
        private void NextPage(object args, RoutedEventArgs e)
        {
            if (currentPageIndex < SolverIterationPages.Count - 1)
            {
                currentPageIndex++;
                LoadPage(currentPageIndex);
            }
            else
            {
                var result = Model.Result.ToString();
                if (Model.Result == SolvedResult.Optimized)
                {
                    foreach (var kvp in Model.ObjectiveResult.Where(kvp => kvp.Key.Type == VariableType.Input))
                    {
                        result += $"\n{kvp.Key.Name}: {kvp.Value.ToFractionString()}";
                    }

                    result += $"\nZ: {Model.Z.ToFractionString()}";
                }
                MessageBox.Show(result, "Result", MessageBoxButton.OK);
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
