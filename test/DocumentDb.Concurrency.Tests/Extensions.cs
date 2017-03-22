using Microsoft.Azure.Documents.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentDb.Concurrenty.Tests
{
	public static class Extensions
    {
		public static async Task<T> TakeOne<T>(this IQueryable<T> source)
		{
			var documentQuery = source.AsDocumentQuery();
			if (documentQuery.HasMoreResults)
			{
				var queryResult = await documentQuery.ExecuteNextAsync<T>();
				if (queryResult.Any())
				{
					return queryResult.Single<T>();
				}
			}
			return default(T);
		}
	}
}
