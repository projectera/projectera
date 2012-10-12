using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Storage;
using System.IO;

namespace ProjectERA.Services.Data.Storage
{
	enum SaveOperation
	{
		Save,

	}

	class StorageOperationAsyncResult : IAsyncResult
	{
		private readonly object accessLock = new object();

		private bool isCompleted = false;

		private readonly StorageDevice storageDevice;
		private readonly String containerName;
		private readonly String fileName;
		private readonly FileAction fileAction;
		private readonly FileMode fileMode;

		public object AsyncState { get; set; }
		public WaitHandle AsyncWaitHandle { get; private set; }
		public bool CompletedSynchronously { get { return false; } }
		
		public bool IsCompleted
		{
			get
			{
				lock (accessLock)
					return isCompleted;
			}
		}

		internal StorageOperationAsyncResult(StorageDevice device, String container, String file, FileAction action, FileMode mode)
		{
			this.storageDevice = device;
			this.containerName = container;
			this.fileName = file;
			this.fileAction = action;
			this.fileMode = mode;
		}

		private void EndOpenContainer(IAsyncResult result)
		{
			using (var container = storageDevice.EndOpenContainer(result))
			{
				if (fileMode == FileMode.Create)
				{
				}
				else if (fileMode == FileMode.Open)
				{
				}
			}

			lock (accessLock)
				isCompleted = true;
		}
	}
}
