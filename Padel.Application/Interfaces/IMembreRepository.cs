using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface IMembreRepository
{
    Task CreerAsync(string matricule, string type, int? siteId);
    Task<Membre?> ObtenirParMatriculeAsync(string matricule);
    Task<List<Membre>> ListerAsync(int? siteId);
}