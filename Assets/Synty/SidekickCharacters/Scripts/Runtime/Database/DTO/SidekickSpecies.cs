// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using SQLite.Attributes;
using System.Collections.Generic;

namespace Synty.SidekickCharacters.Database.DTO
{
    [Table("sk_species")]
    public class SidekickSpecies
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int ID { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("code")]
        public string Code { get; set; }

        /// <summary>
        ///     Gets a list of all the Species in the database.
        /// </summary>
        /// <param name="dbManager">The Database Manager to use.</param>
        /// <returns>A list of all species in the database.</returns>
        public static List<SidekickSpecies> GetAll(DatabaseManager dbManager)
        {
            return dbManager.GetCurrentDbConnection().Table<SidekickSpecies>().ToList();
        }

        /// <summary>
        ///     Gets a specific Species by its database ID.
        /// </summary>
        /// <param name="dbManager">The Database Manager to use.</param>
        /// <param name="id">The id of the required Species.</param>
        /// <returns>The specific Species if it exists; otherwise null.</returns>
        public static SidekickSpecies GetByID(DatabaseManager dbManager, int id)
        {
            return dbManager.GetCurrentDbConnection().Get<SidekickSpecies>(id);
        }
    }
}
