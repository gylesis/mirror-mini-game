using Dev.Infrastructure;
using Dev.ScoreLogic;
using Mirror;

namespace Dev.Utils
{
    public static class MyCustomDataTypesSerializer
    {
        public static void WriteScoreData(this NetworkWriter writer, ScoreData scoreData)
        {
            writer.WriteInt(scoreData.Score);
            writer.Write(scoreData.NetworkIdentity);
        }

        public static ScoreData ReadScoreData(this NetworkReader reader)
        {
            var scoreData = new ScoreData();

            scoreData.Score = reader.ReadInt();
            scoreData.NetworkIdentity = reader.ReadNetworkIdentity();

            return scoreData;
        }

        public static void WriteGameEndContext(this NetworkWriter writer, GameEndContext context)
        {
            writer.Write(context.Winner);
            writer.Write(context.WinnerScoreData);
        }

        public static GameEndContext ReadGameEndContext(this NetworkReader reader)
        {
            var context = new GameEndContext();

            context.Winner = reader.ReadNetworkIdentity();
            context.WinnerScoreData = reader.ReadScoreData();

            return context;
        }
    }
}