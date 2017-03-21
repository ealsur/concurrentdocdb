using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using System.Threading.Tasks;
using System.Net;

namespace DocumentDb.Concurrency
{
    public static class DocumentClientExtensions
    {
		public static Task<ResourceResponse<Document>> ReplaceConcurrentDocumentAsync(this DocumentClient client, Document document, RequestOptions options = null)
		{
			RequestOptions refOptions = options ?? new RequestOptions();
			refOptions.AccessCondition = new AccessCondition() { Type = AccessConditionType.IfMatch, Condition = document.ETag };
			try
			{
				return client.ReplaceDocumentAsync(document, refOptions);
			}
			catch (DocumentClientException dce)
			{
				if (dce.StatusCode == HttpStatusCode.PreconditionFailed)
				{
					//Handle concurrency error
				}
				throw;
			}
		}
	}
}
