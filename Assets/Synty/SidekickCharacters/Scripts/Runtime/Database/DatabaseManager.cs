// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using SqlCipher4Unity3D;
using Synty.SidekickCharacters.Database.DTO;
using System;

namespace Synty.SidekickCharacters.Database
{
    /// <summary>
    ///     Manages connecting to, initializing and validating a local Sidekick SQLite database
    /// </summary>
    public class DatabaseManager
    {
        private static SQLiteConnection _connection;
        private static int _connectionHash;

        /// <summary>
        ///     Gets the DB connection with the given connection details.
        ///     If the current connection is not for the given details, the connection is replaced with the correct connection.
        /// </summary>
        /// <param name="databasePath">The path to the DB.</param>
        /// <param name="connectionKey">The connection key for the DB.</param>
        /// <param name="checkDbOnLoad">Whether or not to valiate the structure of the DB after connecting to it.</param>
        /// <returns>A connection to a DB with the given connection details.</returns>
        public SQLiteConnection GetDbConnection(string databasePath, string connectionKey, bool checkDbOnLoad = false)
        {
            int newConnectionHash = $"{databasePath}|{connectionKey}".GetHashCode();

            // if the connection isn't encrypted, the key must be null to prevent erroring pragma execution
            if (connectionKey == "")
            {
                connectionKey = null;
            }

            if (_connection == null)
            {
                _connectionHash = newConnectionHash;
                _connection = new SQLiteConnection(databasePath, connectionKey);
            }
            // TODO : maybe change handling here to re-open an existing connection if the connectionString matches? possible disposal problems
            else if (_connectionHash != newConnectionHash)
            {
                CloseConnection();
                _connectionHash = newConnectionHash;
                _connection = new SQLiteConnection(databasePath, connectionKey);
            }
            else
            {
                return _connection;
            }

            if (checkDbOnLoad)
            {
                InitialiseDatabase();

                if (!IsDatabaseConfigured())
                {
                    throw new Exception("Database not configured correctly");
                }
            }

            return _connection;
        }

        /// <summary>
        ///     Returns whatever the current DB connection is, regardless of state.
        /// </summary>
        /// <returns>The current DB connection.</returns>
        public SQLiteConnection GetCurrentDbConnection()
        {
            return _connection;
        }

        /// <summary>
        ///     Closes the current DB connection.
        /// </summary>
        public void CloseConnection()
        {
            if (_connection == null)
            {
                return;
            }

            // NOTE: Previously, if we didn't clear the pool in the connection, it resulted in a file lock on the database,
            //       which in some cases could cause problems when trying to update the DB to a new version.
            //       Our SQLCipher library doesn't use pool clearing unless using SQLiteAsyncConnection.
            _connection.Dispose();
            // Our SQLCipher library doesn't surface checking connection state; disposed connections need their reference removed
            _connection = null;
        }

        /// <summary>
        ///     Initialises the Sidekicks database with required data, if they don't already exist.
        /// </summary>
        private void InitialiseDatabase()
        {
            // ensure we have a default color set (a bunch of other code relies on this, not safe to remove yet)
            try
            {
                SidekickColorSet.GetDefault(this);
            }
            catch (Exception)
            {
                SidekickColorSet newSet = new SidekickColorSet
                {
                    Species = new SidekickSpecies { ID = -1, Name = "None" },
                    Name = "Default",
                    SourceColorPath = "Assets/Synty/Tools/SidekickCharacters/Resources/Textures/T_ColorMap.png",
                    SourceMetallicPath = "Assets/Synty/Tools/SidekickCharacters/Resources/Textures/T_MetallicMap.png",
                    SourceSmoothnessPath = "Assets/Synty/Tools/SidekickCharacters/Resources/Textures/T_SmoothnessMap.png",
                    SourceReflectionPath = "Assets/Synty/Tools/SidekickCharacters/Resources/Textures/T_ReflectionMap.png",
                    SourceEmissionPath = "Assets/Synty/Tools/SidekickCharacters/Resources/Textures/T_EmissionMap.png",
                    SourceOpacityPath = "Assets/Synty/Tools/SidekickCharacters/Resources/Textures/T_OpacityMap.png",
                };

                newSet.Save(this);
            }
        }

        /// <summary>
        ///     Checks to see if the current database has the required tables.
        ///     TODO: Check DB version, not just if table is present.
        /// </summary>
        /// <returns>True if the tables are present; otherwise false.</returns>
        private bool IsDatabaseConfigured()
        {
            return GetCurrentDbConnection().GetTableInfo("sk_vdata").Count > 0;
        }

        /// <summary>
        ///     Retrieves the current database version.
        /// </summary>
        /// <returns>Semantic version (major.minor.patch).</returns>
        public Version GetDatabaseVersion()
        {
            return new Version(GetCurrentDbConnection()?.Table<SidekickDBVersion>().FirstOrDefault().SemanticVersion ?? "0.0.1ea");
        }
    }
}
