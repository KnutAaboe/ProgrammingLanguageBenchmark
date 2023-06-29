## How to:

 - **Install Blazegraph:**
	 - Go to [the Blazegraph Github Repository](https://github.com/blazegraph/database/releases/tag/BLAZEGRAPH_2_1_6_RC).
	 - Download '*Blazegraph.jar*' and copy it to the root folder on your PC:
		> C:\Users\YOUR-USER-NAME

- **Run Blazegraph workbench:**
	- Open a command window and run the following line:
	`java -server -Xmx4g -jar blazegraph.jar`
	
	- Copy the URL provided at the bottom of the screen and open it in a browser. This 			should open the Blazegraph workbench.

- **Convert an Excel file to the RDF Turtle format:**
	- Make sure that your file is in the '*.csv*' format. If not, save your file as '*.csv*' in Excel. Be aware that the '*.csv*' format only allows one sheet per file, so you might have to save each sheet as a separate file, if you have multiple sheets.

	- Copy the file to the '*publish-results*' directory where the '*.csproj*' file is found:
		> ProgrammingLanguageBenchmark\publish-results\publish-results\publish-results

	- Open up the C# project and **make sure that the file name correlates with the one mentioned on line 9**. Build and Run the C# project. This should create a '*.ttl*' file in the same directory.

- **Create a database in Blazegraph:**
	- While the Blazegraph workbench is open, go to the *Update* tab and click on *choose file*. Then press the *Update* button. Make sure there are no errors in the output.
	
	- Go to the *Query* tab and copy this code into the text box:
	 `SELECT ?s ?p ?o
	 WHERE {
	 ?s ?p ?o
	 }
	 ORDER BY (?s)`
	 - Press *Execute* and check that the results match the data in your Excel file.
