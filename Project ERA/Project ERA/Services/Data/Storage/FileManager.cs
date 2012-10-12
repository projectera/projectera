using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace ProjectERA.Services.Data.Storage
{
    internal class FileManager : Microsoft.Xna.Framework.GameComponent
    {
        private IsolatedStorageStorageDevice _isolatedDevice;
        private SharedStorageDevice _sharedDevice;
        private PlayerStorageDevice _playerDevice;
        private TitleStorageDevice _titleDevice;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game"></param>
        internal FileManager(Game game)
            :base(game)
        {

            // Add as service
            game.Services.AddService(this.GetType(), this);
        }

        /// <summary>
        /// Initialize the FileManager
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            _sharedDevice = new SharedStorageDevice();
            _playerDevice = new PlayerStorageDevice(PlayerIndex.One);
            _isolatedDevice = new IsolatedStorageStorageDevice();
            _titleDevice = new TitleStorageDevice();

            // hook two event handlers to force the user to choose a new device if they cancel the
			// device selector or if they disconnect the storage device after selecting it
            _sharedDevice.DeviceSelectorCanceled += (s, e) => e.Response = StorageDeviceEventResponse.Force;
			_sharedDevice.DeviceDisconnected += (s, e) => e.Response = StorageDeviceEventResponse.Force;
            _playerDevice.DeviceDisconnected += (s, e) => e.Response = StorageDeviceEventResponse.Force;
            _playerDevice.DeviceSelectorCanceled += (s, e) => e.Response = StorageDeviceEventResponse.Force; 

			// prompt for a device on the first Update we can
			_sharedDevice.PromptForDevice();
            _playerDevice.PromptForDevice();

            // hook an event so we can see that it does fire
			_sharedDevice.SaveCompleted += SharedSaveCompleted;
            _sharedDevice.LoadCompleted += SharedLoadCompleted;
            _sharedDevice.DeleteCompleted += SharedDeleteCompleted;
            _sharedDevice.FileExistsCompleted += SharedFileExistsCompleted;
            _sharedDevice.GetFilesCompleted += SharedGetFilesCompleted;

            _isolatedDevice.SaveCompleted += IsolatedSaveCompleted;
            _isolatedDevice.LoadCompleted += IsolatedLoadCompleted;
            _isolatedDevice.DeleteCompleted += IsolatedDeleteCompleted;
            _isolatedDevice.FileExistsCompleted += IsolatedFileExistsCompleted;
            _isolatedDevice.GetFilesCompleted += IsolatedGetFilesCompleted;

            _playerDevice.SaveCompleted += PlayerSaveCompleted;
            _playerDevice.LoadCompleted += PlayerLoadCompleted;
            _playerDevice.DeleteCompleted += PlayerDeleteCompleted;
            _playerDevice.FileExistsCompleted += PlayerFileExistsCompleted;
            _playerDevice.GetFilesCompleted += PlayerGetFilesCompleted;

            this.Game.Components.Add(_sharedDevice);
            this.Game.Components.Add(_playerDevice);

            
        }

        /// <summary>
        /// Raised when a SaveAsync operation has completed.
        /// </summary>
        public event SaveCompletedEventHandler SharedSaveCompleted, IsolatedSaveCompleted, PlayerSaveCompleted;

        /// <summary>
        /// Raised when a LoadAsync operation has completed.
        /// </summary>
        public event LoadCompletedEventHandler SharedLoadCompleted, IsolatedLoadCompleted, PlayerLoadCompleted;

        /// <summary>
        /// Raised when a DeleteAsync operation has completed.
        /// </summary>
        public event DeleteCompletedEventHandler SharedDeleteCompleted, IsolatedDeleteCompleted, PlayerDeleteCompleted;

        /// <summary>
        /// Raised when a FileExistsAsync operation has completed.
        /// </summary>
        public event FileExistsCompletedEventHandler SharedFileExistsCompleted, IsolatedFileExistsCompleted, PlayerFileExistsCompleted;

        /// <summary>
        /// Raised when a GetFilesAsync operation has completed.
        /// </summary>
        public event GetFilesCompletedEventHandler SharedGetFilesCompleted, IsolatedGetFilesCompleted, PlayerGetFilesCompleted;

        /// <summary>
        /// Gets IStorageDevice
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public IStorageDevice GetStorageDevice(FileLocationContainer container)
        {
            switch (container)
            {
                case FileLocationContainer.IsolatedMachine:
                case FileLocationContainer.IsolatedUser:
                    return _isolatedDevice;
                case FileLocationContainer.Shared:
                    return _sharedDevice;
                case FileLocationContainer.Player:
                    return _playerDevice;
                case FileLocationContainer.Title:
                    return _titleDevice;
            }

            throw new InvalidOperationException("No such container");
        }

        /// <summary>
        /// Gets IAsyncStorageDevice
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public IAsyncStorageDevice GetAsyncStorageDevice(FileLocationContainer container)
        {
            switch (container)
            {
                case FileLocationContainer.IsolatedMachine:
                case FileLocationContainer.IsolatedUser:
                    return _isolatedDevice;
                case FileLocationContainer.Shared:
                    return _sharedDevice;
                case FileLocationContainer.Player:
                    return _playerDevice;
            }

            throw new InvalidOperationException("No such container");
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Remove from components
            this.Game.Components.Remove(_sharedDevice);
            this.Game.Components.Remove(_playerDevice);

            // Remove event hooks
            _sharedDevice.SaveCompleted -= SharedSaveCompleted;
            _sharedDevice.LoadCompleted -= SharedLoadCompleted;
            _sharedDevice.DeleteCompleted -= SharedDeleteCompleted;
            _sharedDevice.FileExistsCompleted -= SharedFileExistsCompleted;
            _sharedDevice.GetFilesCompleted -= SharedGetFilesCompleted;

            _isolatedDevice.SaveCompleted -= IsolatedSaveCompleted;
            _isolatedDevice.LoadCompleted -= IsolatedLoadCompleted;
            _isolatedDevice.DeleteCompleted -= IsolatedDeleteCompleted;
            _isolatedDevice.FileExistsCompleted -= IsolatedFileExistsCompleted;
            _isolatedDevice.GetFilesCompleted -= IsolatedGetFilesCompleted;

            _playerDevice.SaveCompleted -= PlayerSaveCompleted;
            _playerDevice.LoadCompleted -= PlayerLoadCompleted;
            _playerDevice.DeleteCompleted -= PlayerDeleteCompleted;
            _playerDevice.FileExistsCompleted -= PlayerFileExistsCompleted;
            _playerDevice.GetFilesCompleted -= PlayerGetFilesCompleted;
        }
    }

    /// <summary>
    /// A method for loading or saving a file.
    /// </summary>
    /// <param name="stream">A Stream to use for accessing the file data.</param>
    public delegate void FileAction(Stream stream);

    [Flags]
    public enum FileLocationContainer
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// Documents/SavedGames/...
        /// </summary>
        SavedGames = 1,

        /// <summary>
        /// ProgramData/IsolatedStorage/...
        /// </summary>
        Isolated = 2,

        /// <summary>
        /// OnePlayer/OneUser
        /// </summary>
        Private = 4,

        /// <summary>
        /// AllPlayers//AllUsers
        /// </summary>
        Public = 8,

        /// <summary>
        /// GameRoot/[Content]/...
        /// </summary>
        Title = 16,

        /// <summary>
        /// Shared Player Storage
        /// </summary>
        Shared = SavedGames | Public,
        /// <summary>
        /// Player Storage
        /// </summary>
        Player = SavedGames | Private,
        /// <summary>
        /// Machine Isolated Storage
        /// </summary>
        IsolatedMachine = Isolated | Public,
        /// <summary>
        /// User Isolated Storage
        /// </summary>
        IsolatedUser = Isolated | Private,
    }
}
