/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (<mailto:sharpos@michaelruck.de>)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mosa.Runtime.Vm;

namespace Mosa.Runtime.CompilerFramework
{
    /// <summary>
    /// Calculates the layout of the stack of the method.
    /// </summary>
    public sealed class StackLayoutStage :
        IMethodCompilerStage,
        IStackLayoutProvider
    {
        #region Tracing

        /// <summary>
        /// Controls the tracing of the <see cref="StackLayoutStage"/>.
        /// </summary>
        /// <remarks>
        /// The stack layout tracing is done with the TraceLevel.Info. Set the TraceSwitch to this value
        /// to receive full stack layout tracing.
        /// </remarks>
        private static readonly TraceSwitch TRACING = new TraceSwitch(@"Mosa.Runtime.CompilerFramework.StackLayoutStage", @"Controls tracing of the Mosa.Runtime.CompilerFramework.StackLayoutStage method compiler stage.");

        #endregion // Tracing

        #region Data members

        /// <summary>
        /// Holds the total stack requirements of local variables of the compiled method.
        /// </summary>
        private int _localsSize;

        #endregion // Data members

        #region Construction

        /// <summary>
        /// Initializes a new instance of <see cref="StackLayoutStage"/>.
        /// </summary>
        public StackLayoutStage()
        {
        }

        #endregion // Construction

        #region IMethodCompilerStage Members

        string IMethodCompilerStage.Name
        {
            get { return @"StackLayoutStage"; }
        }

        void IMethodCompilerStage.Run(MethodCompilerBase methodCompiler)
        {
            if (null == methodCompiler)
                throw new ArgumentNullException(@"methodCompiler");

            // Instruction lists for the prologue/epilogue instructions
            List<Instruction> prologueBlock, epilogueBlock;
            // Allocate a list of locals
            List<StackOperand> locals = new List<StackOperand>();
            // Architecture
            IArchitecture arch = methodCompiler.Architecture;

            // Retrieve the calling convention of the method
            ICallingConvention cc = methodCompiler.Architecture.GetCallingConvention(methodCompiler.Method.Signature.CallingConvention);
            Debug.Assert(null != cc, @"Failed to retrieve the calling convention of the method.");

            // Is the method split into basic blocks?
            IBasicBlockProvider blockProvider = (IBasicBlockProvider)methodCompiler.GetPreviousStage(typeof(IBasicBlockProvider));
            if (null == blockProvider)
            {
                // No, operate on the raw instruction stream
                IInstructionsProvider ip = (IInstructionsProvider)methodCompiler.GetPreviousStage(typeof(IInstructionsProvider));
                if (null != ip)
                {
                    prologueBlock = epilogueBlock = ip.Instructions;
                    CollectLocalVariables(locals, prologueBlock);
                }
                else
                {
                    throw new InvalidOperationException(@"MethodCompiler must have at least an IInstructionProvider or IBasicBlockProvider stage before the stack layout stage.");
                }
            }
            else
            {
                // Iterate all blocks and collect locals from all blocks
                foreach (BasicBlock block in blockProvider)
                    CollectLocalVariables(locals, block.Instructions);

                // Retrieve the default blocks.
                prologueBlock = blockProvider.FromLabel(-1).Instructions;
                epilogueBlock = blockProvider.FromLabel(Int32.MaxValue).Instructions;
            }

            // Sort all found locals
            OrderVariables(locals, cc);

            // Now we assign increasing stack offsets to each variable
            _localsSize = LayoutVariables(locals, cc, cc.OffsetOfFirstLocal, 1);
            if (TRACING.TraceInfo == true)
            {
                Trace.WriteLine(String.Format(@"Stack layout for method {0}", methodCompiler.Method));
                LogOperands(locals);
            }

            // Layout parameters
            LayoutParameters(methodCompiler, cc);

            // Create a prologue instruction
            prologueBlock.Insert(0, arch.CreateInstruction(typeof(IR.PrologueInstruction), _localsSize));
            // Create an epilogue instruction
            epilogueBlock.Add(arch.CreateInstruction(typeof(IR.EpilogueInstruction), _localsSize));
        }

        #endregion // IMethodCompilerStage Members

        #region IStackLayoutStage Members

        int IStackLayoutProvider.LocalsSize
        {
            get { return _localsSize; }
        }

        #endregion // IStackLayoutStage Members

        #region Internals

        /// <summary>
        /// Collects all local variables assignments into a list.
        /// </summary>
        /// <param name="locals">Holds all locals found by the stage.</param>
        /// <param name="instructions">The enumerable instruction list, which may contain assignments to local variables.</param>
        private static void CollectLocalVariables(List<StackOperand> locals, IEnumerable<Instruction> instructions)
        {
            // Iterate all instructions
            foreach (Instruction i in instructions)
            {
                // Does this instruction define a new stack variable?
                foreach (Operand op in i.Results)
                {
                    // The instruction list may not be in SSA form, so we have to check existence again here unfortunately.
                    // FIXME: Allow us to detect the state of blocks
                    LocalVariableOperand lvop = op as LocalVariableOperand;
                    if (null != lvop && false == locals.Contains(lvop))
                        locals.Add(lvop);
                }
            }
        }

        /// <summary>
        /// Lays out all parameters of the method.
        /// </summary>
        /// <param name="compiler">The method compiler providing the parameters.</param>
        /// <param name="cc">The calling convention used to invoke the method, which controls parameter layout.</param>
        private void LayoutParameters(MethodCompilerBase compiler, ICallingConvention cc)
        {
            List<StackOperand> paramOps = new List<StackOperand>();
            for (int i = 0; i < compiler.Method.Parameters.Count; i++)
            {
                paramOps.Add((StackOperand)compiler.GetParameterOperand(i));
            }

            LayoutVariables(paramOps, cc, cc.OffsetOfFirstParameter, -1);
            if (TRACING.TraceInfo == true)
            {
                LogOperands(paramOps);
            }
        }

        /// <summary>
        /// Performs a stack layout of all local variables in the list.
        /// </summary>
        /// <param name="locals">The enumerable holding all locals.</param>
        /// <param name="cc">The cc.</param>
        /// <param name="offsetOfFirst">Specifies the offset of the first stack operand in the list.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        private static int LayoutVariables(IEnumerable<StackOperand> locals, ICallingConvention cc, int offsetOfFirst, int direction)
        {
            int offset = offsetOfFirst, thisOffset;
            int size, alignment, padding;
            foreach (StackOperand lvo in locals)
            {
                // Does the offset fit the alignment requirement?
                cc.GetStackRequirements(lvo, out size, out alignment);
                if (1 == direction)
                {
                    padding = (offset % alignment);
                    offset -= (padding + size);
                    thisOffset = offset;
                }
                else
                {
                    padding = (offset % alignment);
                    if (0 != padding)
                        padding = alignment - padding;

                    thisOffset = offset;
                    offset += (padding + size);
                }
                
                lvo.Offset = new IntPtr(thisOffset);
            }

            return offset;
        }

        /// <summary>
        /// Logs all operands in <paramref name="locals"/>.
        /// </summary>
        /// <param name="locals">The operands to log.</param>
        private void LogOperands(List<StackOperand> locals)
        {
            foreach (StackOperand local in locals)
            {
                Trace.WriteLine(String.Format(@"\t{0} at {1}", local, local.Offset));
            }
        }

        /// <summary>
        /// Sorts all local variables by their size requirements.
        /// </summary>
        /// <param name="locals">Holds all local variables to sort..</param>
        /// <param name="cc">The calling convention used to determine size and alignment requirements.</param>
        private static void OrderVariables(List<StackOperand> locals, ICallingConvention cc)
        {
            /* Sort the list by stack size requirements - this moves equally sized operands closer together,
             * in the hope that this reduces padding on the stack to enforce HW alignment requirements.
             */
            locals.Sort(delegate(StackOperand op1, StackOperand op2)
            {
                int size1, size2, alignment;
                cc.GetStackRequirements(op1, out size1, out alignment);
                cc.GetStackRequirements(op2, out size2, out alignment);
                return size2 - size1;
            });
        }

        #endregion // Internals
    }
}