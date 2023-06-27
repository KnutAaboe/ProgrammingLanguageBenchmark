using System.Text;
using CsvHelper;
using VDS.RDF;
using VDS.RDF.Writing;

class Program
{
    const string csvFilePath = "..\\..\\..\\Test_Resultater.csv";
    const string rdfFilePath = "..\\..\\..\\Test_Resultater.ttl";
    const string mainURI = "http://greencodingexperiment.com/";

    static void Main()
    {
        var records = LoadCSVRecords();
        var graph = CreateRDFGraph(records);
        SaveRDFGraph(graph);

        Console.WriteLine("CSV to RDF conversion complete!");
    }

    static IEnumerable<MyDataClass> LoadCSVRecords()
    {
        using (var reader = new StreamReader(csvFilePath))
        using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
        {
            return csv.GetRecords<MyDataClass>().ToList();
        }
    }

    static Graph CreateRDFGraph(IEnumerable<MyDataClass> records)
    {
        var graph = new Graph();
        var nameList = new List<string> { "Cecilie", "Sassan", "Espen" };
        int index = -1; // Counter for nameList index
        var durationList = new Dictionary<string, int>(); // Lists the number of times each duration (ms) has been repeated
        var nodeList = new Dictionary<string, string> { { "method", "" }, { "language", "" }, { "system", "" }, { "cpu", "" } };
        var changeList = new Dictionary<string, bool> { { "language", false }, { "system", false }, { "cpu", false }, { "index", false } };

        foreach (var record in records)
        {
            Dictionary<string, string> recordList = new() { { "method", record.Method }, { "language", record.Language }, { "system", record.System }, { "cpu", record.CPU } };
            var duration = record.Duration;

            // Skip to the next person if all current fields are empty
            if (recordList["cpu"].Equals("") && recordList["system"].Equals("") && recordList["language"].Equals("") && recordList["method"].Equals("") && duration.Equals(""))
            {
                index++;
                changeList["index"] = true;
                durationList = new();
                continue;
            }

            if (nameList.Contains(recordList["cpu"]) || recordList["cpu"].Equals("CPU"))
                continue;

            (index, graph, nodeList, changeList) = CreateSubjectTriple(graph, nameList, index, recordList, nodeList, changeList);

            (graph, nodeList, changeList) = HandleCPUChange(graph, nameList, index, recordList, nodeList, changeList);

            (graph, nodeList, changeList) = HandleSystemChange(graph, nameList, index, recordList, nodeList, changeList);

            (graph, nodeList, changeList) = HandleLanguageChange(graph, nameList, index, recordList, nodeList, changeList);

            (graph, nodeList, durationList) = HandleMethodChange(graph, nameList, index, recordList, nodeList, durationList, changeList);

            (graph, durationList) = HandleDuration(graph, nameList, index, recordList, nodeList, durationList, duration);

            changeList = ResetChangeList(changeList);
        }

        return graph;
    }

    static (int, Graph, Dictionary<string, string>, Dictionary<string, bool>) CreateSubjectTriple(Graph graph, List<string> nameList, int index, Dictionary<string, string> recordList, Dictionary<string, string> nodeList, Dictionary<string, bool> changeList)
    {
        if (index == -1)
        {
            // Create CPU node triple if at row 0
            index++;
            var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index]));
            var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "CPU"));
            var obj = graph.CreateLiteralNode(recordList["cpu"]);
            graph.Assert(new Triple(subject, predicate, obj));
            nodeList["cpu"] = recordList["cpu"];
            changeList["cpu"] = true;
        }

        return (index, graph, nodeList, changeList);
    }

    static (Graph, Dictionary<string, string>, Dictionary<string, bool>) HandleCPUChange(Graph graph, List<string> nameList, int index, Dictionary<string, string> recordList, Dictionary<string, string> nodeList, Dictionary<string, bool> changeList)
    {
        if (!recordList["cpu"].Equals(nodeList["cpu"]) || changeList["index"])
        {
            // Create CPU node triple if the CPU name has changed
            var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index]));
            var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "CPU"));
            var obj = graph.CreateLiteralNode(recordList["cpu"]);
            graph.Assert(new Triple(subject, predicate, obj));
            nodeList["cpu"] = recordList["cpu"];
            changeList["cpu"] = true;
        }

        return (graph, nodeList, changeList);
    }

        static (Graph, Dictionary<string, string>, Dictionary<string, bool>) HandleSystemChange(Graph graph, List<string> nameList, int index, Dictionary<string, string> recordList, Dictionary<string, string> nodeList, Dictionary<string, bool> changeList)
    {
        if (!recordList["system"].Equals(nodeList["system"]) || changeList["cpu"] || changeList["index"])
        {
            // Create System node triple if the system name has changed
            var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index] + '/' + recordList["cpu"]));
            var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "System"));
            var obj = graph.CreateLiteralNode(recordList["system"]);
            graph.Assert(new Triple(subject, predicate, obj));
            nodeList["system"] = recordList["system"];
            changeList["system"] = true;
        }

        return (graph, nodeList, changeList);
    }

    static (Graph, Dictionary<string, string>, Dictionary<string, bool>) HandleLanguageChange(Graph graph, List<string> nameList, int index, Dictionary<string, string> recordList, Dictionary<string, string> nodeList, Dictionary<string, bool> changeList)
    {
        if (!recordList["language"].Equals(nodeList["language"]) || changeList["system"] || changeList["cpu"] || changeList["index"])
        {
            // Create Language node triple if the language name has changed
            var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index] + '/' + recordList["cpu"] + '/' + recordList["system"]));
            var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "Language"));
            var obj = graph.CreateLiteralNode(recordList["language"]);
            graph.Assert(new Triple(subject, predicate, obj));
            nodeList["language"] = recordList["language"];
            changeList["language"] = true;
        }

        return (graph, nodeList, changeList);
    }

    static (Graph, Dictionary<string, string>, Dictionary<string, int>) HandleMethodChange(Graph graph, List<string> nameList, int index, Dictionary<string, string> recordList, Dictionary<string, string> nodeList, Dictionary<string, int> durationList, Dictionary<string, bool> changeList)
    {
        if (!recordList["method"].Equals(nodeList["method"]) || changeList["language"] || changeList["system"] || changeList["cpu"] || changeList["index"])
        {
            // Create Method node triple if the method name has changed
            var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index] + '/' + recordList["cpu"] + '/' + recordList["system"] + '/' + recordList["language"]));
            var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "Method"));
            var obj = graph.CreateLiteralNode(recordList["method"]);
            graph.Assert(new Triple(subject, predicate, obj));
            nodeList["method"] = recordList["method"];
            durationList = new();
        }

        return (graph, nodeList, durationList);
    }

    static (Graph, Dictionary<string, int>) HandleDuration(Graph graph, List<string> nameList, int index, Dictionary<string, string> recordList, Dictionary<string, string> nodeList, Dictionary<string, int> durationList, string duration)
    {
        // Create Duration node triple
        var subject1 = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index] + '/' + recordList["cpu"] + '/' + recordList["system"] + '/' + recordList["language"] + '/' + recordList["method"]));
        var predicate1 = graph.CreateUriNode(UriFactory.Create(mainURI + "Duration"));
        if (durationList.ContainsKey(duration))
        {
            var obj = graph.CreateLiteralNode(duration + '.' + new string('0', durationList[duration]));
            graph.Assert(new Triple(subject1, predicate1, obj));
            durationList[duration]++;
        }
        else
        {
            var obj = graph.CreateLiteralNode(duration);
            graph.Assert(new Triple(subject1, predicate1, obj));
            durationList[duration] = 1;
        }

        return (graph, durationList);
    }

    // Reset changeList for the next record
    static Dictionary<string, bool> ResetChangeList(Dictionary<string, bool> changeList)
    {
        foreach (var category in changeList)
        {
            changeList[category.Key] = false;
        }

        return changeList;
    }

    static void SaveRDFGraph(Graph graph)
    {
        var rdfWriter = new CompressingTurtleWriter();
        using (var rdfStream = new MemoryStream())
        using (var rdfWriterStream = new StreamWriter(rdfStream, Encoding.UTF8))
        {
            rdfWriter.Save(graph, rdfWriterStream);

            File.WriteAllBytes(rdfFilePath, rdfStream.ToArray());
        }
    }
}

// Define a class to represent the CSV records
class MyDataClass
{
    public string CPU { get; set; }
    public string System { get; set; }
    public string Language { get; set; }
    public string Method { get; set; }
    public string Duration { get; set; }
}
