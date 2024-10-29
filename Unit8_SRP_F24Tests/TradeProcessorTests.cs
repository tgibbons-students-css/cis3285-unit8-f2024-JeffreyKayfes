using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;

namespace SingleResponsibilityPrinciple.Tests
{
    [TestClass()]
    public class TradeProcessorTests
    {
        private int CountDbRecords()
        {
            string myConnectiionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\jdkay\Documents\tradedatabase.mdf;Integrated Security=True;Connect Timeout=30";
            // Change the connection string used to match the one you want
            using (var connection = new SqlConnection(myConnectiionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string myScalarQuery = "SELECT COUNT(*) FROM trade";
                SqlCommand myCommand = new SqlCommand(myScalarQuery, connection);
                //myCommand.Connection.Open();
                int count = (int)myCommand.ExecuteScalar();
                connection.Close();
                return count;
            }
        }

        [TestMethod()]
        public void TestNormalFile()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Unit8_SRP_F24Tests.goodtrades.txt");
            var tradeProcessor = new TradeProcessor();

            //Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);
            //Assert
            int countAfter = CountDbRecords();
            Assert.AreEqual(countBefore + 4, countAfter);
        }

        [TestMethod()]
        public void ProcessTrades_OneGoodTrade()
        {
            // Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Unit8_SRP_F24Tests.onetrade.txt");
            var tradeProcessor = new TradeProcessor();

            // Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);

            // Assert
            int countAfter = CountDbRecords();
            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod()]
        public void ProcessTrades_TenGoodTrades()
        {
            // Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Unit8_SRP_F24Tests.tengoodtrades.txt");
            var tradeProcessor = new TradeProcessor();

            // Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);

            // Assert
            int countAfter = CountDbRecords();
            Assert.AreEqual(countBefore + 10, countAfter);
        }

        [TestMethod()]
        public void ProcessTrades_EmptyFile()
        {
            // Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Unit8_SRP_F24Tests.notrades.txt");
            var tradeProcessor = new TradeProcessor();

            // Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);

            // Assert
            int countAfter = CountDbRecords();
            Assert.AreEqual(countBefore, countAfter);
        }

        [TestMethod()]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ProcessTrades_NonExistentFile()
        {
            // Arrange
            using var tradeStream = File.OpenRead("nonexistentfile.txt");
            var tradeProcessor = new TradeProcessor();

            // Act
            tradeProcessor.ProcessTrades(tradeStream);

            // Assert handled by ExpectedException
        }

        [TestMethod()]
        public void ReadTradeData_ValidTradeData()
        {
            // Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Unit8_SRP_F24Tests.validtrades.txt");
            var tradeProcessor = new TradeProcessor();

            // Act
            var lines = tradeProcessor.ReadTradeData(tradeStream);

            // Assert
            Assert.AreEqual(2, lines.Count());
            Assert.AreEqual("GBPUSD,1000,1.51", lines.ElementAt(0));
            Assert.AreEqual("GBPJPY,1500,178.13", lines.ElementAt(1));
        }

        [TestMethod()]
        public void ReadTradeData_InvalidTradeDataWithExtraFields()
        {
            // Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Unit8_SRP_F24Tests.extrafieldtrade.txt");
            var tradeProcessor = new TradeProcessor();

            // Act
            var lines = tradeProcessor.ReadTradeData(tradeStream);

            // Assert
            Assert.AreEqual(1, lines.Count());
            Assert.AreEqual("GBPUSD,1000,1.51,extraField", lines.First());
        }
    }
}