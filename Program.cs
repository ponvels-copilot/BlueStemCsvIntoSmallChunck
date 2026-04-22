using System.Text;

namespace BlueStemCsvIntoSmallChunck;

class Program
{
    private const int DefaultChunkSize = 10_000;

    static int Main(string[] args)
    {
        if (args.Length < 1 || args.Length > 3)
        {
            Console.Error.WriteLine("Usage: BlueStemCsvIntoSmallChunck <input-file-or-directory> [output-directory] [chunk-size]");
            return 1;
        }

        var inputPath = Path.GetFullPath(args[0]);
        var outputDirectory = args.Length >= 2
            ? Path.GetFullPath(args[1])
            : Path.GetDirectoryName(inputPath) ?? Directory.GetCurrentDirectory();
        var chunkSize = args.Length >= 3 && int.TryParse(args[2], out var parsedChunkSize) && parsedChunkSize > 0
            ? parsedChunkSize
            : DefaultChunkSize;

        Directory.CreateDirectory(outputDirectory);

        var csvFiles = ResolveCsvFiles(inputPath);
        if (csvFiles.Count == 0)
        {
            Console.Error.WriteLine($"No CSV files found at path: {inputPath}");
            return 1;
        }

        foreach (var csvFile in csvFiles)
        {
            SplitCsv(csvFile, outputDirectory, chunkSize);
        }

        return 0;
    }

    private static List<string> ResolveCsvFiles(string inputPath)
    {
        if (File.Exists(inputPath))
        {
            return [inputPath];
        }

        if (Directory.Exists(inputPath))
        {
            return Directory
                .GetFiles(inputPath, "*.csv", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return [];
    }

    private static void SplitCsv(string inputFilePath, string outputDirectory, int chunkSize)
    {
        using var reader = new StreamReader(inputFilePath, Encoding.UTF8, true);
        var header = reader.ReadLine();

        if (string.IsNullOrWhiteSpace(header))
        {
            Console.Error.WriteLine($"Skipping '{inputFilePath}' because it does not contain a valid header row.");
            return;
        }

        var inputFileName = Path.GetFileNameWithoutExtension(inputFilePath);
        var chunkIndex = 0;
        var rowsInCurrentChunk = 0;
        StreamWriter? writer = null;

        try
        {
            while (reader.ReadLine() is { } line)
            {
                if (rowsInCurrentChunk == 0)
                {
                    writer?.Dispose();
                    chunkIndex++;
                    var outputPath = Path.Combine(outputDirectory, $"{inputFileName}_{chunkIndex:D4}.csv");
                    writer = new StreamWriter(outputPath, false, new UTF8Encoding(false));
                    writer.WriteLine(header);
                }

                writer!.WriteLine(line);
                rowsInCurrentChunk++;

                if (rowsInCurrentChunk == chunkSize)
                {
                    rowsInCurrentChunk = 0;
                }
            }
        }
        finally
        {
            writer?.Dispose();
        }
    }
}
