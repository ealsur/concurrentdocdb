using System;
using Newtonsoft.Json;

namespace DocumentDb.Concurrenty.Tests
{
	/// <summary>
	/// Dummy class used for DocDB testing
	/// </summary>
	public class DummyDocument
    {
		[JsonProperty("id")]
		public string Id { get; set; }
		public string Property1 { get; set; } = Guid.NewGuid().ToString();
		public string Property2 { get; set; } = Guid.NewGuid().ToString();
	}
}
