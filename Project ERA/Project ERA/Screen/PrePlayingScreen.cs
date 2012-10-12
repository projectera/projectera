using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using ProjectERA.Services.Network.Protocols;
using ProjectERA.Services.Data;
using ERAUtils;
using ERAUtils.Logger;
using ERAUtils.Enum;
using Microsoft.Xna.Framework;

namespace ProjectERA.Screen
{
    internal class PrePlayingScreen : ProgressLoadingScreen
    {
        List<Task> _loadingTasks;
        List<VariableLoadingPopup> _popups;
        Object _popupsLock = new Object();
        Boolean _postProcess = false;

        /// <summary>
        /// 
        /// </summary>
        public PrePlayingScreen()
            :base()
        {
            this.IsPopup = false;
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void Initialize()
        {
            _loadingTasks = new List<Task>();
            _popups = new List<VariableLoadingPopup>();

            base.Initialize();
        }
        
        /// <summary>
        /// 
        /// </summary>
        internal override void PostProcessing()
        {
            base.PostProcessing();

            lock (_popupsLock)
            {
                foreach (VariableLoadingPopup popup in _popups)
                    this.ScreenManager.AddScreen(popup);

                _postProcess = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        internal override void LoadContent(ContentManager contentManager)
        {
            // Starts loading tasks
            _loadingTasks.Add(Task.Factory.StartNew(() =>
                {
                    // Received Player Data (since Map.Id is now set). No request
                    // is needed to received this data, as this happens on pickavatar.
                    ProgressBy(1, 5);

                    // Get the mapManager service. We will use this to validate map
                    // and/or tileset data, to receive new map and/ord tileset data 
                    // and graphics, and to receive the interactable data.
                    using (MapManager mapManager = (MapManager)this.Game.Services.GetService(typeof(MapManager)))
                    {
                        // Validates and updates mapData
                        Task validate = ValidateData(mapManager).ContinueWith((mapData) => {
                            // Validates and updates graphics
                            Task innerValidate = ValidateGraphics(mapManager, mapData.Result);
                            Task.WaitAll(innerValidate);
                        });
                        Task.WaitAll(validate);
                    }                    

                }, TaskCreationOptions.LongRunning));

            _loadingTasks.Add(Task.Factory.StartNew(() =>
                {

                    // Get the mapManager service. We will use this to validate map
                    // and/or tileset data, to receive new map and/ord tileset data 
                    // and graphics, and to receive the interactable data.
                    using (MapManager mapManager = (MapManager)this.Game.Services.GetService(typeof(MapManager)))
                    {
                        // Get interactables
                        Task request = mapManager.RequestInteractables(Map.Id, NetworkManager);

                        Task.WaitAll(request);
                    }
                }, TaskCreationOptions.LongRunning));

            // Finish upon all tasks done
            Task.Factory.StartNew(() =>
            {
                Task.WaitAll(_loadingTasks.ToArray());


                Logger.Debug("All Tasks completed");
                FinishProgress();
            }, TaskCreationOptions.PreferFairness);
                
            base.LoadContent(contentManager);
        }

        /// <summary>
        /// Validate Map Graphics
        /// </summary>
        /// <param name="mapManager">MapManager service</param>
        /// <param name="mapData">MapData to obtain graphics from</param>
        private Task ValidateGraphics(MapManager mapManager, MapData mapData)
        {
            // Start validatation
            return mapManager.ValidateGraphics(Map.Id, this.NetworkManager).ContinueWith((validationCode) =>
                {
                    Logger.Debug("Validation graphics code received: " + validationCode.Result);
                    List<Task> innerTasks = new List<Task>();

                    // Validation Reponse is as follows:
                    //
                    // (Int32) ------------------------AAAAAAAT 
                    // whereas T indicates a valid tileset
                    //         A indicates a valid autotile
                    //
                    // If T is not set, a new tilesetgraphic is needed
                    // If A is not set, a new autotilegraphic is needed

                    if ((validationCode.Result & (1 << 0)) != (1 << 0))
                    {
                        // Get tileset graphic
                        Logger.Debug("TilesetGraphic out of date");

                        Protocol protocol;
                        if (this.NetworkManager.TryGetProtocol(typeof(Asset), out protocol))
                        {
                            TaskCompletionSource<Data.Asset> innerAsset = new TaskCompletionSource<Data.Asset>();
                            Asset assetProtocol = ((Asset)protocol);
                            innerTasks.Add(assetProtocol.RequestAsset(AssetType.Tileset, mapData.TilesetData.AssetName, (succeeded, asset) =>
                                {
                                    innerAsset.SetResult(asset);
                                }).
                                ContinueWith((asset) =>
                                    {
                                        Task.WaitAll(innerAsset.Task);

                                        // If asset exists on server, start download request
                                        if (asset != null && asset.Result == AssetOperationResult.Ok)
                                        {
                                            VariableLoadingPopup thisPopup = new VariableLoadingPopup(new Vector2(1280 / 2, 500), true);
                                            thisPopup.DisplayText = "Downloading Tileset";
                                            Double percentDone = 0;
                                            Int32 chunksDone = 0, chunksNum = 0;

                                            lock (_popupsLock)
                                            {

                                                foreach (var popup in _popups)
                                                    popup.Position -= Vector2.UnitX * 150;

                                                _popups.Add(thisPopup);
                                                if (_postProcess == true)
                                                    this.ScreenManager.AddScreen(thisPopup);

                                                thisPopup.Position = (_popups.Count > 0) ? _popups[_popups.Count - 1].Position + Vector2.UnitX * 150 : thisPopup.Position;
                                            }

                                            TaskCompletionSource<Boolean> savingTask = new TaskCompletionSource<Boolean>();
                                            Task innerTask = assetProtocol.DownloadAsset(innerAsset.Task.Result,

                                                // RECEIVED
                                                (suceeded, bytes) =>
                                                {
                                                    Logger.Debug("DownloadAsset Task completed with: " + suceeded + "/" + (bytes ?? new Byte[0]).Length);

                                                    if (suceeded)
                                                    {
                                                        thisPopup.DisplayText = "Saving Tileset";

                                                        mapManager.FileManager.GetStorageDevice(ProjectERA.Services.Data.Storage.FileLocationContainer.IsolatedMachine).
                                                            Save(AssetPath.Get(AssetType.Tileset).Replace('.', '/'), mapData.TilesetData.AssetName.Replace(".png", String.Empty) + ".png", (stream) =>
                                                                {
                                                                    stream.Write(bytes, 0, bytes.Length);
                                                                }
                                                            );
                                                    }
                                                    else
                                                    {
                                                        thisPopup.DisplayText = "Error";
                                                    }

                                                    thisPopup.FinishProgress();
                                                    savingTask.SetResult(suceeded);
                                                },

                                                // PARTIAL CHUNK RECEIVED
                                                (chunkId, chunks, chunkparts, parts) =>
                                                {
                                                    percentDone += 100f/chunks/chunkparts; // 1 chunk part
                                                    chunksNum = chunks;

                                                    thisPopup.DisplayText = String.Format("Tileset {0}%", Math.Round(percentDone));
                                                },

                                                // CHUNK Received
                                                (chunkId, chunks) =>
                                                {
                                                    chunksDone += 1;
                                                    chunksNum = chunks;

                                                    thisPopup.DisplayText = String.Format("Tileset {0}%", Math.Round(percentDone));
                                                }
                                            ).ContinueWith(succeeded => Task.WaitAll(savingTask.Task));
                                            Task.WaitAll(innerTask);

                                            Logger.Debug("DownloadAsset Task completed");
                                        }
                                    }
                                )
                            );
                        }
                        else
                        {
                            throw new InvalidOperationException("Asset protocol not found!");
                        }
                    }

                    for (Int32 i = 0; i < mapData.TilesetData.AutotileAssetNames.Count; i++)
                    {
                        if ((validationCode.Result & (1 << (i + 1))) != (1 << (i + 1)))
                        {
                            // Get autotile graphic
                            Logger.Debug("AutotileGraphic out of date");

                            Protocol protocol;
                            if (this.NetworkManager.TryGetProtocol(typeof(Asset), out protocol))
                            {
                                String fileName =  mapData.TilesetData.AutotileAssetNames[i];
                                TaskCompletionSource<Data.Asset> innerAsset = new TaskCompletionSource<Data.Asset>();
                                Asset assetProtocol = ((Asset)protocol);
                                innerTasks.Add(assetProtocol.RequestAsset(AssetType.Autotile, fileName, (succeeded, asset) =>
                                    {
                                        innerAsset.SetResult(asset);
                                    }).
                                    ContinueWith((asset) =>
                                    {
                                        Task.WaitAll(innerAsset.Task);

                                        if (asset != null && asset.Result == AssetOperationResult.Ok)
                                        {
                                            VariableLoadingPopup thisPopup = new VariableLoadingPopup(new Vector2(1280 / 2, 500), true);
                                            thisPopup.DisplayText = "Downloading Autotile";
                                            Double percentDone = 0;
                                            Int32 chunksDone = 0, chunksNum = 0;
                                            
                                            lock (_popupsLock)
                                            {

                                                foreach (var popup in _popups)
                                                    popup.Position -= Vector2.UnitX * 150;

                                                _popups.Add(thisPopup);
                                                if (_postProcess == true)
                                                    this.ScreenManager.AddScreen(thisPopup);

                                                thisPopup.Position = (_popups.Count > 0) ? _popups[_popups.Count - 1].Position + Vector2.UnitX * 150 : thisPopup.Position;
                                            }

                                            TaskCompletionSource<Boolean> savingTask = new TaskCompletionSource<bool>();
                                            Task innerTask = assetProtocol.DownloadAsset(innerAsset.Task.Result, 
                                                
                                                // RECEIVED
                                                (suceeded, bytes) =>
                                                {
                                                    Logger.Debug("DownloadAsset Task completed with: " + suceeded + "/" + (bytes ?? new Byte[0]).Length);

                                                    if (suceeded)
                                                    {
                                                        thisPopup.DisplayText = "Saving Autotile";

                                                        mapManager.FileManager.GetStorageDevice(ProjectERA.Services.Data.Storage.FileLocationContainer.IsolatedMachine).
                                                            Save(AssetPath.Get(AssetType.Autotile).Replace('.', '/'), fileName.Replace(".png", String.Empty) + ".png", (stream) =>
                                                                {
                                                                    stream.Write(bytes, 0, bytes.Length);
                                                                }
                                                            );
                                                    }
                                                    else
                                                    {
                                                        thisPopup.DisplayText = "Error";
                                                    }

                                                    thisPopup.FinishProgress();
                                                    savingTask.SetResult(suceeded);
                                                },

                                                // PARTIAL CHUNK RECEIVED
                                                (chunkId, chunks, partialId, parts) =>
                                                {
                                                    percentDone += 100f / chunks / parts;
                                                    chunksNum = chunks;

                                                    thisPopup.DisplayText = String.Format("Autotile {0}%", Math.Round(percentDone));
                                                },

                                                // CHUNK Received
                                                (chunkId, chunks) =>
                                                {
                                                    chunksDone += 1;
                                                    chunksNum = chunks;

                                                    thisPopup.DisplayText = String.Format("Autotile {0}%", Math.Round(percentDone));
                                                }
                                            ).ContinueWith(succeeded => Task.WaitAll(savingTask.Task));
                                            Task.WaitAll(innerTask);                                 
                                        }
                                    }
                                    )
                                );
                            }
                            else
                            {
                                throw new InvalidOperationException("Asset protocol not found!");
                            }
                        }
                    }

                    Task.WaitAll(innerTasks.ToArray());

                    Logger.Debug("Validate Graphics Task completed");
                }
            );
        }

        /// <summary>
        /// Validate map and tilesetData
        /// </summary>
        /// <param name="mapManager">MapManager service</param>
        private Task<MapData> ValidateData(MapManager mapManager)
        {
            Boolean tilesetChanged = false;
            Int32 actualValidationCode = 0;
            MongoObjectId tilesetId;

            // Start validatation
            return mapManager.ValidateData(Map.Id, this.NetworkManager, out tilesetId).ContinueWith<MapData>(validationCode =>
                {
                    Logger.Debug("Validation code received: " + validationCode.Result);
                    actualValidationCode = validationCode.Result;

                    // Validation Reponse is as follows:
                    //
                    // (Int32) F-----------------------------TM 
                    // whereas F indicates if the request failed
                    //         M indicates a valid map
                    //         T indicates a valid tileset
                    //
                    // If either F is set or M is not set, the map is not valid 
                    // and needs updating. Please note that we do need to 
                    // revalidate the tileset after updating the map, since the
                    // tilesetId might have been changed during the update.
                    if (validationCode.Result == -2 || (validationCode.Result & (1 << 0)) != (1 << 0))
                    {
                        return mapManager.UpdateMap(Map.Id, this.NetworkManager).ContinueWith<MapData>(mapData =>
                            {
                                // If tilesetId changes
                                if (mapData.Result.TilesetId != tilesetId)
                                {
                                    // Need new validation code, since map is retrieved with different tileset
                                    Task innerValidation = mapManager.ValidateData(Map.Id, this.NetworkManager, out tilesetId).
                                        ContinueWith(innerValidationCode =>
                                        {
                                            Logger.Debug("Validation code received (2nd): " + innerValidationCode.Result);
                                            actualValidationCode = innerValidationCode.Result;

                                            // Mark it as new tileset
                                            tilesetChanged = true;
                                        });

                                    // Wait for task to finish
                                    Task.WaitAll(innerValidation);
                                }

                                return mapData.Result;
                            }
                        ).Result;
                    }

                    return null;
                }).ContinueWith<MapData>(mapData =>
                    {
                        // If the loaded map contains interactables, clear them
                        if (mapData.Result != null && mapData.Result.Interactables != null)
                            mapData.Result.Interactables.Clear();

                        // Received Map Data. The MapRequest is also no longer active and
                        // we can be certain we have an updated and valid mapdata
                        ProgressBy(1, 5);

                        // If the T flag is not set, the tileset was not valid. This means
                        // we need to obtain anew. Also, tilesetChanged is set to true, 
                        // because we might obtain a new tileset graphic name / autotiles
                        if (actualValidationCode == -2 || (actualValidationCode & (1 << 1)) != (1 << 1))
                        {
                            Task tilesetTask = mapManager.UpdateTileset(tilesetId, this.NetworkManager);
                            Task.WaitAll(tilesetTask);

                            tilesetChanged = true;
                        }

                        MapData actualMapData = mapData.Result;

                        // If mapdata is still null or the tileset was changed, we better reload our 
                        // mapdata (and graphics). 
                        if (actualMapData == null || tilesetChanged)
                        {
                            // Also need to refresh the cached mapgraphics if ts changed
                            if (tilesetChanged)
                                ((Services.Data.MapManager)NetworkManager.Game.Services.GetService(typeof(Services.Data.MapManager))).ReloadMapData(Map.Id);

                            Graphics.Sprite.TileMap mapGraphics;
                            mapManager.FillMapObjects(Map.Id, out actualMapData, out mapGraphics);
                        }

                        // Received Tileset Data. The TilesetRequest is also no longer active and
                        // we can be certain we have an updated and valid mapdata
                        ProgressBy(1, 5);

                        // Return valid, updated mapData
                        return actualMapData;
                }
            );
        }
    }
}
