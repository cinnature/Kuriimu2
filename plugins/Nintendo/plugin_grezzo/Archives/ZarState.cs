﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interfaces.FileSystem;
using Kontract.Interfaces.Plugins.State;
using Kontract.Interfaces.Plugins.State.Archive;
using Kontract.Interfaces.Progress;
using Kontract.Interfaces.Providers;
using Kontract.Models.Archive;
using Kontract.Models.Context;
using Kontract.Models.IO;

namespace plugin_grezzo.Archives
{
    class ZarState : IArchiveState, ILoadFiles, ISaveFiles, IReplaceFiles
    {
        private readonly ZAR _zar;

        public IList<ArchiveFileInfo> Files { get; private set; }

        public bool ContentChanged => IsChanged();

        public ZarState()
        {
            _zar = new ZAR();
        }

        public async Task Load(IFileSystem fileSystem, UPath filePath, LoadContext loadContext)
        {
            var fileStream = await fileSystem.OpenFileAsync(filePath);
            Files = await Task.Run(() => _zar.Load(fileStream));
        }

        public Task Save(IFileSystem fileSystem, UPath savePath, SaveContext saveContext)
        {
            var output = fileSystem.OpenFile(savePath, FileMode.Create);
            _zar.Save(output, Files);

            return Task.CompletedTask;
        }

        public void ReplaceFile(ArchiveFileInfo afi, Stream fileData)
        {
            afi.SetFileData(fileData);
        }

        private bool IsChanged()
        {
            return Files.Any(x => x.ContentChanged);
        }
    }
}
