namespace Mars.Web.Tests
{
    internal class MultiGameTests
    {
        [TestCase("a", "b")]
        [TestCase("z", "aa")]
        [TestCase("az", "ba")]
        [TestCase("zz", "aaa")]
        [TestCase("abz", "aca")]
        [TestCase("zza", "zzb")]
        [TestCase("abc", "abd")]
        public void IncrementGameLogic(string starting, string expected)
        {
            MultiGameHoster.IncrementGameId(starting).Should().Be(expected);
        }
    }
}
