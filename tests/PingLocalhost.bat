@echo off

PowerPing.exe 127.0.0.1 >> test.txt
findstr "Reply" && SUCCESS
