﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (grover) <sharpos@michaelruck.de>
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;

using IR = Mosa.Runtime.CompilerFramework.IR;

namespace Mosa.Runtime.CompilerFramework
{

	/// <summary>
	/// 
	/// </summary>
	public sealed class PlatformStubStage : IMethodCompilerStage, IPipelineStage
	{

		#region IPipelineStage Members

		/// <summary>
		/// Retrieves the name of the compilation stage.
		/// </summary>
		/// <value>The name of the compilation stage.</value>
		string IPipelineStage.Name { get { return @"PlatformStubStage"; } }

		#endregion // IPipelineStage Members

		#region IMethodCompilerStage Members

		/// <summary>
		/// Setup stage specific processing on the compiler context.
		/// </summary>
		/// <param name="compiler">The compiler context to perform processing in.</param>
		void IMethodCompilerStage.Setup(IMethodCompiler compiler) { }

		/// <summary>
		/// Performs stage specific processing on the compiler context.
		/// </summary>
		void IMethodCompilerStage.Run() { }

		#endregion // IMethodCompilerStage Members
	}
}
