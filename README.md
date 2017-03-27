# Azure DocumentDB concurrency-aware extensions

## Introduction

DocumentDB provides [Optimistic Concurrency Control support](https://docs.microsoft.com/azure/documentdb/documentdb-faq) with the [ETag](https://en.wikipedia.org/wiki/HTTP_ETag) attribute and the [AccessCondition](https://msdn.microsoft.com/library/en-us/Dn799196.aspx) options.

Handling these scenarios is not a cookie-cutter problem, it requires a number of moving parts and try/catch cases to nail it.

Taking a snippet of the [official sample repo](https://github.com/Azure/azure-documentdb-dotnet/blob/master/samples/code-samples/DocumentManagement/Program.cs#L397):

    try
    {
    	var ac = new AccessCondition { Condition = readDoc.ETag, Type = AccessConditionType.IfMatch };
    	readDoc.SetPropertyValue("foo", "the updated value of foo");
    	updatedDoc = await client.ReplaceDocumentAsync(readDoc, new RequestOptions { AccessCondition = ac });
    }
    catch (DocumentClientException dce)
    {
    	//   now notice the failure when attempting the update 
    	//   this is because the ETag on the server no longer matches the ETag of doc (b/c it was changed in step 2)
    	if (dce.StatusCode == HttpStatusCode.PreconditionFailed)
    	{
		    Console.WriteLine("As expected, we have a pre-condition failure exception\n");
	    }
    }

These extensions are an attempt to make it less error-prone and easier to implement.

With the included extensions, the former example changes to:

    updateDoc = await client.ReplaceConcurrentDocumentAsync(readDoc).OnConcurrencyException((exception)=>
    {
    	//The original exception is in exception.InnerException
    	Console.WriteLine($"As expected, we have a pre-condition failure exception: {exception.Message}");
    });
 
 The key factors are the [ReplaceConcurrentDocumentAsync](https://github.com/ealsur/concurrentdocdb/blob/master/src/DocumentDb.Concurrency/DocumentClientExtensions.cs) extension and the [OnConcurrencyException](https://github.com/ealsur/concurrentdocdb/blob/master/src/DocumentDb.Concurrency/ConcurrencyExceptionTaskExtension.cs) extension that gives you the opportunity to define a handler that will only execute on a Concurrency error.
 
 These extensions help reduce moving parts and visually aid in understanding the program flow.
 
 ## Get it

You can obtain these extensions as a [Nuget Package](https://www.nuget.org/packages/DocumentDB.Concurrency). 

    Install-Package DocumentDB.Concurrency

Or reference it and use it according to the [License](./LICENSE).
 
 ## Issues

Please feel free to [report any issues](https://github.com/ealsur/concurrentdocdb/issues) you might encounter. I am always looking for feedback!

## Supported Frameworks

* .Net 4.5 Full Framework
* [.Net Standard Library](https://docs.microsoft.com/en-us/dotnet/articles/standard/library) 1.6

 
  
