﻿using DiscUtils;
using DiscUtils.Streams;
using System;
using System.Collections.Generic;

namespace Microsoft.CST.RecursiveExtractor.Extractors
{
    /// <summary>
    /// The VMDK Extractor Implementation
    /// </summary>
    public class VmdkExtractor : AsyncExtractorInterface
    {
        /// <summary>
        /// The constructor takes the Extractor context for recursion.
        /// </summary>
        /// <param name="context">The Extractor context.</param>
        public VmdkExtractor(Extractor context)
        {
            Context = context;
        }
        private readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal Extractor Context { get; }

        /// <summary>
        ///     Extracts an a VMDK file
        /// </summary>
        /// <param name="fileEntry"> </param>
        /// <returns> </returns>
        public async IAsyncEnumerable<FileEntry> ExtractAsync(FileEntry fileEntry, ExtractorOptions options, ResourceGovernor governor, bool topLevel = true)
        {
            using var disk = new DiscUtils.Vmdk.Disk(fileEntry.Content, Ownership.None);
            LogicalVolumeInfo[]? logicalVolumes = null;

            try
            {
                var manager = new VolumeManager(disk);
                logicalVolumes = manager.GetLogicalVolumes();
            }
            catch (Exception e)
            {
                Logger.Debug("Error reading {0} disk at {1} ({2}:{3})", disk.GetType(), fileEntry.FullPath, e.GetType(), e.Message);
            }

            if (logicalVolumes != null)
            {
                foreach (var volume in logicalVolumes)
                {
                    await foreach (var entry in DiscCommon.DumpLogicalVolumeAsync(volume, fileEntry.FullPath, options, governor, Context, fileEntry, topLevel))
                    {
                        yield return entry;
                    }
                }
            }
            else
            {
                if (options.ExtractSelfOnFail)
                {
                    yield return fileEntry;
                }
            }
        }

        /// <summary>
        ///     Extracts an a VMDK file
        /// </summary>
        /// <param name="fileEntry"> </param>
        /// <returns> </returns>
        public IEnumerable<FileEntry> Extract(FileEntry fileEntry, ExtractorOptions options, ResourceGovernor governor, bool topLevel = true)
        {
            using var disk = new DiscUtils.Vmdk.Disk(fileEntry.Content, Ownership.None);
            LogicalVolumeInfo[]? logicalVolumes = null;

            try
            {
                var manager = new VolumeManager(disk);
                logicalVolumes = manager.GetLogicalVolumes();
            }
            catch (Exception e)
            {
                Logger.Debug("Error reading {0} disk at {1} ({2}:{3})", disk.GetType(), fileEntry.FullPath, e.GetType(), e.Message);
            }

            if (logicalVolumes != null)
            {
                foreach (var volume in logicalVolumes)
                {
                    foreach (var entry in DiscCommon.DumpLogicalVolume(volume, fileEntry.FullPath, options, governor, Context, fileEntry, topLevel))
                    {
                        yield return entry;
                    }
                }
            }
            else
            {
                if (options.ExtractSelfOnFail)
                {
                    yield return fileEntry;
                }
            }
        }
    }
}
