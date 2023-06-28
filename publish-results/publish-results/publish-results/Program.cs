using CsvHelper;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;
using Graph = VDS.RDF.Graph;

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
        var durationList = new List<double>();
        var nodeList = new Dictionary<string, string> { { "method", "" }, { "language", "" }, { "system", "" }, { "cpu", "" } };
        var changeList = new Dictionary<string, bool> { { "language", false }, { "system", false }, { "cpu", false }, { "index", false } };
        var prevNodeList = new Dictionary<string, string> { { "method", "" }, { "language", "" }, { "system", "" }, { "cpu", "" }, { "index", "" } };

        foreach (var record in records)
        {
            Dictionary<string, string> recordList = new() { { "method", record.Method }, { "language", record.Language }, { "system", record.System }, { "cpu", record.CPU } };
            var duration = record.Duration;

            // Skip to the next person if all current fields are empty
            if (recordList["cpu"].Equals("") && recordList["system"].Equals("") && recordList["language"].Equals("") && recordList["method"].Equals("") && duration.Equals(""))
            {
                index++;
                changeList["index"] = true;

                continue;
            }

            if (nameList.Contains(recordList["cpu"]) || recordList["cpu"].Equals("CPU"))
                continue;

            if (index == 2 && recordList["language"].Equals("Nodejs") && recordList["method"].Equals("S"))
                Console.WriteLine("We're here.");

            (index, graph, nodeList, changeList) = CreateSubjectTriple(graph, nameList, index, recordList, nodeList, changeList);

            (graph, nodeList, changeList) = HandleCPUChange(graph, nameList, index, recordList, nodeList, changeList);

            (graph, nodeList, changeList) = HandleSystemChange(graph, nameList, index, recordList, nodeList, changeList);

            (graph, nodeList, changeList) = HandleLanguageChange(graph, nameList, index, recordList, nodeList, changeList);

            (graph, nodeList, durationList) = HandleMethodChange(graph, nameList, index, recordList, nodeList, prevNodeList, changeList, durationList, duration);

            if (records.Last().Equals(record))
            {
                graph = AnalyseDuration(graph, durationList, nameList, index, nodeList["cpu"], nodeList["system"], nodeList["language"], nodeList["method"]);
                continue;
            }

            changeList = ResetChangeList(changeList);
            
            foreach (var (key, val) in nodeList)
            {
                prevNodeList[key] = val;
            }

            prevNodeList["index"] = index.ToString();
        }

        return graph;
    }

    static (int, Graph, Dictionary<string, string>, Dictionary<string, bool>) CreateSubjectTriple(Graph graph, List<string> nameList, int index, Dictionary<string, string> recordList, Dictionary<string, string> nodeList, Dictionary<string, bool> changeList)
    {
        if (index == -1)
        {
            index++;
            nodeList = new Dictionary<string, string> { { "method", "" }, { "language", "" }, { "system", "" }, { "cpu", "" } };
            
            // Create CPU node triple if at row 0
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

    static (Graph, Dictionary<string, string>, List<double> durationListOut) HandleMethodChange(Graph graph, List<string> nameList, int index, Dictionary<string, string> recordList, Dictionary<string, string> nodeList, Dictionary<string, string> prevNodeList, Dictionary<string, bool> changeList, List<double> durationList, string duration)
    {
        var durationListOut = new List<double>();

        if (!recordList["method"].Equals(nodeList["method"]) || changeList["language"] || changeList["system"] || changeList["cpu"] || changeList["index"])
        {
            var noPrevMethod = false;

            var prevMethod = prevNodeList["method"];
            if (prevMethod == "")
            {
                prevMethod = recordList["method"];
                noPrevMethod = true;
            }
            
            var language = recordList["language"];
            if (!recordList["language"].Equals(prevNodeList["language"]) && !prevNodeList["language"].Equals(""))
                language = prevNodeList["language"];

            var system = recordList["system"];
            if (!recordList["system"].Equals(prevNodeList["system"]) && !prevNodeList["system"].Equals(""))
                system = prevNodeList["system"];

            var cpu = recordList["cpu"];
            if (!recordList["cpu"].Equals(prevNodeList["cpu"]) && !prevNodeList["cpu"].Equals(""))
                cpu = prevNodeList["cpu"];

            var prevIndex = index;
            if (!index.Equals(prevNodeList["index"]))
                prevIndex = prevNodeList["index"] != "" ? Convert.ToInt32(prevNodeList["index"]) : 0;

            // Create Method node triple if the method name has changed
            var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[prevIndex] + '/' + cpu + '/' + system + '/' + language));
            var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "Method"));
            var obj = graph.CreateLiteralNode(recordList["method"]);
            graph.Assert(new Triple(subject, predicate, obj));
            nodeList["method"] = recordList["method"];

            if (noPrevMethod)
            {
                // Skip wrapping up existing values in durationList if on a new person
                // Create Cold Duration node triple for the current duration value
                subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index] + '/' + cpu + '/' + system + '/' + language + '/' + recordList["method"]));
                predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "Duration/Cold"));
                obj = graph.CreateLiteralNode(duration);
                graph.Assert(new Triple(subject, predicate, obj));
            }
            else
            {
                graph = AnalyseDuration(graph, durationList, nameList, prevIndex, cpu, system, language, prevMethod);

                // Create Cold Duration node triple for the current duration value
                subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index] + '/' + recordList["cpu"] + '/' + recordList["system"] + '/' + recordList["language"] + '/' + recordList["method"]));
                predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "Duration/Cold"));
                obj = graph.CreateLiteralNode(duration);
                graph.Assert(new Triple(subject, predicate, obj));
            }
        }
        else
        {
            durationListOut = HandleDuration(durationList, duration);
        }

        return (graph, nodeList, durationListOut);
    }

    static List<double> HandleDuration(List<double> durationList, string duration)
    {
        durationList.Add(Convert.ToDouble(duration));

        return durationList;
    }

    static Graph AnalyseDuration(Graph graph, List<double> durationList, List<string> nameList, int prevIndex, string cpu, string system, string language, string prevMethod)
    {
        // Find the average of all duration values
        var average = durationList.Count > 0 ? durationList.Average() : 0.0;
        var sum = durationList.Count > 0 ? durationList.Sum() : 0.0;
        var min = durationList.Count > 0 ? durationList.Min() : 0.0;

        // Create Mean Duration node triple with the average duration value
        var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[prevIndex] + '/' + cpu + '/' + system + '/' + language + '/' + prevMethod));
        var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "Duration/Mean"));
        var obj = graph.CreateLiteralNode(average.ToString());
        graph.Assert(new Triple(subject, predicate, obj));

        // Create Sum Duration node triple with the sum of duration values
        subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[prevIndex] + '/' + cpu + '/' + system + '/' + language + '/' + prevMethod));
        predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "Duration/Sum"));
        obj = graph.CreateLiteralNode(sum.ToString());
        graph.Assert(new Triple(subject, predicate, obj));

        // Create Min Duration node triple with minimum duration value
        subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[prevIndex] + '/' + cpu + '/' + system + '/' + language + '/' + prevMethod));
        predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "Duration/Min"));
        obj = graph.CreateLiteralNode(min.ToString());
        graph.Assert(new Triple(subject, predicate, obj));

        return graph;
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
