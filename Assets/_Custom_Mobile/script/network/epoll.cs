using System;
using System.Runtime.InteropServices;
using System.Net.Sockets;
namespace wazServer
{
	public class epoll
	{
		
		public enum EVENTS
		{
			IN = 0x001,
			PRI = 0x002,
			OUT = 0x004,
			RDNORM = 0x040,
			RDBAND = 0x080,
			WRNORM = 0x100,
			WRBAND = 0x200,
			MSG = 0x400,
			ERR = 0x008,
			HUP = 0x010,
			RDHUP = 0x2000,
			ONESHOT = (1 << 30),
			ET = (1 << 31)

		};
        public enum CTL
        {
            ADD = 1,
            DEL = 2,
            MOD = 3

        }
		[StructLayout(LayoutKind.Sequential,Pack = 1)]
		public struct _event 
		{
            [MarshalAs(UnmanagedType.U4)]  
			public UInt32 events;
            [MarshalAs(UnmanagedType.I8)]  
			public Int64 userData;
		 
		}


		[DllImport("libglib-2.0.so.0",EntryPoint =  "epoll_create")]
		public static extern int create (int _size);

		[DllImport("libglib-2.0.so.0",EntryPoint = "epoll_ctl")]
		public static extern int ctl (int __epfd, int __op, int __fd,IntPtr __event);

        [DllImport("libglib-2.0.so.0",EntryPoint = "epoll_wait")]
        public static extern int wait(int __epfd, IntPtr __events, int __maxevents, int __timeout);

        [DllImport("libglib-2.0.so.0",EntryPoint = "close")]
        public static extern int close(int __ep);
	}
}

