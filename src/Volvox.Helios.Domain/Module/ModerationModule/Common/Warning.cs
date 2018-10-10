﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    [Table("mod_warnings")]
    public class Warning
    {
        [Key]
        public int Id { get; set; }

        public virtual UserWarnings User { get; set; }

        [Required]
        public WarningType WarningType { get; set; }

        [Required]
        public DateTimeOffset WarningRecieved { get; set; }

        [Required]
        public DateTimeOffset WarningExpires { get; set; }
    }
}
