# Duplicates Finder

Simple tool to find duplicate files on Windows.
It scans supplied directory and generate script file which later can be used to remove duplicates or create hard links.


## Guide

Parameters:
```
DuplicatesFinder \mode:<mode> [\includeEmpty] [\includeSoftLinks] [\bufSize:268435456] <file> <scanDirectory>
```

Available mode: 
- list - generate text file with the list of duplicates,
- delete - generate script to delete duplicated files, 
- hardLink - generate script to replace duplicated files with hard links.


Sample:
```
DuplicatesFinder.exe \mode:hardLink \bufSize:536870912 "work.bat" "d:\scanDir"
```


