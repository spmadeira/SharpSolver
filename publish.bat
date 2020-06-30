dotnet publish -r win-x64 -c Release --self-contained false
dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true --self-contained false

copy /y SharpSolverUI\bin\Release\netcoreapp3.1\win-x64\publish\SharpSolverUI.exe publish\SharpSolver-win.exe
copy /y SharpSolverUI.Avalonia\bin\Release\netcoreapp3.1\linux-x64\publish\SharpSolverUI.Avalonia publish\SharpSolver-linux

timeout 5