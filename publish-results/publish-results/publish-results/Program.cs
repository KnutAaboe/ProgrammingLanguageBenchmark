using System.ComponentModel;
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
        // Load the CSV file
        using (var reader = new StreamReader(csvFilePath))
        using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
        {
            // Parse the CSV records
            var records = csv.GetRecords<MyDataClass>();

            // Create a new RDF graph
            var graph = new Graph();

            var nameList = new List<string> {"Cecilie", "Sassan", "Espen"};
            int index = -1; // Counter for nameList index
            var durationList = new Dictionary<string, int>(); // Lists the number of times each duration(ms) has been repeated
            Dictionary<string, string> nodeList = new() { { "method", "" }, { "language", "" }, { "system", "" }, { "cpu", "" } };
            Dictionary<string, bool> changeList = new() { { "language", false }, { "system", false }, { "cpu", false }, { "index", false } };

            // Iterate over the CSV records and create RDF triples
            foreach (var record in records)
            {
                Dictionary<string, string> recordList = new() { { "method", record.Method }, { "language", record.Language }, { "system", record.System }, { "cpu", record.CPU } };
                var duration = record.Duration;

                if (recordList["cpu"].Equals("") && recordList["system"].Equals("") && recordList["language"].Equals("") && recordList["method"].Equals("") && duration.Equals(""))
                {
                    index++;
                    changeList["index"] = true;
                    durationList = new();
                    continue;
                }

                if (nameList.Contains(recordList["cpu"]) || recordList["cpu"].Equals("CPU"))
                    continue;


                if (index == -1)
                {
                    index++;
                    changeList["index"] = true;
                    durationList = new();
                    // Create a new RDF triple with the subject URI
                    var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index]));
                    var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "CPU"));
                    var obj = graph.CreateLiteralNode(recordList["cpu"]);
                    graph.Assert(new Triple(subject, predicate, obj));
                    nodeList["cpu"] = recordList["cpu"];
                    changeList["cpu"] = true;
                }
                else if (!recordList["cpu"].Equals(nodeList["cpu"]) || changeList["index"])
                {
                    durationList = new();
                    var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index]));
                    var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "CPU"));
                    var obj = graph.CreateLiteralNode(recordList["cpu"]);
                    graph.Assert(new Triple(subject, predicate, obj));
                    nodeList["cpu"] = recordList["cpu"];
                    changeList["cpu"] = true;
                }

                if (!recordList["system"].Equals(nodeList["system"]) || changeList["cpu"] || changeList["index"])
                {
                    durationList = new();
                    var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index] + '/' + recordList["cpu"]));
                    var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "System"));
                    var obj = graph.CreateLiteralNode(recordList["system"]);
                    graph.Assert(new Triple(subject, predicate, obj));
                    nodeList["system"] = recordList["system"];
                    changeList["system"] = true;
                }

                if (!recordList["language"].Equals(nodeList["language"]) || changeList["system"] || changeList["cpu"] || changeList["index"])
                {
                    durationList = new();
                    var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index] + '/' + recordList["cpu"] + '/' + recordList["system"]));
                    var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "Language"));
                    var obj = graph.CreateLiteralNode(recordList["language"]);
                    graph.Assert(new Triple(subject, predicate, obj));
                    nodeList["language"] = recordList["language"];
                    changeList["language"] = true;
                }

                if (!recordList["method"].Equals(nodeList["method"]) || changeList["language"] || changeList["system"] || changeList["cpu"] || changeList["index"])
                {
                    var subject = graph.CreateUriNode(UriFactory.Create(mainURI + nameList[index] + '/' + recordList["cpu"] + '/' + recordList["system"] + '/' + recordList["language"]));
                    var predicate = graph.CreateUriNode(UriFactory.Create(mainURI + "Method"));
                    var obj = graph.CreateLiteralNode(recordList["method"]);
                    graph.Assert(new Triple(subject, predicate, obj));
                    nodeList["method"] = recordList["method"];
                    durationList = new();
                }

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

                foreach (var category in changeList)
                {
                    changeList[category.Key] = false;
                }
            }

            // Serialize the RDF graph to Turtle format
            var rdfWriter = new CompressingTurtleWriter();
            using (var rdfStream = new MemoryStream())
            using (var rdfWriterStream = new StreamWriter(rdfStream, Encoding.UTF8))
            {
                rdfWriter.Save(graph, rdfWriterStream);

                // Save the RDF content to a file
                File.WriteAllBytes(rdfFilePath, rdfStream.ToArray());
            }
        }

        Console.WriteLine("CSV to RDF conversion complete!");
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
