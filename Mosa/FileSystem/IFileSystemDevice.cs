﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

using Mosa.DeviceDrivers;

namespace Mosa.FileSystem
{
    /// <summary>
    /// 
    /// </summary>
	public interface IFileSystemDevice
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="partition"></param>
        /// <returns></returns>
		GenericFileSystem Create(IPartitionDevice partition);
	}
}