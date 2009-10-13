﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (<mailto:sharpos@michaelruck.de>)
 */

using System;
using System.Collections.Generic;
using System.Text;
using Mosa.Runtime.CompilerFramework;
using System.Diagnostics;
using Mosa.Runtime.Metadata;
using IR = Mosa.Runtime.CompilerFramework.IR;

namespace Mosa.Platforms.x86.CPUx86
{
	/// <summary>
	/// Intermediate representation of the x86 cvtsi2ss instruction.
	/// </summary>
	public sealed class Cvtsi2ssInstruction : TwoOperandInstruction
	{
		#region Construction

		/// <summary>
		/// Initializes a new instance of <see cref="Cvtsi2ssInstruction"/>.
		/// </summary>
		public Cvtsi2ssInstruction() :
			base()
		{
		}

		#endregion // Construction

		#region Methods

		/// <summary>
		/// Returns a string representation of the instruction.
		/// </summary>
		/// <returns>
		/// A string representation of the instruction in intermediate form.
		/// </returns>
		public override string ToString(Context context)
		{
			return String.Format(@"x86 cvtsi2ss {0}, {1} ; {0} = (float32){1}", context.Operand1, context.Operand2);
		}

		/// <summary>
		/// Allows visitor based dispatch for this instruction object.
		/// </summary>
		/// <param name="visitor">The visitor object.</param>
		/// <param name="context">The context.</param>
		public override void Visit(IX86Visitor visitor, Context context)
		{
			visitor.Cvtsi2ss(context);
		}

		#endregion // Methods
	}
}