using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using DocumentDb.Concurrency.Tests.Common;
using System.Net;
using Microsoft.ApplicationInsights;

namespace DocumentDb.Concurrency.Web.Tests.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
		private readonly DocumentClient _dbClient;
		private readonly Uri _collectionUri;
		private readonly TelemetryClient _telemetry;
		public ValuesController(TelemetryClient telemetry)
		{
			_collectionUri = GetCollectionLink();
			//Keys for the DocDB emulator
			_dbClient = new DocumentClient(new Uri("https://localhost:8081"), "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", new ConnectionPolicy()
			{
				MaxConnectionLimit = 100,
				ConnectionMode = ConnectionMode.Direct,
				ConnectionProtocol = Protocol.Tcp
			});
			_dbClient.OpenAsync().Wait();
			_telemetry = telemetry;
		}

		/// <summary>
		/// Obtains the link of a collection
		/// </summary>
		/// <param name="databaseName"></param>
		/// <param name="collectionName"></param>
		/// <returns></returns>
		private Uri GetCollectionLink()
		{
			return UriFactory.CreateDocumentCollectionUri("test", "concurrencyTest");
		}


		private Uri GetDocumentLink(string theId)
		{
			return UriFactory.CreateDocumentUri("test", "concurrencyTest", theId);
		}

		private async Task<Database> GetOrCreateDatabaseAsync(string id)
		{
			// Get the database by name, or create a new one if one with the name provided doesn't exist.
			// Create a query object for database, filter by name.
			IEnumerable<Database> query = from db in _dbClient.CreateDatabaseQuery()
										  where db.Id == id
										  select db;

			// Run the query and get the database (there should be only one) or null if the query didn't return anything.
			// Note: this will run synchronously. If async exectution is preferred, use IDocumentServiceQuery<T>.ExecuteNextAsync.
			Database database = query.FirstOrDefault();
			if (database == null)
			{
				// Create the database.
				database = await _dbClient.CreateDatabaseAsync(new Database { Id = id });
			}

			return database;
		}

		// GET api/values
		[HttpGet]
        public async Task<IActionResult> Get()
        {
			//Creates db and collection if they dont exist
			var database = await GetOrCreateDatabaseAsync("test");
			DocumentCollection c1 = await _dbClient.CreateDocumentCollectionIfNotExistsAsync(database.SelfLink, new DocumentCollection { Id = "concurrencyTest" });

			//Creates a random document
			var theId = Guid.NewGuid().ToString();
			var aDoc = new DummyDocument();
			aDoc.Id = theId;
			await _dbClient.CreateDocumentAsync(_collectionUri, aDoc);

			//Obtains the created document
			Document theDoc = await _dbClient.ReadDocumentAsync(GetDocumentLink(theId));
			if (!theDoc.Id.Equals(theId))
			{
				var err = new Exception("Error reading doc.");
				_telemetry.TrackException(err);
				throw err;
			}

			//Modifies document altering ETag internally
			theDoc.SetPropertyValue("Property1", "modified");
			var result = await _dbClient.ReplaceConcurrentDocumentAsync(GetDocumentLink(theDoc.Id), theDoc);
			if (!HttpStatusCode.OK.Equals(result.StatusCode))
			{
				var err = new Exception("Error modifying doc.");
				_telemetry.TrackException(err);
				throw err;
			}

			//Modifies the same document generating a concurrency exception and handling it
			theDoc.SetPropertyValue("Property1", "modified again");
			var result2 = await _dbClient.ReplaceConcurrentDocumentAsync(GetDocumentLink(theDoc.Id), theDoc).OnConcurrencyException((exception) =>
			{
				_telemetry.TrackEvent("Concurrent handler working!");
				Console.WriteLine($"I got it!! {exception.Message}");
			});
			//Now with Wait (shouldnt be using it but Im testing for deadlocks)
			var result3 = _dbClient.ReplaceConcurrentDocumentAsync(GetDocumentLink(theDoc.Id), theDoc).OnConcurrencyException((exception) =>
			{
				_telemetry.TrackEvent("Concurrent handler working!");
				Console.WriteLine($"I got it!! {exception.Message}");
			});
			result3.Wait();
			return Ok();
        }
    }
}
