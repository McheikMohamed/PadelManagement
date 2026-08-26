using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Padel.Domain.Enums;

namespace Padel.Domain.Entities;

public class Administrateur
{
    public string Matricule { get; set; } = string.Empty;
    public required TypeAdmin Type { get; set; }
    public int? SiteId { get; set; }  // null pour Global
}
