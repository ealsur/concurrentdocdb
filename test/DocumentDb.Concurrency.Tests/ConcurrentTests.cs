using DocumentDb.Concurrency;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DocumentDb.Concurrenty.Tests
{
    public class ConcurrentTests
    {
		private DocumentClient _dbClient;
		private readonly Uri _collectionUri;
		public ConcurrentTests()
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

		/// <summary>
		/// Creates a Query with FeedOptions
		/// </summary>
		/// <typeparam name="T">Type of Class to serialize</typeparam>
		/// <param name="feedOptions"></param>
		/// <returns></returns>
		private IQueryable<T> CreateQuery<T>(FeedOptions feedOptions = null)
		{
			return _dbClient.CreateDocumentQuery<T>(_collectionUri, feedOptions);
		}
		
		[Fact]
		public async Task TheOneAndOnly()
		{
			var theId = Guid.NewGuid().ToString();
			var aDoc = new DummyDocument();
			aDoc.Id = theId;
			await _dbClient.CreateDocumentAsync(_collectionUri, aDoc);
			
			Document theDoc = await _dbClient.ReadDocumentAsync(UriFactory.CreateDocumentUri("test", "concurrencyTest", theId));
			Assert.Equal(theDoc.Id, theId);

			theDoc.SetPropertyValue("Property1", "modified");

			var result = await _dbClient.ReplaceConcurrentDocumentAsync(UriFactory.CreateDocumentUri("test", "concurrencyTest", theDoc.Id),theDoc);
			Assert.Equal(result.StatusCode, HttpStatusCode.OK);
			theDoc.SetPropertyValue("Property1", "modified again");
			var result2 = await _dbClient.ReplaceConcurrentDocumentAsync(UriFactory.CreateDocumentUri("test", "concurrencyTest", theDoc.Id), theDoc);
		}
	}
}
