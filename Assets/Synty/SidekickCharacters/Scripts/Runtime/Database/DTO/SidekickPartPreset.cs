// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using SqlCipher4Unity3D;
using SQLite.Attributes;
using Synty.SidekickCharacters.Enums;
using System.Collections.Generic;

namespace Synty.SidekickCharacters.Database.DTO
{
    [Table("sk_part_preset")]
    public class SidekickPartPreset
    {
        private SidekickSpecies _species;

        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        public int ID { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("part_group")]
        public PartGroup PartGroup { get; set; }
        [Column("ptr_species")]
        public int PtrSpecies { get; set; }
        [Column("outfit")]
        public string Outfit { get; set; }

        [Ignore]
        public SidekickSpecies Species
        {
            get => _species;
            set
            {
                _species = value;
                PtrSpecies = value.ID;
            }
        }

        /// <summary>
        ///     Gets a specific Preset by its database ID.
        /// </summary>
        /// <param name="dbManager">The Database Manager to use.</param>
        /// <param name="id">The id of the required Preset.</param>
        /// <returns>The specific Preset if it exists; otherwise null.</returns>
        public static SidekickPartPreset GetByID(DatabaseManager dbManager, int id)
        {
            SidekickPartPreset partPreset = dbManager.GetCurrentDbConnection().Find<SidekickPartPreset>(id);
            Decorate(dbManager, partPreset);
            return partPreset;
        }

        /// <summary>
        ///     Gets a list of all the Presets in the database.
        /// </summary>
        /// <param name="dbManager">The Database Manager to use.</param>
        /// <returns>A list of all presets in the database.</returns>
        public static List<SidekickPartPreset> GetAll(DatabaseManager dbManager)
        {
            List<SidekickPartPreset> partPresets = dbManager.GetCurrentDbConnection().Table<SidekickPartPreset>().ToList();

            foreach (SidekickPartPreset partPreset in partPresets)
            {
                Decorate(dbManager, partPreset);
            }

            return partPresets;
        }

        /// <summary>
        ///     Gets a list of all the Part Presets in the database that have the matching species.
        /// </summary>
        /// <param name="dbManager">The Database Manager to use.</param>
        /// <param name="species">The species to get all the part presets for.</param>
        /// <returns>A list of all part presets in the database for the given species.</returns>
        public static List<SidekickPartPreset> GetAllBySpecies(DatabaseManager dbManager, SidekickSpecies species)
        {
            List<SidekickPartPreset> partPresets = dbManager.GetCurrentDbConnection().Table<SidekickPartPreset>()
                .Where(partPreset => partPreset.PtrSpecies == species.ID)
                .ToList();

            foreach (SidekickPartPreset partPreset in partPresets)
            {
                partPreset.Species = species;
            }

            return partPresets;
        }

        /// <summary>
        ///     Gets a list of all the Part Presets in the database that have the matching species and part group.
        /// </summary>
        /// <param name="dbManager">The Database Manager to use.</param>
        /// <param name="partGroup">The part group to filter search by.</param>
        /// <returns>A list of all part presets in the database for the given species and part group.</returns>
        public static List<SidekickPartPreset> GetAllByGroup(DatabaseManager dbManager, PartGroup partGroup)
        {
            List<SidekickPartPreset> partPresets = dbManager.GetCurrentDbConnection().Table<SidekickPartPreset>()
                .Where(partPreset => partPreset.PartGroup == partGroup)
                .ToList();

            foreach (SidekickPartPreset partPreset in partPresets)
            {
                Decorate(dbManager, partPreset);
            }

            return partPresets;
        }

        /// <summary>
        ///     Gets a list of all the Part Presets in the database that have the matching species and part group.
        /// </summary>
        /// <param name="dbManager">The Database Manager to use.</param>
        /// <param name="species">The species to get all the part presets for.</param>
        /// <param name="partGroup">The part group to filter search by.</param>
        /// <returns>A list of all part presets in the database for the given species and part group.</returns>
        public static List<SidekickPartPreset> GetAllBySpeciesAndGroup(DatabaseManager dbManager, SidekickSpecies species, PartGroup partGroup)
        {
            List<SidekickPartPreset> partPresets = dbManager.GetCurrentDbConnection().Table<SidekickPartPreset>()
                .Where(partPreset => partPreset.PtrSpecies == species.ID && partPreset.PartGroup == partGroup)
                .ToList();

            foreach (SidekickPartPreset partPreset in partPresets)
            {
                partPreset.Species = species;
            }

            return partPresets;
        }

        /// <summary>
        ///     Ensures that the given set has its nice DTO class properties set
        /// </summary>
        /// <param name="dbManager">The Database Manager to use.</param>
        /// <param name="partPreset">The color set to decorate</param>
        /// <returns>A color set with all DTO class properties set</returns>
        private static void Decorate(DatabaseManager dbManager, SidekickPartPreset partPreset)
        {
            if (partPreset.Species == null && partPreset.PtrSpecies >= 0)
            {
                partPreset.Species = SidekickSpecies.GetByID(dbManager, partPreset.PtrSpecies);
            }
        }
    }
}
