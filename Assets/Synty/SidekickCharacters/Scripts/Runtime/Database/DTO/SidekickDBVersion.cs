// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using SQLite.Attributes;
using System;

namespace Synty.SidekickCharacters.Database.DTO
{
    [Table("sk_vdata")]
    public class SidekickDBVersion
    {
        [PrimaryKey, Column("id")]
        public int ID { get; set; }
        [Column("semantic_version")]
        public string SemanticVersion { get; set; }
        [Column("update_time")]
        public DateTime LastUpdated { get; set; }
    }
}
