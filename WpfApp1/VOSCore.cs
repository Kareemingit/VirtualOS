using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualOS
{
    public abstract class VOSCore
    {
    }

    public class FileManager : VOSCore
    {
        private VirtualDirController virtualDirController = new VirtualDirController();

        public DeskDriver GetRoot()
        {
            return virtualDirController.driverTree;
        }

        public VirtualFolder BackTrack()
        {
            return null;
        }
        public void Load()
        {
            virtualDirController.StartLoader();
        }

        public void OpenNew(VirtualDir vDir)
        {
            if (vDir == null) return;

            if(vDir is VirtualFolder folder)
            {
                virtualDirController.OpenNewFolder(folder);
            }
            if(vDir is VirtualFile)
            {

            }
        }
    }
}
