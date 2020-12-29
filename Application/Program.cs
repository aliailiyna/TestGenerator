using System;
using System.IO;
using System.Threading.Tasks.Dataflow;
using TestGeneratorLibrary;

namespace Application
{
    public class Program
    {
        private static readonly string EXTENSION = ".cs";

        private static readonly string SOURCE_DIR = "../../../../TestLibrary";
        private static readonly string DESTINATION_DIR = "../../../../TestsGenerated/Generated";

        private static readonly int MAX_DEGREE_OF_PARALLELISM = 3;

        public static void Main()
        {
            if (!Directory.Exists(DESTINATION_DIR))
            {
                Directory.CreateDirectory(DESTINATION_DIR);
            }

            TestGenerator testGenerator = new TestGenerator();

            ExecutionDataflowBlockOptions executionOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = MAX_DEGREE_OF_PARALLELISM,
                EnsureOrdered = false
            };

            // Creating the members of the pipeline
            TransformBlock<string, string> fileReadBlock = GetFileReadBlock(executionOptions);
            TransformManyBlock<string, ResultOfTestGeneration> testsGenerateBlock = GetTestGenerateBlock(executionOptions, testGenerator);
            ActionBlock<ResultOfTestGeneration> fileWriteBlock = GetFileWriteBlock(executionOptions, DESTINATION_DIR);


            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            // Connecting the dataflow blocks to form a pipeline
            fileReadBlock.LinkTo(testsGenerateBlock, linkOptions);
            testsGenerateBlock.LinkTo(fileWriteBlock, linkOptions);

            // Posting Data to the pipeline
            foreach (string fileName in Directory.EnumerateFiles(SOURCE_DIR, string.Format("*{0}", EXTENSION)))
            {
                fileReadBlock.Post(fileName);
            }

            // Marking the head of the pipeline as complete
            fileReadBlock.Complete();

            // Waiting  for the last block in the pipeline to process all messages
            fileWriteBlock.Completion.Wait();

            Console.WriteLine("Нажмите ENTER для завершения программы.");
            Console.ReadLine();
        }

        // Reading from file (async)
        public static TransformBlock<string, string> GetFileReadBlock(ExecutionDataflowBlockOptions options)
        {
            return new TransformBlock<string, string>(async fileName =>
            {
                Console.WriteLine(string.Format("Читаем из файла {0}.", Path.GetFileName(fileName)));
                return await new StreamReader(fileName).ReadToEndAsync();
            },
            options);
        }

        // Test generation (async)
        public static TransformManyBlock<string, ResultOfTestGeneration> GetTestGenerateBlock(ExecutionDataflowBlockOptions options, 
            TestGenerator testGenerator)
        {
            return new TransformManyBlock<string, ResultOfTestGeneration>(async code =>
            {
                Console.WriteLine("Происходит генерация тестов...");
                return await testGenerator.GenerateAsync(code);
            },
            options);
        }

        // Writing to file (async)
        public static ActionBlock<ResultOfTestGeneration> GetFileWriteBlock(ExecutionDataflowBlockOptions options, string outputDirectoryName)
        {
            return new ActionBlock<ResultOfTestGeneration>(async resultOfTestGeneration =>
            {
                string outputFileName = resultOfTestGeneration.Name + EXTENSION;
                Console.WriteLine(string.Format("Пишем в файл {0}.", outputFileName));
                await File.WriteAllTextAsync(Path.Combine(outputDirectoryName, outputFileName),
                    resultOfTestGeneration.Code);
            },
            options);
        }
    }
}
