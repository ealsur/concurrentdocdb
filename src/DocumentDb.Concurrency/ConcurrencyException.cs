using Microsoft.Azure.Documents;
using System;

namespace DocumentDb.Concurrency
{
	/// <summary>
	/// Azure DocumentDb concurrency exception 
	/// </summary>
	public class ConcurrencyException : Exception
    {
		/// <summary>
		/// Azure DocumentDb concurrency exception 
		/// </summary>
		/// <param name="etag">ETag used for the AccessCondition</param>
		/// <param name="inner">Original DocumentClientException</param>
		public ConcurrencyException(string etag, DocumentClientException inner):base($"Azure DocumentDb concurrency exception for ETag {etag}",inner){}
	}
}
