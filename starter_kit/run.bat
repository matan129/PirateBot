@echo off

rem this script will always take you first of all to current directory
cd /d "%~dp0"

set pyexe=C:\python27\python.exe
rem check if python installed
if not exist %pyexe% (
    rem now we try with regular python
    where python > nul 2> nul
    if ERRORLEVEL 1 goto :nopython
    set pyexe=python
)

rem print usage if no params
if [%1]==[] goto usage
if [%2]==[] goto usage

set map=%3
if [%3]==[]  set map=%~dp0maps\default_map.map

if not exist %1 goto notexist1
if not exist %2 goto notexist2
set bot1=%~1
set bot2=%~2


%pyexe% "%~dp0lib\playgame.py" --loadtime 10000 -e -E -d --engine_seed 42 --player_seed 42 --log_dir "%~dp0lib\game_logs" --map_file "%map%" "%bot1%" "%bot2%"
goto:EOF

:usage
@echo Usage: %0 ^<Player One Bot^> ^<Player Two Bot^> [Map]
exit /B 1

:nopython
@echo ERROR: Python is not installed OR Python not in PATH
exit /B 1

:notexist1
@echo ERROR: Bot #1 ^"%1^" does not exist!
exit /B 1

:notexist2
@echo ERROR: Bot #2 ^"%2^" does not exist!
exit /B 1