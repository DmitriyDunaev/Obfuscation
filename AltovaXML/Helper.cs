// YOU SHOULD NOT MODIFY THIS FILE

using System;

namespace Helper 
{
	/// <summary>
	/// Base class for all exceptions thrown by functions of the Helper-library..
	/// </summary>
	public class HelperException : Exception 
	{
		public HelperException(string text) : base(text) 
		{
		}

		public HelperException(Exception other) : base("", other)
		{
		}

		public HelperException(string text, Exception other)
			: base(text, other)
		{
		}

		public string GetMessage() 
		{
			return Message;
		}

		public Exception GetInnerException() 
		{
			return InnerException;
		}
	}
	
	/// <summary>
	/// Exception that can be thrown by the user.
	/// </summary>
	public class UserException : HelperException
	{
		public UserException (string text) : base(text)
		{}
	}

	/// <summary>
	/// Interface to print TRACE and result output generated by the application.
	/// </summary>
	public interface TraceTarget 
	{
		void WriteTrace(string info);
	}

	/// <summary>
	/// Abstract class to be derived by the application for printing TRACE- and result-output generated by the application.
	/// </summary>
	public abstract class TraceProvider 
	{
		protected TraceTarget traceTarget = null;

		protected void WriteTrace(string info) 
		{
			if (traceTarget != null)
				traceTarget.WriteTrace(info);
		}

		public void RegisterTraceTarget(TraceTarget newTraceTarget) 
		{
			traceTarget = newTraceTarget;
		}

		public void UnregisterTraceTarget() 
		{
			traceTarget = null;
		}
	}
}