using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;

namespace DocumentDb.Concurrency
{
	/// <summary>
	/// Exception extension to handle only concurrency cases
	/// </summary>
	public static class ConcurrencyExceptionTaskExtension
    {
		/// <summary>
		/// Handles concurrency exceptions with a custom Action
		/// </summary>
		/// <param name="task"></param>
		/// <param name="continuationAction">Action to execute on concurrency exception</param>
		/// <returns></returns>
		public static async Task<ResourceResponse<Document>> OnConcurrencyException(this Task<ResourceResponse<Document>> task, Action<ConcurrencyException> continuationAction)
		{
			if (continuationAction == null)
			{
				throw new ArgumentNullException("continuationAction cannot be null");
			}

			try
			{
				return await task;
			}
			catch (ConcurrencyException ex)
			{
				continuationAction(ex);
				return null;
			}
		}
	}
}
