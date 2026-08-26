using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface IAdministrateurRepository
{
    Task CreerAsync(string matricule, string type, int? siteId);
    Task<Administrateur?> ObtenirParMatriculeAsync(string matricule);
}