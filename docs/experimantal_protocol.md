# Experiment Protocol for Azure Functions Language Performance Benchmark

## Background

Making energy efficient software is good for those that run the software, provinding lower costs,
users, providing fater software and for the society at large by contriuting to less emissinons
from creating te energy it consumes.

Serverless computing promises to help redue energy use by software by providing automatic
scaling that minimizes te energy onsumed y the software when it is not in use. Since serverless software mostly uses energy when it runs there is a clear link etween te resources consumed y running the software and its energy use and its cost. 

Still, there seems to be little guidance in te litterature, and the vendors doumentations, for how to create as efficient serverless software as possible. In this document we will desccribe an experiment to determie whic programming language performs best when run as an Azure Function App.


In[1] R. Pereiraa et al ranks programming languages for energy use based on implmemntations of a set of algoritmic problems. The results for the time and energy rankings are shown in te figure below. We can see that there is a strong, though not perfect corrolation between energy and time use.
 

![Programming languages ranked by time and energy use from[1]](./figures/pl_rank.png "Programming languages ranked")




## Research Questions

While the results form [1] may seem robust, there may be differenecs in the Azure Functions runtime that influence the results. Furtermore, while there is no way to run native code, such as C or Rust in Azure Functions on the consumptino plan. The conosumption plan is te most interesting from an energy perspective as it is te only plan that allows scaling entirely to zero, as all oter plans include soome sort
of allways on functionality. However, it is possible to compile native languages to Web Assembly[].

Thus, these are our research questions:

**RQ-1** What are the relative efficiency of nativly supported programming languages in Azure Functions.

**RQ-2** How efficent is Rust compiled to Web Assembly when run in an Azure Function.

**RQ-3** Is performance in the `func`loccal Azure Functuions locasl development environment similar to results running as actual Azure Functions.

## Experimental Design

In order to answer the above reearch questions, we have desined two  experiments. The first experiment is to time implmentations of some of the probblems used in [1] to crate a similar ranking in the Azure Functions develoopment environment on a single laptop. The secoond experiment will e a repeat of the first one but in an actual Azure Functions with the consumption plan. To reduce noice we will only run one instance og onoe function at a time. Since we ccan not know wich hardware will run the software in Azure Functinos, we will use time as our measuring tool and not use any energy measuring tools such as RAPL. We will also make only minimal adjustments to te problem too make tem runnable as Azure Functions.

In each experiment we will use implementations in te following languages

* Java
* C-Sharp
* Javascript (node.js)
* Rust comopiled to Wasm run trough Javasccript

We are using te following problems for each language:


I nessecary we will run eac experiment with 1X, 10X and 100X the input size in otder too study the effects f startup times.

Each problem in each languages will be run 10 times and we will look at both the avaerage and minimal execcution times.



## Runbook

### Experiment 1: Using Func

Instal the latest version the the Azure Functions Core Tools

```
npm i -g azure-functions-core-tools@4 --unsafe-perm true
```

Build all Functions and binaries using:

```
make build-all
```

Then run each function app as well as a test script tat execcutes eac function 10 times and logs to a file.

```
make run-csharp > csarp.log 2>&1
make run-js  > js.log 2>&1
make run-rust > rust.log 2>&1
````

When each functions app is runnin run the experiment that executes each function 10 times.

```
ruby run-exeriment.rb http://localhhost:7071
```

Finally record the exection times from eacchh log file and analyse.

### Experiment 2; Using Azure Functions on the Consumption Plan


Build all Functions and binaries using:

```
make build-all
```

Run each app as an Azure Function App with the Consumption plan in Azure.

When each functions app is runnin run the experiment that executes each function 10 times.

```
ruby run-exeriment.rb https://<path-to-functions-app-in-azure>
```

**NB!** To aviod incurring costs, remember to remoove eacc app after the experiment is finised

## Reporting Guidelines



## References

[1] R. Pereiraa et al., Ranking Programming Languages by Energy Efficiency