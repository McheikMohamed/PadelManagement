using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Application.Dtos;

namespace Padel.Application.Interfaces;

public interface IMembreService
{
    Task CreerMembreAsync(CreerMembreDto dto);
    Task<MembreDto?> ObtenirMembreAsync(string matricule);
    Task<List<MembreDto>> ListerMembresAsync(int? siteId);
}