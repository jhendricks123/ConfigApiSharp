using System;

namespace ConfigApiSharp
{
    /// <summary>
    /// The response from client builders which contains the WCF client proxy if successful, or an exception if not successful.
    /// </summary>
    public class BuildClientResult<T>
    {
        public string Name { get; set; }
        public T Client { get; set; }
        public Exception Exception { get; set; }
        public bool Success => Exception == null;
    }
}