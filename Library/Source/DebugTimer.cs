using System;

/// <summary>
/// Debug Timer Class
/// </summary>
public class DebugTimer : IDisposable
{
	private readonly System.Diagnostics.Stopwatch _watch;
	private readonly string _blockName;
	
	/// <summary>
	/// Creates a timer.
	/// </summary>
	/// <param name="blockName">Name of the block that's being timed</param>
	/// <example>
	/// public void Foo()
	/// {
	///   using (new DebugTimer("Foo()"))
	///   {
	///     // Do work
	///   }
	/// }
	///
	/// // In the Visual Studio Output window:
	/// // Foo(): 1.2345 seconds.
	/// </example>
	public DebugTimer(string blockName)
	{
		_blockName = blockName;
		_watch = System.Diagnostics.Stopwatch.StartNew();
	}
	
	public void Dispose()
	{
		_watch.Stop();
		GC.SuppressFinalize(this);
		System.Diagnostics.Debug.WriteLine(_blockName + ": " + _watch.Elapsed.TotalSeconds + " seconds.");
	}

	~DebugTimer()
	{
		throw new InvalidOperationException("Must Dispose() of all instances of " + this.GetType().FullName);
	}
}