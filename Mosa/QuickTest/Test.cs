﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 */

namespace Mosa.QuickTest
{
	/// <summary>
	/// 
	/// </summary>
	public static class Test
	{
		/// <summary>
		/// Main
		/// </summary>
		public static void Main()
		{
			int x = 3;
			int y = 4 + x;
			int z = x * y;
			int a = z - 5;
			int b = a;
			if (a > z)
				b = a * 100;
			else
				b = a * 1000;
			int q = a / 10;

			//byte a = 11;
			//int b = a;// +22;
			//byte c = (byte)b;

			//long x = 16;
		}
	}
}
