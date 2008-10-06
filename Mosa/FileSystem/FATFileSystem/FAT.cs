/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

using System;
using System.IO;
using Mosa.ClassLib;
using Mosa.DeviceDrivers;
using Mosa.FileSystem.VFS;

namespace Mosa.FileSystem.FATFileSystem
{

	#region Constants

    /// <summary>
    /// 
    /// </summary>
	internal struct BootSector
	{
		internal const uint JumpInstruction = 0x00; // 3 
		internal const uint EOMName = 0x03;	// 8 - "IBM  3.3", "MSDOS5.0", "MSWIN4.1", "FreeDOS"
		internal const uint BytesPerSector = 0x0B;	// 2 - common value 512
		internal const uint SectorsPerCluster = 0x0D;	// 1 - valid 1 to 128
		internal const uint ReservedSectors = 0x0E; // 2 - 1 for FAT12/FAT16, usually 32 for FAT32
		internal const uint FatAllocationTables = 0x10;	// 1 - always 2
		internal const uint MaxRootDirEntries = 0x11; // 2
		internal const uint TotalSectors = 0x13;	// 2
		internal const uint MediaDescriptor = 0x15; // 1
		internal const uint SectorsPerFAT = 0x16; // 2
		internal const uint SectorsPerTrack = 0x18;	// 2
		internal const uint NumberOfHeads = 0x1A;	// 2
		internal const uint HiddenSectors = 0x1C; // 4
		internal const uint FAT32_TotalSectors = 0x20; // 4

		// Extended BIOS Paremeter Block

		internal const uint PhysicalDriveNbr = 0x24; // 1
		internal const uint ReservedCurrentHead = 0x25; // 1
		internal const uint ExtendedBootSignature = 0x26; // 1 // value: 0x29 or 0x28
		internal const uint IDSerialNumber = 0x25; // 4
		internal const uint VolumeLabel = 0x2B; // 11
		internal const uint FATType = 0x36; // 8 - padded with blanks (0x20) "FAT12"; "FAT16"
		internal const uint OSBootCode = 0x3E; // 448 - Operating system boot code
		internal const uint BootSectorSignature = 0x1FE; // 2 - value: 0x55 0xaa

		// Fat32

		internal const uint FAT32_SectorPerFAT = 0x24; // 4
		internal const uint FAT32_Flags = 0x28; // 2
		internal const uint FAT32_Version = 0x2A; // 2
		internal const uint FAT32_ClusterNumberOfRoot = 0x2C; // 2
		internal const uint FAT32_SectorFSInformation = 0x30; // 2
		internal const uint FAT32_SecondBootSector = 0x32; // 2
		internal const uint FAT32_Reserved1 = 0x34; // 12
		internal const uint FAT32_PhysicalDriveNbr = 0x40; // 1
		internal const uint FAT32_Reserved2 = 0x40; // 1
		internal const uint FAT32_ExtendedBootSignature = 0x42; // 1
		internal const uint FAT32_IDSerialNumber = 0x43; // 4
		internal const uint FAT32_VolumeLabel = 0x47; // 2
		internal const uint FAT32_FATType = 0x52; // 2
		internal const uint FAT32_OSBootCode = 0x5A; // 2
	}

    /// <summary>
    /// 
    /// </summary>
	internal struct FSInfo
	{
		internal const uint FSI_LeadSignature = 0x00; // 4 - always 0x41615252
		internal const uint FSI_Reserved1 = 0x04; // 480 - always 0
		internal const uint FSI_StructureSigature = 484; // 4 - always 0x61417272
		internal const uint FSI_FreeCount = 488; // 4
		internal const uint FSI_NextFree = 492; // 4
		internal const uint FSI_Reserved2 = 496; // 4 - always 0
		internal const uint FSI_TrailSignature = 508; // 4 - always 0xAA550000
		internal const uint FSI_TrailSignature2 = 510; // 4 - always 0xAA55
	}

    /// <summary>
    /// 
    /// </summary>
	internal struct Entry
	{
		internal const uint DOSName = 0x00; // 8
		internal const uint DOSExtension = 0x08;	// 3
		internal const uint FileAttributes = 0x0B;	// 1
		internal const uint Reserved = 0x0C;	// 1
		internal const uint CreationTimeFine = 0x0D; // 1
		internal const uint CreationTime = 0x0E; // 2
		internal const uint CreationDate = 0x10; // 2
		internal const uint LastAccessDate = 0x12; // 2
		internal const uint EAIndex = 0x14; // 2
		internal const uint LastModifiedTime = 0x16; // 2
		internal const uint LastModifiedDate = 0x18; // 2
		internal const uint FirstCluster = 0x1A; // 2
		internal const uint FileSize = 0x1C; // 4
		internal const uint EntrySize = 32;
	}

    /// <summary>
    /// 
    /// </summary>
	[Flags]
	public enum FileAttributes : byte
	{
        /// <summary>
        /// 
        /// </summary>
		ReadOnly = 0x01,
        /// <summary>
        /// 
        /// </summary>
		Hidden = 0x02,
        /// <summary>
        /// 
        /// </summary>
		System = 0x04,
        /// <summary>
        /// 
        /// </summary>
		VolumeLabel = 0x08,
        /// <summary>
        /// 
        /// </summary>
		SubDirectory = 0x10,
        /// <summary>
        /// 
        /// </summary>
		Archive = 0x20,
        /// <summary>
        /// 
        /// </summary>
		Device = 0x40,
        /// <summary>
        /// 
        /// </summary>
		Unused = 0x80,
        /// <summary>
        /// 
        /// </summary>
		LongFileName = 0x0F
	}

    /// <summary>
    /// 
    /// </summary>
	internal struct FileNameAttribute
	{
		internal const uint LastEntry = 0x00;
		internal const uint Escape = 0x05;	// special msdos hack where 0x05 really means 0xE5 (since 0xE5 was already used for delete
		internal const uint Dot = 0x2E;
		internal const uint Deleted = 0xE5;
	}

    /// <summary>
    /// 
    /// </summary>
	public enum FATType : byte
	{
        /// <summary>
        /// 
        /// </summary>
		FAT12 = 12,
        /// <summary>
        /// 
        /// </summary>
		FAT16 = 16,
        /// <summary>
        /// 
        /// </summary>
		FAT32 = 32
	}

	#endregion

    /// <summary>
    /// 
    /// </summary>
	public class FAT : GenericFileSystem
	{
		// limitations: fat32 and vfat (long files) are not supported
		// plus almost all testing has been against fat12 (not fat16)

        /// <summary>
        /// 
        /// </summary>
		public interface ICompare
		{
            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="offset"></param>
            /// <param name="type"></param>
            /// <returns></returns>
			bool Compare(byte[] data, uint offset, FATType type);
		}

        /// <summary>
        /// 
        /// </summary>
		private FATType fatType;

        /// <summary>
        /// 
        /// </summary>
		private uint last;

        /// <summary>
        /// 
        /// </summary>
		private uint bad;

        /// <summary>
        /// 
        /// </summary>
		private uint reserved;

        /// <summary>
        /// 
        /// </summary>
		private uint fatMask;

        /// <summary>
        /// 
        /// </summary>
		private uint bytesPerSector;

        /// <summary>
        /// 
        /// </summary>
		private byte sectorsPerCluster;

        /// <summary>
        /// 
        /// </summary>
		private byte reservedSectors;

        /// <summary>
        /// 
        /// </summary>
		private byte nbrFats;

        /// <summary>
        /// 
        /// </summary>
		private uint rootEntries;

        /// <summary>
        /// 
        /// </summary>
		private uint totalClusters;

        /// <summary>
        /// 
        /// </summary>
		private uint fatStart;

        /// <summary>
        /// 
        /// </summary>
		private uint rootDirSectors;

        /// <summary>
        /// 
        /// </summary>
		private uint firstDataSector;

        /// <summary>
        /// 
        /// </summary>
		private uint totalSectors;

        /// <summary>
        /// 
        /// </summary>
		private uint dataSectors;

        /// <summary>
        /// 
        /// </summary>
		private uint dataAreaStart;

        /// <summary>
        /// 
        /// </summary>
		private uint entriesPerSector;

        /// <summary>
        /// 
        /// </summary>
		private uint firstRootDirectorySector;

        /// <summary>
        /// 
        /// </summary>
		private uint fatEntries;

        /// <summary>
        /// 
        /// </summary>
		private uint clusterSizeInBytes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partition"></param>
		public FAT(IPartitionDevice partition)
			: base(partition)
		{
			ReadBootSector();
		}

        /// <summary>
        /// 
        /// </summary>
		public object SettingsType 
        { get { return new FATSettings(); } 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public override IFileSystem CreateVFSMount()
		{
			return new VFSFileSystem(this);
		}

        /// <summary>
        /// 
        /// </summary>
		public bool IsReadOnly { get { return true; } }

        /// <summary>
        /// 
        /// </summary>
		public uint ClusterSizeInBytes { get { return clusterSizeInBytes; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
		public byte[] ReadCluster(uint cluster)
		{
			return partition.ReadBlock(dataAreaStart + ((cluster - 1) * (uint)sectorsPerCluster), clusterSizeInBytes / blockSize);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="block"></param>
        /// <returns></returns>
		public bool ReadCluster(uint cluster, byte[] block)
		{
			return partition.ReadBlock(dataAreaStart + ((cluster - 1) * (uint)sectorsPerCluster), clusterSizeInBytes / blockSize, block);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="block"></param>
        /// <returns></returns>
		public bool WriteCluster(uint cluster, byte[] block)
		{
			return partition.WriteBlock(dataAreaStart + ((cluster - 1) * (uint)sectorsPerCluster), clusterSizeInBytes / blockSize, block);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		protected bool ReadBootSector()
		{
			valid = false;

			if (blockSize != 512)	// only going to work with 512 sector sizes (for now)
				return false;

			BinaryFormat bootSector = new BinaryFormat(partition.ReadBlock(0, 1));

			byte bootSignature = bootSector.GetByte(BootSector.ExtendedBootSignature);

			if ((bootSignature != 0x29) && (bootSignature != 0x28))
				return false;

			//TextMode.Write ("EOM NAME: ");

			//for (uint i = 0; i < 8; i++)
			//    TextMode.WriteChar (bootsector.GetByte (BootSector.EOMName + i));

			//TextMode.WriteLine ();

			bytesPerSector = bootSector.GetUShort(BootSector.BytesPerSector);
			sectorsPerCluster = bootSector.GetByte(BootSector.SectorsPerCluster);
			reservedSectors = bootSector.GetByte(BootSector.ReservedSectors);
			nbrFats = bootSector.GetByte(BootSector.FatAllocationTables);
			rootEntries = bootSector.GetUShort(BootSector.MaxRootDirEntries);

			uint sectorsPerFat16 = bootSector.GetUShort(BootSector.SectorsPerFAT);
			uint sectorsPerFat32 = bootSector.GetUInt(BootSector.FAT32_SectorPerFAT);
			uint totalSectors16 = bootSector.GetUShort(BootSector.TotalSectors);
			uint totalSectors32 = bootSector.GetUInt(BootSector.FAT32_TotalSectors);
			uint sectorsPerFat = (sectorsPerFat16 != 0) ? sectorsPerFat16 : sectorsPerFat32;
			uint fatSectors = nbrFats * sectorsPerFat;

			clusterSizeInBytes = sectorsPerCluster * blockSize;
			rootDirSectors = (((rootEntries * 32) + (bytesPerSector - 1)) / bytesPerSector);
			firstDataSector = reservedSectors + (nbrFats * sectorsPerFat) + rootDirSectors;

			if (totalSectors16 != 0)
				totalSectors = totalSectors16;
			else
				totalSectors = totalSectors32;

			dataSectors = totalSectors - (reservedSectors + (nbrFats * sectorsPerFat) + rootDirSectors);
			totalClusters = dataSectors / sectorsPerCluster;
			entriesPerSector = (bytesPerSector / 32);
			firstRootDirectorySector = reservedSectors + fatSectors;
			dataAreaStart = firstRootDirectorySector + rootDirSectors;
			fatStart = reservedSectors;

			if (totalClusters < 4085)
				fatType = FATType.FAT12;
			else if (totalClusters < 65525)
				fatType = FATType.FAT16;
			else
				fatType = FATType.FAT32;

			if (fatType == FATType.FAT12) {
				reserved = 0xFF0;
				last = 0x0FF8;
				bad = 0x0FF7;
				fatMask = 0xFFFFFFFF;
				fatEntries = sectorsPerFat * 3 * blockSize / 2;
			}
			else if (fatType == FATType.FAT16) {
				reserved = 0xFFF0;
				last = 0xFFF8;
				bad = 0xFFF7;
				fatMask = 0xFFFFFFFF;
				fatEntries = sectorsPerFat * blockSize / 2;
			}
			else { //  if (type == FatType.FAT32) {
				reserved = 0xFFF0;
				last = 0x0FFFFFF8;
				bad = 0x0FFFFFF7;
				fatMask = 0x0FFFFFFF;
				fatEntries = sectorsPerFat * blockSize / 4;
			}

			// some basic checks 

			if ((nbrFats == 0) || (nbrFats > 2))
				valid = false;
			else if (totalSectors == 0)
				valid = false;
			else if (sectorsPerFat == 0)
				valid = false;
			else if (!((fatType == FATType.FAT12) || (fatType == FATType.FAT16))) // no support for Fat32 yet
				valid = false;
			else
				valid = true;

			if (valid) {
				//FIXME:base.volumeLabel = bootSector.GetString (fatType != FATType.FAT32 ? BootSector.VolumeLabel : BootSector.FAT32_VolumeLabel, 11);
				base.serialNbr = bootSector.GetBytes(fatType != FATType.FAT32 ? BootSector.IDSerialNumber : BootSector.FAT32_IDSerialNumber, 4);
			}

			return valid;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fatSettings"></param>
        /// <returns></returns>
		public bool Format(FATSettings fatSettings)
		{
			if (!partition.CanWrite)
				return false;

			this.fatType = fatSettings.FatType;
			bytesPerSector = 512;

			totalSectors = partition.BlockCount;

			sectorsPerCluster = GetSectorsPerClusterByTotalSectors(fatType, totalSectors);
			nbrFats = 2;

			if (fatType == FATType.FAT32) {
				reservedSectors = 32;
				rootEntries = 0;
			}
			else {
				reservedSectors = 1;
				rootEntries = 512;
			}

			rootDirSectors = (((rootEntries * 32) + (bytesPerSector - 1)) / bytesPerSector);
			fatStart = reservedSectors;

			uint val1 = totalSectors - (reservedSectors + rootDirSectors);
			uint val2 = (uint)((256 * sectorsPerCluster) + nbrFats);

			if (fatType == FATType.FAT32)
				val2 = val2 / 2;

			uint sectorsperfat = (val1 + (val2 - 1)) / val2;

			BinaryFormat bootSector = new BinaryFormat(512);

			bootSector.SetUInt(BootSector.JumpInstruction, 0);
			bootSector.SetString(BootSector.EOMName, "MSWIN4.1");
			bootSector.SetUShort(BootSector.BytesPerSector, (ushort)bytesPerSector);
			bootSector.SetByte(BootSector.SectorsPerCluster, (byte)sectorsPerCluster);
			bootSector.SetUShort(BootSector.ReservedSectors, (ushort)reservedSectors);
			bootSector.SetByte(BootSector.FatAllocationTables, nbrFats);
			bootSector.SetUShort(BootSector.MaxRootDirEntries, (ushort)rootEntries);

			if (totalSectors > 0xFFFF) {
				bootSector.SetUShort(BootSector.TotalSectors, 0);
				bootSector.SetUInt(BootSector.FAT32_TotalSectors, totalClusters);
			}
			else {
				bootSector.SetUShort(BootSector.TotalSectors, (ushort)totalSectors);
				bootSector.SetUInt(BootSector.FAT32_TotalSectors, 0);
			}

			bootSector.SetByte(BootSector.MediaDescriptor, 0xF8);

			if (fatType == FATType.FAT32)
				bootSector.SetUShort(BootSector.SectorsPerFAT, 0);
			else
				bootSector.SetUShort(BootSector.SectorsPerFAT, (ushort)sectorsperfat);

			bootSector.SetUShort(BootSector.SectorsPerTrack, 0); ////FIXME
			bootSector.SetUInt(BootSector.HiddenSectors, 0);

			if (fatType != FATType.FAT32) {
				bootSector.SetByte(BootSector.PhysicalDriveNbr, 0x80);
				bootSector.SetByte(BootSector.ReservedCurrentHead, 0);
				bootSector.SetByte(BootSector.ExtendedBootSignature, 0x29);
				bootSector.SetBytes(BootSector.IDSerialNumber, fatSettings.SerialID, 0, (uint)(fatSettings.SerialID.Length <= 4 ? fatSettings.SerialID.Length : 4));
				bootSector.SetString(BootSector.VolumeLabel, "            ");  // 12 blank spaces
				bootSector.SetString(BootSector.VolumeLabel, fatSettings.VolumeLabel, (uint)(fatSettings.VolumeLabel.Length <= 12 ? fatSettings.VolumeLabel.Length : 12));
				bootSector.SetUShort(BootSector.BootSectorSignature, 0x55AA);
				//BootSector.OSBootCode // TODO
			}

			if (fatType == FATType.FAT12)
				bootSector.SetString(BootSector.FATType, "FAT12   ");
			else if (fatType == FATType.FAT16)
				bootSector.SetString(BootSector.FATType, "FAT16   ");
			else // if (type == FatType.FAT32)
				bootSector.SetString(BootSector.FATType, "FAT32   ");

			if (fatType == FATType.FAT32) {
				bootSector.SetUInt(BootSector.FAT32_SectorPerFAT, sectorsperfat);
				bootSector.SetByte(BootSector.FAT32_Flags, 0);
				bootSector.SetUShort(BootSector.FAT32_Version, 0);
				bootSector.SetUInt(BootSector.FAT32_ClusterNumberOfRoot, 2);
				bootSector.SetUShort(BootSector.FAT32_SectorFSInformation, 1);
				bootSector.SetUShort(BootSector.FAT32_SecondBootSector, 6);
				//FAT32_Reserved1
				bootSector.SetByte(BootSector.FAT32_PhysicalDriveNbr, 0x80);
				bootSector.SetByte(BootSector.FAT32_Reserved2, 0);
				bootSector.SetByte(BootSector.FAT32_ExtendedBootSignature, 0x29);
				bootSector.SetBytes(BootSector.FAT32_IDSerialNumber, fatSettings.SerialID, 0, (uint)(fatSettings.SerialID.Length <= 4 ? fatSettings.SerialID.Length : 4));
				bootSector.SetString(BootSector.FAT32_VolumeLabel, "            ");  // 12 blank spaces
				bootSector.SetString(BootSector.FAT32_VolumeLabel, fatSettings.VolumeLabel, (uint)(fatSettings.VolumeLabel.Length <= 12 ? fatSettings.VolumeLabel.Length : 12));
				bootSector.SetString(BootSector.FAT32_FATType, "FAT32   ");
				//BootSector.OSBootCode // TODO
			}

			// Write Boot Sector
			partition.WriteBlock(0, 1, bootSector.Data);

			// Write backup Boot Sector
			if (fatType == FATType.FAT32) {
				partition.WriteBlock(0, 1, bootSector.Data);	// FIXME: wrong block #
			}

			// create FSInfo Structure
			if (fatType == FATType.FAT32) {
				BinaryFormat infoSector = new BinaryFormat(512);

				infoSector.SetUInt(FSInfo.FSI_LeadSignature, 0x41615252);
				//FSInfo.FSI_Reserved1
				infoSector.SetUInt(FSInfo.FSI_StructureSigature, 0x61417272);
				infoSector.SetUInt(FSInfo.FSI_FreeCount, 0xFFFFFFFF);
				infoSector.SetUInt(FSInfo.FSI_NextFree, 0xFFFFFFFF);
				//FSInfo.FSI_Reserved2
				bootSector.SetUInt(FSInfo.FSI_TrailSignature, 0xAA550000);

				partition.WriteBlock(1, 1, infoSector.Data);
				partition.WriteBlock(7, 1, infoSector.Data);

				// create 2nd sector
				BinaryFormat secondSector = new BinaryFormat(512);

				secondSector.SetUInt((ushort)FSInfo.FSI_TrailSignature2, 0xAA55);

				partition.WriteBlock(2, 1, secondSector.Data);
				partition.WriteBlock(8, 1, secondSector.Data);
			}

			// create fats
			// TODO: incomplete
			BinaryFormat emptyFat = new BinaryFormat(512);

			// clear primary & secondary fats entries
			for (uint i = fatStart; i < fatStart + 1; i++) {	//FIXME
				partition.WriteBlock(i, 1, emptyFat.Data);
			}

			// first block is special
			BinaryFormat firstFat = new BinaryFormat(512);
			// TODO: incomplete
			partition.WriteBlock(reservedSectors, 1, emptyFat.Data);

			return ReadBootSector();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
		protected bool IsClusterFree(uint cluster)
		{
			return ((cluster & fatMask) == 0x00);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
		protected bool IsClusterReserved(uint cluster)
		{
			return (((cluster & fatMask) == 0x00) || ((cluster & fatMask) >= reserved) && ((cluster & fatMask) < bad));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
		protected bool IsClusterBad(uint cluster)
		{
			return ((cluster & fatMask) == bad);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
		protected bool IsClusterLast(uint cluster)
		{
			return ((cluster & fatMask) >= last);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
		protected bool IsUsed(uint cluster)
		{
			return !IsClusterFree(cluster) && !IsClusterReserved(cluster) && !IsClusterBad(cluster) && !IsClusterLast(cluster);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sector"></param>
        /// <returns></returns>
		protected uint GetClusterBySector(uint sector)
		{
			if (sector < dataAreaStart)
				return 0;

			return (sector - dataAreaStart) / sectorsPerCluster;
		}

		//protected uint ClusterToFirstSector (uint cluster)
		//{
		//    return ((cluster - 2) * sectorspercluster) + firstdatasector;
		//}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
		protected uint GetClusterEntryValue(uint cluster)
		{
			uint fatoffset = 0;

			if (fatType == FATType.FAT12)
				fatoffset = (cluster + (cluster / 2));
			else if (fatType == FATType.FAT16)
				fatoffset = cluster * 2;
			else //if (type == FatType.FAT32)
				fatoffset = cluster * 4;

			uint sector = fatStart + (fatoffset / bytesPerSector);
			uint sectorOffset = fatoffset % bytesPerSector;
			uint nbrSectors = 1;

			if ((fatType == FATType.FAT12) && (sectorOffset == bytesPerSector - 1))
				nbrSectors = 2;

			BinaryFormat fat = new BinaryFormat(partition.ReadBlock(sector, nbrSectors));

			uint clusterValue;

			if (fatType == FATType.FAT12) {
				clusterValue = fat.GetUShort(sectorOffset);
				if (cluster % 2 == 1)
					clusterValue = clusterValue >> 4;
				else
					clusterValue = clusterValue & 0x0fff;
			}
			else if (fatType == FATType.FAT16)
				clusterValue = fat.GetUShort(sectorOffset);
			else //if (type == FatType.FAT32)
				clusterValue = fat.GetUInt(sectorOffset) & 0x0fffffff;

			return clusterValue;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="nextcluster"></param>
        /// <returns></returns>
		protected bool SetClusterEntryValue(uint cluster, uint nextcluster)
		{
			uint fatOffset = 0;

			if (fatType == FATType.FAT12)
				fatOffset = (cluster + (cluster / 2));
			else if (fatType == FATType.FAT16)
				fatOffset = cluster * 2;
			else //if (type == FatType.FAT32)
				fatOffset = cluster * 4;

			uint sector = fatStart + (fatOffset / bytesPerSector);
			uint sectorOffset = fatOffset % bytesPerSector;
			uint nbrSectors = 1;

			if ((fatType == FATType.FAT12) && (sectorOffset == bytesPerSector - 1))
				nbrSectors = 2;

			BinaryFormat fat = new BinaryFormat(partition.ReadBlock(sector, nbrSectors));

			if (fatType == FATType.FAT12) {
				uint clustervalue = fat.GetUShort(sectorOffset);

				if (cluster % 2 == 1)
					clustervalue = ((clustervalue & 0xF) | (nextcluster << 4));
				else
					clustervalue = ((clustervalue & 0xf000) | (nextcluster));

				fat.SetUShort(sectorOffset, (ushort)clustervalue);
			}
			else if (fatType == FATType.FAT16)
				fat.SetUShort(sectorOffset, (ushort)nextcluster);
			else //if (type == FatType.FAT32)
				fat.SetUInt(sectorOffset, nextcluster);

			partition.WriteBlock(sector, nbrSectors, fat.Data);

			return true;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sectors"></param>
        /// <returns></returns>
		public static byte GetSectorsPerClusterByTotalSectors(FATType type, uint sectors)
		{
			switch (type) {
				case FATType.FAT12: {
						if (sectors < 512) return 1;
						else if (sectors == 720) return 2;
						else if (sectors == 1440) return 2;
						else if (sectors <= 2880) return 1;
						else if (sectors <= 5760) return 2;
						else if (sectors <= 16384) return 4;
						else if (sectors <= 32768) return 8;
						else return 0;
					}
				case FATType.FAT16: {
						if (sectors < 8400) return 0;
						else if (sectors < 32680) return 2;
						else if (sectors < 262144) return 4;
						else if (sectors < 524288) return 8;
						else if (sectors < 1048576) return 16;
						else if (sectors < 2097152) return 32;
						else if (sectors < 4194304) return 64;
						else return 0;
					}
				case FATType.FAT32: {
						if (sectors < 66600) return 0;
						else if (sectors < 532480) return 1;
						else if (sectors < 16777216) return 8;
						else if (sectors < 33554432) return 16;
						else if (sectors < 67108864) return 32;
						else return 64;
					}
				default: return 0;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="index"></param>
        /// <returns></returns>
		public static string ExtractFileName(byte[] directory, uint index)
		{
			// rewrite to use string
			BinaryFormat entry = new BinaryFormat(directory);

			char[] name = new char[12];

			for (uint i = 0; i < 8; i++)
				name[i] = (char)entry.GetByte(index + i + Entry.DOSName);

			int len = 8;

			for (int i = 7; i > 0; i--)
				if (name[i] == ' ')
					len--;
				else
					break;

			// special case where real character is same as the delete
			if ((len >= 1) && (name[0] == (char)FileNameAttribute.Escape))
				name[0] = (char)FileNameAttribute.Deleted;

			name[len] = '.';

			len++;

			for (uint i = 0; i < 3; i++)
				name[len + i] = (char)entry.GetByte(index + i + Entry.DOSExtension);

			len = len + 3;

			int spaces = 0;
			for (int i = len - 1; i >= 0; i--)
				if (name[i] == ' ')
					spaces++;
				else
					break;

			if (spaces == 3)
				spaces = 4;

			len = len - spaces;

			return name.ToString();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
		protected static bool IsValidFatCharacter(char c)
		{
			if ((c >= 'A') || (c <= 'Z'))
				return true;
			if ((c >= '0') || (c <= '9'))
				return true;
			if ((c >= 128) || (c <= 255))
				return true;

			string valid = " !#$%&'()-@^_`{}~";

			for (int i = 0; i < valid.Length; i++)
				if (valid[i] == c)
					return true;

			return false;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <returns></returns>
		static public uint GetClusterEntry(byte[] data, uint index, FATType type)
		{
			BinaryFormat entry = new BinaryFormat(data);

			uint cluster = entry.GetUShort((index * Entry.EntrySize) + Entry.FirstCluster);

			if (type == FATType.FAT32) {
				uint clusterhi = ((uint)entry.GetUShort((index * Entry.EntrySize) + Entry.EAIndex)) << 16;
				cluster = cluster | clusterhi;
			}

			return cluster;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compare"></param>
        /// <param name="startCluster"></param>
        /// <returns></returns>
		public DirectoryEntryLocation FindEntry(FAT.ICompare compare, uint startCluster)
		{
			uint activeSector = (startCluster == 0) ? firstRootDirectorySector : (startCluster * this.sectorsPerCluster);
			uint increment = 0;

			for (; ; ) {
				BinaryFormat directory = new BinaryFormat(partition.ReadBlock(activeSector, 1));

				for (uint index = 0; index < entriesPerSector; index++) {
					if (directory.GetByte((index * Entry.EntrySize) + Entry.DOSName) == FileNameAttribute.LastEntry)
						return new DirectoryEntryLocation();

					FileAttributes attribute = (FileAttributes)directory.GetByte((index * Entry.EntrySize) + Entry.FileAttributes);

					if (compare.Compare(directory.Data, index * 32, fatType))
						return new DirectoryEntryLocation(GetClusterEntry(directory.Data, index, fatType), activeSector, index, (attribute & FileAttributes.SubDirectory) != 0);
				}

				++increment;

				if (startCluster == 0) {
					// root directory
					if (increment >= rootDirSectors)
						return new DirectoryEntryLocation();

					activeSector = startCluster + increment;
					continue;
				}
				else {
					// subdirectory
					if (increment < sectorsPerCluster) {
						// still within cluster
						activeSector = startCluster + increment;
						continue;
					}
					// exiting cluster

					// goto next cluster (if any)
					uint cluster = GetClusterBySector(startCluster);

					if (cluster == 0)
						return new DirectoryEntryLocation();

					uint nextCluster = GetClusterEntryValue(cluster);

					if ((IsClusterLast(nextCluster)) || (IsClusterBad(nextCluster)) || (IsClusterFree(nextCluster)) || (IsClusterReserved(nextCluster)))
						return new DirectoryEntryLocation();

					activeSector = (uint)(dataAreaStart + (nextCluster - 1 * sectorsPerCluster));

					continue;
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryBlock"></param>
        /// <param name="index"></param>
        /// <returns></returns>
		public uint GetFileSize(uint directoryBlock, uint index)
		{
			BinaryFormat directory = new BinaryFormat(partition.ReadBlock(directoryBlock, 1));

			return directory.GetUInt((index * Entry.EntrySize) + Entry.FileSize);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childBlock"></param>
        /// <param name="parentBlock"></param>
        /// <param name="parentBlockIndex"></param>
		public void Delete(uint childBlock, uint parentBlock, uint parentBlockIndex)
		{
			BinaryFormat entry = new BinaryFormat(partition.ReadBlock(parentBlock, 1));

			entry.SetByte((parentBlockIndex * Entry.EntrySize) + Entry.DOSName, (byte)FileNameAttribute.Deleted);

			partition.WriteBlock(parentBlock, 1, entry.Data);

			if (!FreeClusterChain(childBlock))
				throw new System.ArgumentException();	//throw new IOException ("Unable to free all cluster allocations in fat");
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <returns></returns>
		protected bool FreeClusterChain(uint first)
		{
			//TODO: add locking
			uint at = first;

			while (true) {
				uint next = GetClusterEntryValue(first);
				SetClusterEntryValue(at, 0);

				if (IsClusterLast(next))
					return true;

				if (IsClusterFree(next) || IsClusterBad(next) || IsClusterReserved(next))
					return false;

				at = next;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
		public uint FindNthCluster(uint start, uint count)
		{
			// TODO: add locking
			uint at = start;

			for (int i = 0; i < count; i++) {
				at = GetClusterEntryValue(at);

				if (IsClusterLast(at))
					return 0;

				if (IsClusterFree(at) || IsClusterBad(at) || IsClusterReserved(at))
					return 0;
			}

			return at;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
		public uint NextCluster(uint start)
		{
			// TODO: add locking
			uint at = GetClusterEntryValue(start);

			if (IsClusterLast(at))
				return 0;

			if (IsClusterFree(at) || IsClusterBad(at) || IsClusterReserved(at))
				return 0;

			return at;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		protected uint AllocateCluster()
		{
			//TODO: add locking
			//TODO: improve performance by scanning the block directly for free entries (FAT16 & 32 only)
			//TODO: keep cache of last allocation and re-use as next starting location
			uint at = 0;

			while (at < fatEntries) {
				uint value = GetClusterEntryValue(at);

				if (IsClusterFree(value)) {
					SetClusterEntryValue(at, 0xFFFFFFFF);
					return at;
				}
				at++;
			}

			return 0;	// mean no free space
		}

		//protected OpenFile ExtractFileInformation (MemoryBlock directory, uint index, OpenFile parent)
		//{
		//    uint offset = index * 32;

		//    byte first = directory.GetByte (offset + Entry.DOSName);

		//    if ((first == FileNameAttribute.LastEntry) || (first == FileNameAttribute.Deleted))
		//        return null;

		//    FileAttributes attribute = (FileAttributes)directory.GetByte (offset + Entry.FileAttributes);

		//    if (attribute == FileAttributes.LongFileName)
		//        return null;	// long file names are not supported

		//    byte second = directory.GetByte (offset + Entry.DOSName);

		//    if ((first == FileNameAttribute.Dot) && (first == FileNameAttribute.Dot))
		//        return null;

		//    OpenFile file = new OpenFile ();

		//    if ((attribute & FileAttributes.SubDirectory) != 0)
		//        file.Type = FileType.Directory;
		//    else
		//        file.Type = FileType.File;

		//    file.ReadOnly = ((attribute & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
		//    file.Hidden = ((attribute & FileAttributes.Hidden) == FileAttributes.Hidden);
		//    file.Archive = ((attribute & FileAttributes.Archive) == FileAttributes.Archive);
		//    file.System = ((attribute & FileAttributes.System) == FileAttributes.System);
		//    file.Size = directory.GetUInt (offset + Entry.FileSize);

		//    //TODO: build file name name.Trim()+'.'+ext.Trim()
		//    //string name = ByteBuffer.GetString(directory, 8, offset + Entry.DOSName);
		//    //string ext = ByteBuffer.GetString(directory, 3, offset + Entry.DOSExtension);

		//    file.Name = ExtractFileName (directory.Offset (index * 32));
		//    ushort cdate = directory.GetUShort (offset + Entry.CreationDate);
		//    ushort ctime = directory.GetUShort (offset + Entry.CreationTime);
		//    ushort mtime = directory.GetUShort (offset + Entry.LastModifiedTime);
		//    ushort mdate = directory.GetUShort (offset + Entry.LastModifiedDate);
		//    ushort adate = directory.GetUShort (offset + Entry.LastAccessDate);
		//    ushort msec = (ushort)(directory.GetByte (offset + Entry.CreationTimeFine) * 10);

		//    file.CreateTime.Year = (ushort)((cdate >> 9) + 1980);
		//    file.CreateTime.Month = (ushort)(((cdate >> 5) - 1) & 0x0F);
		//    file.CreateTime.Day = (ushort)(cdate & 0x1F);
		//    file.CreateTime.Hour = (ushort)(ctime >> 11);
		//    file.CreateTime.Month = (ushort)((ctime >> 5) & 0x0F);
		//    file.CreateTime.Second = (ushort)(((ctime & 0x1F) * 2) + (msec / 100));
		//    file.CreateTime.Milliseconds = (ushort)(msec / 20);

		//    file.LastModifiedTime.Year = (ushort)((mdate >> 9) + 1980);
		//    file.LastModifiedTime.Month = (ushort)((mdate >> 5) & 0x0F);
		//    file.LastModifiedTime.Day = (ushort)(mdate & 0x1F);
		//    file.LastModifiedTime.Hour = (ushort)(mtime >> 11);
		//    file.LastModifiedTime.Minute = (ushort)((mtime >> 5) & 0x3F);
		//    file.LastModifiedTime.Second = (ushort)((mtime & 0x1F) * 2);
		//    file.LastModifiedTime.Milliseconds = 0;

		//    file.LastAccessTime.Year = (ushort)((adate >> 9) + 1980);
		//    file.LastAccessTime.Month = (ushort)((adate >> 5) & 0x0F);
		//    file.LastAccessTime.Day = (ushort)(adate & 0x1F);

		//    file.Directory = parent;
		//    file._startdisklocation = directory.GetUShort (offset + Entry.FirstCluster);

		//    if (file.Type == FileType.Directory)
		//        file._startdisklocation = dataareastart + ((file._startdisklocation - 2) * sectorspercluster);

		//    file._position = 0;
		//    file._count = 0;

		//    return file;
		//}

		//protected OpenFile GetRootDirectory ()
		//{
		//    OpenFile file = new OpenFile ();

		//    file.Type = FileType.Root;
		//    file.ReadOnly = true;
		//    file.Hidden = false;
		//    file.Archive = false;
		//    file.System = true;
		//    file.Size = 0;

		//    file.Name = null;
		//    file.CreateTime.Year = 0;
		//    file.CreateTime.Month = 0;
		//    file.CreateTime.Day = 0;
		//    file.CreateTime.Hour = 0;
		//    file.CreateTime.Month = 0;
		//    file.CreateTime.Second = 0;
		//    file.CreateTime.Milliseconds = 0;

		//    file.LastModifiedTime.Year = 0;
		//    file.LastModifiedTime.Month = 0;
		//    file.LastModifiedTime.Day = 0;
		//    file.LastModifiedTime.Hour = 0;
		//    file.LastModifiedTime.Minute = 0;
		//    file.LastModifiedTime.Second = 0;
		//    file.LastModifiedTime.Milliseconds = 0;

		//    file.LastAccessTime.Year = 0;
		//    file.LastAccessTime.Month = 0;
		//    file.LastAccessTime.Day = 0;

		//    file.Directory = null;

		//    file._startdisklocation = 0; // rootstartingsector;
		//    file._position = 0;
		//    file._count = 0;

		//    return file;
		//}

        /// <summary>
        /// 
        /// </summary>
		public class DirectoryEntryLocation
		{
            /// <summary>
            /// 
            /// </summary>
			public bool Valid;

            /// <summary>
            /// 
            /// </summary>
			public uint Block;

            /// <summary>
            /// 
            /// </summary>
			public uint DirectorySector;

            /// <summary>
            /// 
            /// </summary>
			public uint DirectoryIndex;

            /// <summary>
            /// 
            /// </summary>
			private bool directory;

            /// <summary>
            /// 
            /// </summary>
			public bool IsDirectory
			{
				get
				{
					return directory;
				}
			}

            /// <summary>
            /// 
            /// </summary>
			public DirectoryEntryLocation()
			{
				this.Valid = false;
			}

            /// <summary>
            /// 
            /// </summary>
            /// <param name="block"></param>
            /// <param name="directorySector"></param>
            /// <param name="directoryIndex"></param>
            /// <param name="directory"></param>
			public DirectoryEntryLocation(uint block, uint directorySector, uint directoryIndex, bool directory)
			{
				this.Valid = true;
				this.Block = block;
				this.DirectorySector = directorySector;
				this.DirectoryIndex = directoryIndex;
				this.directory = directory;
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public class FatMatchClusterComparer : FAT.ICompare
		{
            /// <summary>
            /// 
            /// </summary>
			protected uint cluster;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="cluster"></param>
			public FatMatchClusterComparer(uint cluster)
			{
				this.cluster = cluster;
			}

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="offset"></param>
            /// <param name="type"></param>
            /// <returns></returns>
			public bool Compare(byte[] data, uint offset, FATType type)
			{
				BinaryFormat entry = new BinaryFormat(data);

				byte first = entry.GetByte(offset + Entry.DOSName);

				if (first == FileNameAttribute.LastEntry)
					return false;

				if ((first == FileNameAttribute.Deleted) | (first == FileNameAttribute.Dot))
					return false;

				if (first == FileNameAttribute.Escape)
					return false;

				uint startcluster = FAT.GetClusterEntry(data, offset, type);

				if (startcluster == cluster)
					return true;

				return false;
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public class FatAnyExistComparer : FAT.ICompare
		{
            /// <summary>
            /// 
            /// </summary>
			protected uint cluster;

            /// <summary>
            /// 
            /// </summary>
			public FatAnyExistComparer()
			{
			}

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="offset"></param>
            /// <param name="type"></param>
            /// <returns></returns>
			public bool Compare(byte[] data, uint offset, FATType type)
			{
				BinaryFormat entry = new BinaryFormat(data);

				byte first = entry.GetByte(offset + Entry.DOSName);

				if (first == FileNameAttribute.LastEntry)
					return false;

				if ((first == FileNameAttribute.Deleted) | (first == FileNameAttribute.Dot))
					return false;

				if (first == FileNameAttribute.Escape)
					return false;

				return true;
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public class FatEntityComparer : FAT.ICompare
		{
            /// <summary>
            /// 
            /// </summary>
			protected string name;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="name"></param>
			public FatEntityComparer(string name)
			{
				this.name = name;
			}

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="offset"></param>
            /// <param name="type"></param>
            /// <returns></returns>
			public bool Compare(byte[] data, uint offset, FATType type)
			{
				BinaryFormat entry = new BinaryFormat(data);

				byte first = entry.GetByte(offset + Entry.DOSName);

				if (first == FileNameAttribute.LastEntry)
					return false;

				if ((first == FileNameAttribute.Deleted) | (first == FileNameAttribute.Dot))
					return false;

				if (first == FileNameAttribute.Escape)
					return false;

				string entryname = FAT.ExtractFileName(data, offset);

				if (entryname == name)
					return true;

				return false;
			}
		}
	}
}