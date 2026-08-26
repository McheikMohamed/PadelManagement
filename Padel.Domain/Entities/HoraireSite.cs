using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Domain.Entities;

public class HoraireSite
{
    public int HoraireId { get; set; }
    public int SiteId { get; set; }
    public int Annee { get; set; }
    public TimeOnly HeureOuverture { get; set; }
    public TimeOnly HeureFermeture { get; set; }
}
