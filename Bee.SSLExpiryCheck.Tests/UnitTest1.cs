namespace SSLExpiryCheck.Tests
{
    [TestClass]
    public class UnitTest1
    {
        string _TestHostName = "google.com";

        [TestMethod]
        public async Task TestIsNotNull()
        {
            var r = await SSLChecker.GetExpirationDate(_TestHostName);

            Assert.IsNotNull(r);
        }

        [TestMethod]
        public async Task TestGetRemainingDays()
        {
            var r = await SSLChecker.GetRemainingDays(_TestHostName);

            Assert.IsTrue(r.Count > 0);
            Assert.AreEqual<string>(_TestHostName, r.Keys.First());
        }
    }
}