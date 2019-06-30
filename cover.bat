@echo off
cd %~dp0

rem boilerplate code
set OPEN_COVER="packages\OpenCover.4.7.922\tools\OpenCover.Console.exe"
set REPORTGEN="%UserProfile%\.nuget\packages\reportgenerator\4.2.2\tools\net47\ReportGenerator.exe"
set TARGET=dotnet.exe
set TARGET_ARG=test

rem set output
set OUTPUT=Test\results.xml
set OUTPUT_DIR=Test\report

rem project code
set SEARCH_DIR=CommandLineArgumentParser\bin\Debug\netstandard2.0\
set FILTERS="+[CommandLineArgumentParser]* -[*]*Exception -[*]*Attribute -[*.Test]*"

rem coverage
%OPEN_COVER%					^
	-target:%TARGET%			^
	-targetargs:%TARGET_ARG% 	^
	-searchdirs:%SEARCH_DIR%	^
	-output:%OUTPUT%			^
	-filter:%FILTERS%			^
	-skipautoprops				^
	-oldstyle					^
    -register:user

rem report
%REPORTGEN% -reports:%OUTPUT% -targetdir:%OUTPUT_DIR%