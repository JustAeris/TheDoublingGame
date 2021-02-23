using System.Numerics;

namespace TheDoublingGame
{
    public class GuildConfiguration
    {
        public ulong GuildId { get; init; }
        
        public bool IsSetup { get; set; }
        
        public ulong CountingChannelId { get; set; }
        
        public BigInteger LastNumber { get; set; }

        public ulong LastPlayerId { get; set; }
        
        public BigInteger HighScore { get; set; }
        
        public bool GameEnabled { get; set; }
    }
}