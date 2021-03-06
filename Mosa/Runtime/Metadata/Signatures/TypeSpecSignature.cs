/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (grover) <sharpos@michaelruck.de>
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Mosa.Runtime.Metadata.Signatures
{
    /// <summary>
    /// 
    /// </summary>
    public class TypeSpecSignature : Signature
    {
        /// <summary>
        /// 
        /// </summary>
        private SigType _type;

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public SigType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Parses the signature.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="index">The index.</param>
        protected override void ParseSignature(byte[] buffer, ref int index)
        {
            _type = SigType.ParseTypeSignature(buffer, ref index);
        }
    }
}
