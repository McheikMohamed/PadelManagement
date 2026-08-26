using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Application.Dtos;

namespace Padel.Application.Interfaces;

public interface IAdminService
{
    Task CreerAdministrateurAsync(CreerAdministrateurDto dto, string appelantMatricule);
    Task<AdministrateurDto?> ObtenirAdministrateurAsync(string matricule);
}