using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Domain.Entities;

public class Site
{
    public int SiteId { get; set; }
    public string Nom { get; set; } = string.Empty;
}