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
        public void Load()
        {
            virtualDirController.StartLoader();
        }
        public void Open(VirtualDir vDir)
        {
            if (vDir == null) throw new ArgumentNullException("virtual directory is not found");

            if(vDir is VirtualFolder folder)
            {
                virtualDirController.OpenFolder(folder);
            }
            if(vDir is VirtualFile)
            {

            }
        }
    }
}
