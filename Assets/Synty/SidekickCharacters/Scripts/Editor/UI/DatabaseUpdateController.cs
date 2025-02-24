// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using Synty.SidekickCharacters.Database;
using Synty.SidekickCharacters.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
//using Unity.EditorCoroutines.Editor;
//using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace Synty.SidekickCharacters.UI
{
    /// <summary>
    ///     Controls UI interactions for downloads and updates to the Sidekick database
    /// </summary>
    [InitializeOnLoad]
    public class DatabaseUpdateController
    {
        private const string _API_FAILED_MESSAGE = "Could not connect to the API, please check your internet connection and try again.";
        private const string _BAD_DB_MESSAGE = "Unable to start tool as data is unreadable.";
        private const string _DB_KEY = "";
        private const string _NO_DB_MESSAGE = "Unable to start tool as data is missing.";
        private const string _PROCESSING_UPDATE_MESSAGE = "Processing Update.";
        private const string _UPDATE_DONE_MESSAGE = "Update Complete. Restarting the tool...";
        private const string _UPDATE_FAILED_MESSAGE = "Update failed to download, please check your internet connection and try again.";
        private const string _UPDATE_FOUND_MESSAGE = "New update found ({0} => {1}). Downloading now.";
        private const string _UPDATE_PROGRESS_MESSAGE = "Update downloading... {0:P2} complete.";

        // TODO : currently Proto_Side_Kick_Data, should be something like SidekickData
        private static readonly string _DATABASE_PATH = "Assets/Synty/SidekickCharacters/Database/Proto_Side_Kick_Data"; //Path.Combine(Application.persistentDataPath, "Proto_Side_Kick_Data");

        private static Label _statusText;
        private static UnityWebRequest _downloadWebRequest;
        private static UnityWebRequest _updateCheckWebRequest;

        // TODO : make a UI element for this instead
        // private string _dbPreviewKey = "";

        public DatabaseManager DatabaseManager { get; }
        public bool IsUpdating { get; private set; }

        // static DatabaseUpdateController()
        // {
        //     EditorApplication.update += Update;
        // }

        public DatabaseUpdateController() : this(null)
        {
        }

        public DatabaseUpdateController(Label statusLabel)
        {
            _statusText = statusLabel;

            DatabaseManager = new DatabaseManager();
            // simple DB validation check
            StartDBConnection(true);

            //EditorCoroutineUtility.StartCoroutine(CheckForUpdates(), this);
        }

        // TODO: Determine if any of the following code is still required, and if not remove
        /// <summary>
        ///     Update function hooked into by static constructor, paired with the <c>[InitializeOnLoad]</c> class attribute.
        /// </summary>>
        // private static void Update()
        // {
        //     if (_downloadWebRequest != null && !_downloadWebRequest.isDone)
        //     {
        //         SetStatusText(String.Format(_UPDATE_PROGRESS_MESSAGE, _downloadWebRequest.downloadProgress));
        //     }
        // }

        /// <summary>
        ///     Queries the latest DB version from the API, and if newer than the local version starts the DB update.
        /// </summary>
        // private IEnumerator CheckForUpdates()
        // {
        //     // if the web request is already running, we don't need another one!
        //     if (_updateCheckWebRequest != null)
        //     {
        //         yield break;
        //     }
        //
        //     List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //     if (_dbPreviewKey != "")
        //     {
        //         formData.Add(new MultipartFormDataSection("preview_key", _dbPreviewKey));
        //     }
        //
        //     _updateCheckWebRequest = UrlManager.CreatePostRequest(UrlManager.GetDbUpdateUrl(), formData);
        //     yield return _updateCheckWebRequest.SendWebRequest();
        //
        //     if (_updateCheckWebRequest.result == UnityWebRequest.Result.Success)
        //     {
        //         Version currentVersion = File.Exists(_DATABASE_PATH) ? DatabaseManager.GetDatabaseVersion() : new Version("0.0.0");
        //         JObject dbData = JObject.Parse(_updateCheckWebRequest.downloadHandler.text);
        //         Version newVersion = new Version(dbData["semantic_version"]!.ToString());
        //         if (currentVersion.CompareTo(newVersion) < 0)
        //         {
        //             IsUpdating = true;
        //             SetStatusText(String.Format(_UPDATE_FOUND_MESSAGE, currentVersion, newVersion));
        //             EditorCoroutineUtility.StartCoroutine(UpdateDatabase(dbData["url"]!.ToString()), this);
        //         }
        //         else
        //         {
        //             StartDBConnection();
        //         }
        //     }
        //     else
        //     {
        //         SetStatusText(_API_FAILED_MESSAGE);
        //         StartDBConnection();
        //     }
        //
        //     if (!IsUpdating)
        //     {
        //         _statusText.parent.Remove(_statusText);
        //     }
        //
        //     _updateCheckWebRequest.Dispose();
        //     _updateCheckWebRequest = null;
        // }

        /// <summary>
        ///     Updates the Database with the latest version from the server.
        /// </summary>
        /// <param name="dbUrl">The URL to download the DB from.</param>
        // private IEnumerator UpdateDatabase(string dbUrl)
        // {
        //     // if the web request is already running, we don't need another one!
        //     if (_downloadWebRequest != null)
        //     {
        //         yield break;
        //     }
        //
        //     _downloadWebRequest = UrlManager.CreateGetRequest(dbUrl);
        //     // TODO ideas: use Application.temporaryCachePath instead? dispose of the update download after extraction?
        //     string downloadPath = Path.Combine(Application.persistentDataPath, "temp", "updateDB.zip");
        //     _downloadWebRequest.downloadHandler = new DownloadHandlerFile(downloadPath);
        //     yield return _downloadWebRequest.SendWebRequest();
        //
        //     if (_downloadWebRequest.result == UnityWebRequest.Result.Success)
        //     {
        //         SetStatusText(_PROCESSING_UPDATE_MESSAGE);
        //         DatabaseManager.CloseConnection();
        //         ZipArchive dbZip = ZipFile.Open(downloadPath, ZipArchiveMode.Read);
        //         foreach (ZipArchiveEntry entry in dbZip.Entries)
        //         {
        //             if (entry.Name.Contains(".db"))
        //             {
        //                 entry.ExtractToFile(_DATABASE_PATH, true);
        //             }
        //         }
        //
        //         StartDBConnection(true);
        //         SetStatusText(_UPDATE_DONE_MESSAGE);
        //         EditorCoroutineUtility.StartCoroutineOwnerless(MenuBootstrapController.RestartSidekickWindow());
        //     }
        //     else
        //     {
        //         SetStatusText(_UPDATE_FAILED_MESSAGE);
        //     }
        //
        //     _downloadWebRequest.Dispose();
        //     _downloadWebRequest = null;
        // }

        /// <summary>
        /// Sets the status text to update the user as to the current status.
        /// </summary>
        /// <param name="message">The message to display</param>
        private static void SetStatusText(string message)
        {
            if (_statusText == null)
            {
                Debug.Log(message);
            }
            else
            {
                _statusText.text = message;
            }
        }

        /// <summary>
        ///     Starts the connection to the database.
        /// </summary>
        /// <param name="checkDB">Whether to run the database validation</param>
        private void StartDBConnection(bool checkDB = false)
        {
            if (File.Exists(_DATABASE_PATH))
            {
                DatabaseManager.GetDbConnection(_DATABASE_PATH, _DB_KEY, checkDB);

                if (DatabaseManager.GetCurrentDbConnection() == null)
                {
                    SetStatusText(_BAD_DB_MESSAGE);
                }
                else
                {
                    _statusText.parent.Remove(_statusText);
                }
            }
            else
            {
                SetStatusText(_NO_DB_MESSAGE);
            }
        }
    }
}
