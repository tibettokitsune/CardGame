// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using SQLite.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synty.SidekickCharacters.Database.DTO
{
    [Table("sk_pdata")]
    public class SidekickPart
    {
        [PrimaryKey, Column("id")]
        public int ID { get; set; }
        [Column("guid")]
        public string Guid { get; set; }
        [Column("type")]
        public string Type { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        ///     Gets a list of all the parts in the database.
        /// </summary>
        /// <param name="dbManager">The Database Manager to use.</param>
        /// <returns>A list of all parts in the database.</returns>
        public static List<SidekickPart> GetAll(DatabaseManager dbManager)
        {
            return dbManager.GetCurrentDbConnection().Table<SidekickPart>().ToList();
        }

        /// <summary>
        ///     Gets a list of all parts that match the given metadata type and value provided.
        /// </summary>
        /// <param name="dbManager">The Database Manager to use.</param>
        /// <param name="type">The metadata type to search for.</param>
        /// <param name="value">The metadata value to search for.</param>
        /// <returns>A list of all parts that match the given metadata type and value provided.</returns>
        public static List<SidekickPart> GetAllByMetadataTypeAndValue(DatabaseManager dbManager, string type, string value)
        {
            List<string> partGuids = SidekickPartMetaData.GetPartGuidsByMetaDataValue(dbManager, type, value);
            if (partGuids.Count == 0)
            {
                return new List<SidekickPart>();
            }

            return dbManager.GetCurrentDbConnection().Table<SidekickPart>().Where(part => partGuids.Contains(part.Guid)).ToList();
        }

        /// <summary>
        ///     Gets the species for a specific part.
        ///     TODO: get from the DB when data is populated.
        /// </summary>
        /// <param name="allSpecies">The list of all species to return a populated species object from.</param>
        /// <param name="partName">The name of the part to get the species for.</param>
        /// <returns>The species the part belongs to.</returns>
        public static SidekickSpecies GetSpeciesForPart(List<SidekickSpecies> allSpecies, string partName)
        {
            // species is currently contained in the last block of the part name
            if (partName.Split('_').Last().Substring(0, 2) == "GO")
            {
                return allSpecies.FirstOrDefault(s => s.Name == "Goblin");
            }

            return allSpecies.FirstOrDefault(s => s.Name == "Human");
        }
    }
}
