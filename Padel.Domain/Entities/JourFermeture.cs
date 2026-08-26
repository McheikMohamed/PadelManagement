using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Domain.Entities;

public class JourFermeture
{
    public int FermetureId { get; set; }
    public int? SiteId { get; set; }  // null = fermeture globale
    public DateOnly Date { get; set; }
}
