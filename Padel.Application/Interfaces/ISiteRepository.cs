using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface ISiteRepository
{
    Task<int> CreerAsync(string nom);
    Task<List<Site>> ListerAsync();
    Task<Site?> ObtenirParIdAsync(int siteId);
}