using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using System.Threading.Tasks;
using System.Net;
using System.Reflection;

namespace DocumentDb.Concurrency
{
	/// <summary>
	/// Extensions to provide Concurrency-aware Replace operations
	/// </summary>
	public static class DocumentClientExtensions
    {
		public static async Task<ResourceResponse<Document>> ReplaceConcurrentDocumentAsync(this DocumentClient client, Document document, RequestOptions options = null)
		{
			RequestOptions refOptions = options ?? new RequestOptions();
			refOptions.AccessCondition = new AccessCondition() { Type = AccessConditionType.IfMatch, Condition = document.ETag };
			try
			{
				return await client.ReplaceDocumentAsync(document, refOptions).ConfigureAwait(false);
			}
			catch (DocumentClientException dce)
			{
				if (dce.StatusCode == HttpStatusCode.PreconditionFailed)
				{
					//Handle concurrency error
#if NETSTANDARD1_6
					return await Task.FromException<ResourceResponse<Document>>(new ConcurrencyException(refOptions.AccessCondition.Condition, dce));
#else
					return await FromExAsync<ResourceResponse<Document>>(new ConcurrencyException(refOptions.AccessCondition.Condition, dce));
#endif
				}
				throw;
			}
		}

		public static async Task<ResourceResponse<Document>> ReplaceConcurrentDocumentAsync(this DocumentClient client, string documentLink, object document, RequestOptions options = null)
		{
			RequestOptions refOptions = options ?? new RequestOptions();
			var runtimeEtag = document.GetType().GetRuntimeProperty("ETag");
			if (runtimeEtag != null)
			{
				refOptions.AccessCondition = new AccessCondition() { Type = AccessConditionType.IfMatch, Condition = (string)runtimeEtag.GetValue(document) };
			}
			try
			{
				return await client.ReplaceDocumentAsync(documentLink, document, refOptions).ConfigureAwait(false);
			}
			catch (DocumentClientException dce)
			{
				if (dce.StatusCode == HttpStatusCode.PreconditionFailed)
				{
					//Handle concurrency error
#if NETSTANDARD1_6
					return await Task.FromException<ResourceResponse<Document>>(new ConcurrencyException(refOptions.AccessCondition.Condition, dce));
#else
					return await FromExAsync<ResourceResponse<Document>>(new ConcurrencyException(refOptions.AccessCondition.Condition, dce));
#endif
				}
				throw;
			}
		}

		public static async Task<ResourceResponse<Document>> ReplaceConcurrentDocumentAsync(this DocumentClient client, Uri documentLink, object document, RequestOptions options = null)
		{
			return await ReplaceConcurrentDocumentAsync(client, documentLink.ToString(), document, options).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a task from an exception to use on NET451 scenarios
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ex"></param>
		/// <returns></returns>
		private static Task<T> FromExAsync<T>(Exception ex)
		{
			var task = new Task<T>(() => { throw ex; });
			task.RunSynchronously();
			return task;
		}
	}
}
