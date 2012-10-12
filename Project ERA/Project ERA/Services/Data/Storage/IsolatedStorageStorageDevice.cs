using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;

namespace ProjectERA.Services.Data.Storage
{
	/// <summary>
	/// An implementation of ISaveDevice utilizing IsolatedStorage for Windows Phone.
	/// Given that this is an ISaveDevice, all of our methods still have a container
	/// name for compatibility, but that parameter is simply ignored by the
	/// implementation since there is no concept of a container within IsolatedStorage.
	/// </summary>
	public class IsolatedStorageStorageDevice : IAsyncStorageDevice
	{
		// a queue used for recycling our state Objects to avoid garbage or boxing when using ThreadPool
		private Queue<FileOperationState> pendingStates = new Queue<FileOperationState>(100);

		private Int32 pendingOperations;

		/// <summary>
		/// Gets whether or not the device is in a state to receive file operation method calls.
		/// </summary>
		public bool IsReady { get { return true; } }

		/// <summary>
		/// Gets whether or not the device is busy performing a file operation.
		/// </summary>
		/// <remarks>
		/// Games can query this property to determine when to show an indication that game is saving
		/// such as a spinner or other icon.
		/// </remarks>
		public bool IsBusy
		{
			get
			{
				return pendingOperations > 0;
			}
		}

		/// <summary>
		/// Raised when a SaveAsync operation has completed.
		/// </summary>
		public event SaveCompletedEventHandler SaveCompleted;

		/// <summary>
		/// Raised when a LoadAsync operation has completed.
		/// </summary>
		public event LoadCompletedEventHandler LoadCompleted;

		/// <summary>
		/// Raised when a DeleteAsync operation has completed.
		/// </summary>
		public event DeleteCompletedEventHandler DeleteCompleted;

		/// <summary>
		/// Raised when a FileExistsAsync operation has completed.
		/// </summary>
		public event FileExistsCompletedEventHandler FileExistsCompleted;

		/// <summary>
		/// Raised when a GetFilesAsync operation has completed.
		/// </summary>
		public event GetFilesCompletedEventHandler GetFilesCompleted;

		/// <summary>
		/// Helper method that gets isolated storage. On Windows we use GetUserStoreForDomain, but on the other
		/// platforms we use GetUserStoreForApplication.
		/// </summary>
		/// <returns>The opened IsolatedStorageFile.</returns>
		private IsolatedStorageFile GetIsolatedStorage(FileLocationContainer container)
		{
#if WINDOWS
			switch (container)
			{
				case FileLocationContainer.Public:
				case FileLocationContainer.IsolatedMachine:
					return IsolatedStorageFile.GetMachineStoreForDomain();

				case FileLocationContainer.Private:
				case FileLocationContainer.IsolatedUser:
					return IsolatedStorageFile.GetUserStoreForDomain();

				default :
					return GetIsolatedStorage(FileLocationContainer.IsolatedMachine);
			}
#else
			return IsolatedStorageFile.GetUserStoreForApplication();
#endif
		}

		/// <summary>
		/// Saves a file.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <param name="fileName">The file to save.</param>
		/// <param name="saveAction">The save action to perform.</param>
		public void Save(String containerName, String fileName, FileAction saveAction)
		{
			Save(containerName, fileName, saveAction, FileLocationContainer.IsolatedMachine);
		}

			 
		/// <summary>
		/// Saves a file.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <param name="fileName">The file to save.</param>
		/// <param name="saveAction">The save action to perform.</param>
		public void Save(String containerName, String fileName, FileAction saveAction, FileLocationContainer container)
		{
			if (container.HasFlag(FileLocationContainer.SavedGames) || container.HasFlag(FileLocationContainer.Title))
				throw new InvalidOperationException("Can not search in other containers than IsolatedStorage");

			using (IsolatedStorageFile storage = GetIsolatedStorage(container))
			{
				SpinWait.SpinUntil(() => { System.Threading.Thread.MemoryBarrier(); return this.IsReady && !this.IsBusy; });

                try
                {
                    if (containerName != ".")
                        storage.CreateDirectory(containerName);

                    using (var stream = storage.CreateFile(String.Join("/", containerName, fileName)))
                    {
                        saveAction(stream);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    throw;
                }
                catch (FileNotFoundException)
                {
                    throw;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(100);
                    Save(containerName, fileName, saveAction, container);
                }
			}
		}

		/// <summary>
		/// Loads a file.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <param name="fileName">The file to load.</param>
		/// <param name="loadAction">The load action to perform.</param>
		public void Load(String containerName, String fileName, FileAction loadAction)
		{
			Load(containerName, fileName, loadAction, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Loads a file.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <param name="fileName">The file to load.</param>
		/// <param name="loadAction">The load action to perform.</param>
		public void Load(String containerName, String fileName, FileAction loadAction, FileLocationContainer container)
		{
			if (container.HasFlag(FileLocationContainer.SavedGames) || container.HasFlag(FileLocationContainer.Title))
				throw new InvalidOperationException("Can not search in other containers than IsolatedStorage");

			using (IsolatedStorageFile storage = GetIsolatedStorage(container))
			{

				SpinWait.SpinUntil(() => { System.Threading.Thread.MemoryBarrier(); return this.IsReady && !this.IsBusy; });

                try
                {
                    using (var stream = storage.OpenFile(String.Join("/", containerName, fileName), FileMode.Open))
                    {
                        loadAction(stream);
                    }
                }
                catch (DirectoryNotFoundException e)
                {
                    throw e;
                }
                catch (FileNotFoundException e)
                {
                    throw e;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(100);
                    Load(containerName, fileName, loadAction, container);
                }
			}
		}

		/// <summary>
		/// Deletes a file.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <param name="fileName">The file to delete.</param>
		public void Delete(String containerName, String fileName)
		{
			Delete(containerName, fileName, FileLocationContainer.IsolatedMachine);

		}

		/// <summary>
		/// Deletes a file.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <param name="fileName">The file to delete.</param>
		public void Delete(String containerName, String fileName, FileLocationContainer container)
		{
			if (container.HasFlag(FileLocationContainer.SavedGames) || container.HasFlag(FileLocationContainer.Title))
				throw new InvalidOperationException("Can not search in other containers than IsolatedStorage");

			using (IsolatedStorageFile storage = GetIsolatedStorage(container))
			{
				SpinWait.SpinUntil(() => { System.Threading.Thread.MemoryBarrier(); return this.IsReady && !this.IsBusy; });   

				if (FileExists(containerName, fileName, container))
				{
					storage.DeleteFile(String.Join("/", containerName, fileName));
				}
			}
		}

		/// <summary>
		/// Determines if a given file exists.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <param name="fileName">The name of the file.</param>
		/// <returns>True if the file exists, false otherwise.</returns>
		public bool FileExists(String containerName, String fileName)
		{
			return FileExists(containerName, fileName, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Determines if a given file exists.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <param name="fileName">The name of the file.</param>
		/// <returns>True if the file exists, false otherwise.</returns>
		public bool FileExists(String containerName, String fileName, FileLocationContainer container)
		{
			if (container.HasFlag(FileLocationContainer.SavedGames) || container.HasFlag(FileLocationContainer.Title))
				throw new InvalidOperationException("Can not search in other containers than IsolatedStorage");
		
			using (IsolatedStorageFile storage = GetIsolatedStorage(container))
			{
				return storage.FileExists(String.Join("/", containerName, fileName));
			}
		}

		/// <summary>
		/// Gets an array of all files available in a container.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <returns>An array of file names of the files in the container.</returns>
		public String[] GetFiles(String containerName)
		{
			return GetFiles(containerName, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Gets an array of all files available in a container.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <returns>An array of file names of the files in the container.</returns>
		public String[] GetFiles(String containerName, FileLocationContainer container)
		{
			if (container.HasFlag(FileLocationContainer.SavedGames) || container.HasFlag(FileLocationContainer.Title))
				throw new InvalidOperationException("Can not search in other containers than IsolatedStorage");
		
			using (IsolatedStorageFile storage = GetIsolatedStorage(container))
			{
				return storage.GetFileNames();
			}
		}

		/// <summary>
		/// Gets an array of all files available in a container.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <param name="pattern">A search pattern to use to find files.</param>
		/// <returns>An array of file names of the files in the container.</returns>
		public String[] GetFiles(String containerName, String pattern)
		{
			return GetFiles(containerName, pattern, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Gets an array of all files available in a container.
		/// </summary>
		/// <param name="containerName">Used to match the ISaveDevice interface; ignored by the implementation.</param>
		/// <param name="pattern">A search pattern to use to find files.</param>
		/// <returns>An array of file names of the files in the container.</returns>
		public String[] GetFiles(String containerName, String pattern, FileLocationContainer container)
		{
			if (container.HasFlag(FileLocationContainer.SavedGames) || container.HasFlag(FileLocationContainer.Title))
				throw new InvalidOperationException("Can not search in other containers than IsolatedStorage");
		
			using (IsolatedStorageFile storage = GetIsolatedStorage(container))
			{
				return storage.GetFileNames(pattern);
			}
		}

		/// <summary>
		/// Saves a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to save the file.</param>
		/// <param name="fileName">The file to save.</param>
		/// <param name="saveAction">The save action to perform.</param>
		public void SaveAsync(String containerName, String fileName, FileAction saveAction)
		{
			SaveAsync(containerName, fileName, saveAction, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Saves a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to save the file.</param>
		/// <param name="fileName">The file to save.</param>
		/// <param name="saveAction">The save action to perform.</param>
		public void SaveAsync(String containerName, String fileName, FileAction saveAction, FileLocationContainer container)
		{
			SaveAsync(containerName, fileName, saveAction, null, container);
		}

		/// <summary>
		/// Saves a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to save the file.</param>
		/// <param name="fileName">The file to save.</param>
		/// <param name="saveAction">The save action to perform.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void SaveAsync(String containerName, String fileName, FileAction saveAction, Object userState)
		{
			SaveAsync(containerName, fileName, saveAction, userState, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Saves a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to save the file.</param>
		/// <param name="fileName">The file to save.</param>
		/// <param name="saveAction">The save action to perform.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void SaveAsync(String containerName, String fileName, FileAction saveAction, Object userState, FileLocationContainer container)
		{
			// increment our pending operations count
			PendingOperationsIncrement();

			// get a FileOperationState and fill it in
			FileOperationState state = GetFileOperationState();
			state.Container = containerName;
			state.File = String.Join("/", containerName, fileName);
			state.Action = saveAction;
			state.UserState = userState;
			state.Location = container;

			// queue up the work item
			ThreadPool.QueueUserWorkItem(DoSaveAsync, state);
		}

		/// <summary>
		/// Loads a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container from which to load the file.</param>
		/// <param name="fileName">The file to load.</param>
		/// <param name="loadAction">The load action to perform.</param>
		public void LoadAsync(String containerName, String fileName, FileAction loadAction)
		{
			LoadAsync(containerName, fileName, loadAction, null, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Loads a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container from which to load the file.</param>
		/// <param name="fileName">The file to load.</param>
		/// <param name="loadAction">The load action to perform.</param>
		public void LoadAsync(String containerName, String fileName, FileAction loadAction, FileLocationContainer container)
		{
			LoadAsync(containerName, fileName, loadAction, null, container);
		}

		/// <summary>
		/// Loads a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container from which to load the file.</param>
		/// <param name="fileName">The file to load.</param>
		/// <param name="loadAction">The load action to perform.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void LoadAsync(String containerName, String fileName, FileAction loadAction, Object userState)
		{
			LoadAsync(containerName, fileName, loadAction, userState, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Loads a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container from which to load the file.</param>
		/// <param name="fileName">The file to load.</param>
		/// <param name="loadAction">The load action to perform.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void LoadAsync(String containerName, String fileName, FileAction loadAction, Object userState, FileLocationContainer container)
		{
			// increment our pending operations count
			PendingOperationsIncrement();

			// get a FileOperationState and fill it in
			FileOperationState state = GetFileOperationState();
			state.Container = containerName;
			state.File = String.Join("/", containerName, fileName);
			state.Action = loadAction;
			state.UserState = userState;
			state.Location = container;

			// queue up the work item
			ThreadPool.QueueUserWorkItem(DoLoadAsync, state);
		}

		/// <summary>
		/// Deletes a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container from which to delete the file.</param>
		/// <param name="fileName">The file to delete.</param>
		public void DeleteAsync(String containerName, String fileName)
		{
			DeleteAsync(containerName, fileName, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Deletes a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container from which to delete the file.</param>
		/// <param name="fileName">The file to delete.</param>
		public void DeleteAsync(String containerName, String fileName, FileLocationContainer container)
		{
			DeleteAsync(containerName, fileName, null, container);
		}

		/// <summary>
		/// Deletes a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container from which to delete the file.</param>
		/// <param name="fileName">The file to delete.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void DeleteAsync(String containerName, String fileName, Object userState)
		{
			DeleteAsync(containerName, fileName, userState, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Deletes a file asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container from which to delete the file.</param>
		/// <param name="fileName">The file to delete.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void DeleteAsync(String containerName, String fileName, Object userState, FileLocationContainer container)
		{
			// increment our pending operations count
			PendingOperationsIncrement();

			// get a FileOperationState and fill it in
			FileOperationState state = GetFileOperationState();
			state.Container = containerName;
			state.File = String.Join("/", containerName, fileName);
			state.UserState = userState;
			state.Location = container;

			// queue up the work item
			ThreadPool.QueueUserWorkItem(DoDeleteAsync, state);
		}

		/// <summary>
		/// Determines if a given file exists asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to check for the file.</param>
		/// <param name="fileName">The name of the file.</param>
		public void FileExistsAsync(String containerName, String fileName)
		{
			FileExistsAsync(containerName, fileName, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Determines if a given file exists asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to check for the file.</param>
		/// <param name="fileName">The name of the file.</param>
		public void FileExistsAsync(String containerName, String fileName, FileLocationContainer container)
		{
			FileExistsAsync(containerName, fileName, null, container);
		}

		/// <summary>
		/// Determines if a given file exists asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to check for the file.</param>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void FileExistsAsync(String containerName, String fileName, Object userState)
		{
			FileExistsAsync(containerName, fileName, userState, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Determines if a given file exists asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to check for the file.</param>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void FileExistsAsync(String containerName, String fileName, Object userState, FileLocationContainer container)
		{
			// increment our pending operations count
			PendingOperationsIncrement();

			// get a FileOperationState and fill it in
			FileOperationState state = GetFileOperationState();
			state.Container = containerName;
			state.File = String.Join("/", containerName, fileName);
			state.UserState = userState;
			state.Location = container;

			// queue up the work item
			ThreadPool.QueueUserWorkItem(DoFileExistsAsync, state);
		}

		/// <summary>
		/// Gets an array of all files available in a container asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to search for files.</param>
		public void GetFilesAsync(String containerName, FileLocationContainer container)
		{
			GetFilesAsync(containerName, null, container);
		}

		/// <summary>
		/// Gets an array of all files available in a container asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to search for files.</param>
		public void GetFilesAsync(String containerName)
		{
			GetFilesAsync(containerName, null, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Gets an array of all files available in a container asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to search for files.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void GetFilesAsync(String containerName, Object userState)
		{
			GetFilesAsync(containerName, "*", userState, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Gets an array of all files available in a container asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to search for files.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void GetFilesAsync(String containerName, Object userState, FileLocationContainer container)
		{
			GetFilesAsync(containerName, "*", userState, container);
		}

		/// <summary>
		/// Gets an array of all files available in a container that match the given pattern asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to search for files.</param>
		/// <param name="pattern">A search pattern to use to find files.</param>
		public void GetFilesAsync(String containerName, String pattern)
		{
			GetFilesAsync(containerName, pattern, null, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Gets an array of all files available in a container that match the given pattern asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to search for files.</param>
		/// <param name="pattern">A search pattern to use to find files.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void GetFilesAsync(String containerName, String pattern, Object userState)
		{
			GetFilesAsync(containerName, pattern, userState, FileLocationContainer.IsolatedMachine);
		}

		/// <summary>
		/// Gets an array of all files available in a container that match the given pattern asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to search for files.</param>
		/// <param name="pattern">A search pattern to use to find files.</param>
		public void GetFilesAsync(String containerName, String pattern, FileLocationContainer container)
		{
			GetFilesAsync(containerName, pattern, null, container);
		}

		/// <summary>
		/// Gets an array of all files available in a container that match the given pattern asynchronously.
		/// </summary>
		/// <param name="containerName">The name of the container in which to search for files.</param>
		/// <param name="pattern">A search pattern to use to find files.</param>
		/// <param name="userState">A state Object used to identify the async operation.</param>
		public void GetFilesAsync(String containerName, String pattern, Object userState, FileLocationContainer container)
		{
			// increment our pending operations count
			PendingOperationsIncrement();

			// get a FileOperationState and fill it in
			FileOperationState state = GetFileOperationState();
			state.Container = containerName;
			state.Pattern = pattern;
			state.UserState = userState;
			state.Location = container;

			// queue up the work item
			ThreadPool.QueueUserWorkItem(DoGetFilesAsync, state);
		}

		/// <summary>
		/// Helper that performs our asynchronous saving.
		/// </summary>
		private void DoSaveAsync(Object asyncState)
		{
			FileOperationState state = asyncState as FileOperationState;
			Exception error = null;

			// perform the save operation
			try
			{
				Save(state.Container, state.File, state.Action, state.Location);
			}
			catch (Exception e)
			{
				error = e;
			}

			// construct our event arguments
			FileActionCompletedEventArgs args = new FileActionCompletedEventArgs(error, state.UserState);

			// fire our completion event
			SaveCompleted(this, args);

			// recycle our state Object
			ReturnFileOperationState(state);

			// decrement our pending operation count
			PendingOperationsDecrement();
		}

		/// <summary>
		/// Helper that performs our asynchronous loading.
		/// </summary>
		private void DoLoadAsync(Object asyncState)
		{
			FileOperationState state = asyncState as FileOperationState;
			Exception error = null;

			// perform the load operation
			try
			{
				Load(state.Container, state.File, state.Action, state.Location);
			}
			catch (Exception e)
			{
				error = e;
			}

			// construct our event arguments
			FileActionCompletedEventArgs args = new FileActionCompletedEventArgs(error, state.UserState);

			// fire our completion event
			LoadCompleted(this, args);

			// recycle our state Object
			ReturnFileOperationState(state);

			// decrement our pending operation count
			PendingOperationsDecrement();
		}

		/// <summary>
		/// Helper that performs our asynchronous deleting.
		/// </summary>
		private void DoDeleteAsync(Object asyncState)
		{
			FileOperationState state = asyncState as FileOperationState;
			Exception error = null;

			// perform the delete operation
			try
			{
				Delete(state.Container, state.File, state.Location);
			}
			catch (Exception e)
			{
				error = e;
			}

			// construct our event arguments
			FileActionCompletedEventArgs args = new FileActionCompletedEventArgs(error, state.UserState);

			// fire our completion event
			DeleteCompleted(this, args);

			// recycle our state Object
			ReturnFileOperationState(state);

			// decrement our pending operation count
			PendingOperationsDecrement();
		}

		/// <summary>
		/// Helper that performs our asynchronous FileExists.
		/// </summary>
		private void DoFileExistsAsync(Object asyncState)
		{
			FileOperationState state = asyncState as FileOperationState;
			Exception error = null;
			bool result = false;

			// perform the FileExists operation
			try
			{
				result = FileExists(state.Container, state.File, state.Location);
			}
			catch (Exception e)
			{
				error = e;
			}

			// construct our event arguments
			FileExistsCompletedEventArgs args = new FileExistsCompletedEventArgs(error, result, state.UserState);

			// fire our completion event
			FileExistsCompleted(this, args);

			// recycle our state Object
			ReturnFileOperationState(state);

			// decrement our pending operation count
			PendingOperationsDecrement();
		}

		/// <summary>
		/// Helper that performs our asynchronous GetFiles.
		/// </summary>
		private void DoGetFilesAsync(Object asyncState)
		{
			FileOperationState state = asyncState as FileOperationState;
			Exception error = null;
			String[] result = null;

			// perform the GetFiles operation
			try
			{
				result = GetFiles(state.Container, state.Pattern, state.Location);
			}
			catch (Exception e)
			{
				error = e;
			}

			// construct our event arguments
			GetFilesCompletedEventArgs args = new GetFilesCompletedEventArgs(error, result, state.UserState);

			// fire our completion event
			GetFilesCompleted(this, args);

			// recycle our state Object
			ReturnFileOperationState(state);

			// decrement our pending operation count
			PendingOperationsDecrement();
		}

		/// <summary>
		/// Helper to increment the pending operation count.
		/// </summary>
		private void PendingOperationsIncrement()
		{
			Interlocked.Increment(ref pendingOperations);
		}

		/// <summary>
		/// Helper to decrement the pending operation count.
		/// </summary>
		private void PendingOperationsDecrement()
		{
			Interlocked.Decrement(ref pendingOperations);
		}

		/// <summary>
		/// Helper for getting a FileOperationState Object.
		/// </summary>
		private FileOperationState GetFileOperationState()
		{
			lock (pendingStates)
			{
				// recycle any states if we have some available
				if (pendingStates.Count > 0)
				{
					FileOperationState state = pendingStates.Dequeue();
					state.Reset();
					return state;
				}

				return new FileOperationState();
			}
		}

		/// <summary>
		/// Helper for returning a FileOperationState to be recycled.
		/// </summary>
		private void ReturnFileOperationState(FileOperationState state)
		{
			lock (pendingStates)
			{
				pendingStates.Enqueue(state);
			}
		}

		/// <summary>
		/// State Object used for our operations.
		/// </summary>
		class FileOperationState
		{
			public String Container;
			public String File;
			public String Pattern;
			public FileAction Action;
			public Object UserState;
			public FileLocationContainer Location;

			public void Reset()
			{
				Container = null;
				File = null;
				Pattern = null;
				Action = null;
				UserState = null;
				Location = 0;
			}
		}
	}
}
