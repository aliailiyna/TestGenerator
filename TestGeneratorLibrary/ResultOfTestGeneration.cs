namespace TestGeneratorLibrary
{
    public class ResultOfTestGeneration
    {
        public string Name
        { get; }

        public string Code
        { get; }

        public ResultOfTestGeneration(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }
}
