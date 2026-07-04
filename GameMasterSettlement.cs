using System;
using System.Collections.Generic;
using System.Linq;
using CafeManager.Models;

namespace CafeManager
{
    public class GameMasterSettlement
    {
        public string GameMasterName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<GameSession> Games { get; set; } = new List<GameSession>();
        public DateTime SettlementDate { get; set; } = DateTime.Now;
        public bool IsSettled { get; set; } = false;
        public string SettlementMethod { get; set; } = "نقدی";

        public int TotalGames => Games?.Count ?? 0;
        public double TotalRevenue => Games?.Sum(g => g.TotalRevenue) ?? 0;
        public double TotalShare => Games?.Sum(g => g.GameMasterShare) ?? 0;
        public double AverageRevenue => TotalGames > 0 ? TotalRevenue / TotalGames : 0;
        public double AverageShare => TotalGames > 0 ? TotalShare / TotalGames : 0;
    }
}