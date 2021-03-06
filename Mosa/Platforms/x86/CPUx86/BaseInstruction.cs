﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

using Mosa.Runtime.CompilerFramework;
using Mosa.Runtime.CompilerFramework.Operands;

namespace Mosa.Platforms.x86.CPUx86
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class BaseInstruction : Runtime.CompilerFramework.BaseInstruction, IX86Instruction
	{

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseInstruction"/> class.
		/// </summary>
		protected BaseInstruction()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseInstruction"/> class.
		/// </summary>
		/// <param name="operandCount">The operand count.</param>
		private BaseInstruction(byte operandCount)
			: base(operandCount)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseInstruction"/> class.
		/// </summary>
		/// <param name="operandCount">The operand count.</param>
		/// <param name="resultCount">The result count.</param>
		protected BaseInstruction(byte operandCount, byte resultCount)
			: base(operandCount, resultCount)
		{
		}

		#endregion // Construction

		#region IPlatformInstruction Overrides

		/// <summary>
		/// Emits the specified platform instruction.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="emitter">The emitter.</param>
		public void Emit(Context context, ICodeEmitter emitter)
		{
			Emit(context, emitter as MachineCodeEmitter);
		}

		/// <summary>
		/// Gets the instruction latency.
		/// </summary>
		/// <value>The latency.</value>
		public virtual int Latency { get { return -1; } }

		#endregion // IPlatformInstruction Overrides

		#region Operand Overrides

		/// <summary>
		/// Returns a string representation of <see cref="ConstantOperand"/>.
		/// </summary>
		/// <returns>A string representation of the operand.</returns>
		public override string ToString()
		{
			return "X86." + base.ToString();
		}

		#endregion // Operand Overrides

		#region Methods

		/// <summary>
		/// Computes the opcode.
		/// </summary>
		/// <param name="destination">The destination operand.</param>
		/// <param name="source">The source operand.</param>
		/// <param name="third">The third operand.</param>
		/// <returns></returns>
		protected virtual OpCode ComputeOpCode(Operand destination, Operand source, Operand third)
		{
			throw new System.Exception("opcode not implemented for this instruction");
		}

		/// <summary>
		/// Emits the specified platform instruction.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="emitter">The emitter.</param>
		protected virtual void Emit(Context context, MachineCodeEmitter emitter)
		{
			OpCode opCode = ComputeOpCode(context.Result, context.Operand1, context.Operand2);
			emitter.Emit(opCode, context.Result, context.Operand1, context.Operand2);
		}

		/// <summary>
		/// Allows visitor based dispatch for this instruction object.
		/// </summary>
		/// <param name="visitor">The visitor.</param>
		/// <param name="context">The context.</param>
		public virtual void Visit(IX86Visitor visitor, Context context)
		{
		}

		/// <summary>
		/// Allows visitor based dispatch for this instruction object.
		/// </summary>
		/// <param name="visitor">The visitor.</param>
		/// <param name="context">The context.</param>
		public override void Visit(IVisitor visitor, Context context)
		{
			if (visitor is IX86Visitor)
				Visit(visitor as IX86Visitor, context);
		}

		#endregion // Overrides

		#region Typesizes

		/// <summary>
		/// Check if the given operand is an unsigned byte
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is an unsigned byte</returns>
		private static bool IsUnsignedByte(Operand operand)
		{
			return (operand.Type.Type == Runtime.Metadata.CilElementType.U1);
		}

		/// <summary>
		/// Check if the given operand is a signed byte
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is a signed byte</returns>
		private static bool IsSignedByte(Operand operand)
		{
			return (operand.Type.Type == Runtime.Metadata.CilElementType.I1);
		}

		/// <summary>
		/// Check if the given operand is an unsigned short
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is an unsigned short</returns>
		private static bool IsUnsignedShort(Operand operand)
		{
			return (operand.Type.Type == Runtime.Metadata.CilElementType.U2);
		}

		/// <summary>
		/// Check if the given operand is a signed short
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is a signed short</returns>
		private static bool IsSignedShort(Operand operand)
		{
			return (operand.Type.Type == Runtime.Metadata.CilElementType.I2);
		}

		/// <summary>
		/// Check if the given operand is an unsigned integer
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is an unsigned integer</returns>
		private static bool IsUnsignedInt(Operand operand)
		{
			return (operand.Type.Type == Runtime.Metadata.CilElementType.U4);
		}

		/// <summary>
		/// Check if the given operand is a signed integer
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is a signed integer</returns>
		private static bool IsSignedInt(Operand operand)
		{
			return (operand.Type.Type == Runtime.Metadata.CilElementType.I4);
		}

		/// <summary>
		/// Check if the given operand is an unsigned long
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is an unsigned long</returns>
		private static bool IsUnsignedLong(Operand operand)
		{
			return (operand.Type.Type == Runtime.Metadata.CilElementType.U8);
		}

		/// <summary>
		/// Check if the given operand is a signed long
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is a signed long</returns>
		private static bool IsSignedLong(Operand operand)
		{
			return (operand.Type.Type == Runtime.Metadata.CilElementType.I8);
		}

		/// <summary>
		/// Check if the given operand is a byte
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is a byte</returns>
		protected static bool IsByte(Operand operand)
		{
			return IsUnsignedByte(operand) || IsSignedByte(operand);
		}

		/// <summary>
		/// Check if the given operand is a short
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is a short</returns>
		protected static bool IsShort(Operand operand)
		{
			return IsUnsignedShort(operand) || IsSignedShort(operand);
		}

		/// <summary>
		/// Check if the given operand is a char
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is a char</returns>
		protected static bool IsChar(Operand operand)
		{
			return operand.Type.Type == Runtime.Metadata.CilElementType.Char;
		}

		/// <summary>
		/// Check if the given operand is an integer
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is an integer</returns>
		protected static bool IsInt(Operand operand)
		{
			return IsUnsignedInt(operand) || IsSignedInt(operand);
		}

		/// <summary>
		/// Check if the given operand is a long
		/// </summary>
		/// <param name="operand">The operand to check</param>
		/// <returns>True if it is a long</returns>
		public static bool IsLong(Operand operand)
		{
			return IsUnsignedLong(operand) || IsSignedLong(operand);
		}

		#endregion
	}
}
