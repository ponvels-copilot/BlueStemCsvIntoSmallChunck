# BlueStemCsvIntoSmallChunck

Split very large CSV files into smaller chunks.

## Behavior
- Splits each CSV into files with a maximum of **10,000** data rows (configurable).
- Repeats the original header row in every output chunk.
- Output file names start with the parent file name and keep chunk order:
  - `ParentFile_0001.csv`
  - `ParentFile_0002.csv`
  - ...

## Usage
```bash
dotnet run -- <input-file-or-directory> [output-directory] [chunk-size]
```

Examples:
```bash
dotnet run -- /path/data/master.csv
dotnet run -- /path/data /path/output
dotnet run -- /path/data/master.csv /path/output 10000
```
