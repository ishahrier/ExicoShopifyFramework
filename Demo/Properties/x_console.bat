@echo Off
SET /P compile_configuration=Enter compile configuration (debug or release):
SET /P app_environment=Enter app environment (development or production):
SET /P pnc=Publish and Compile again? (y or n):
IF %pnc%==y (
dotnet publish ..\ -c %compile_configuration%
)
cd ..\bin\%compile_configuration%\netcoreapp2.1\publish
start /MAX dotnet Demo.dll x_console --environment %app_environment%
pause