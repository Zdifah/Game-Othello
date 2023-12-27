using Moq;
using Othello;
using System.Numerics;

namespace Game_Othello.Test
{
    [TestFixture]
    public class Tests
    {
        private GameController _game;


        [SetUp]
        public void Setup()
        {
            
            _game = new GameController();
        }

        [TestCase(2)]
        [TestCase(4)]
        [TestCase(6)]
        public void SetSizeBoard_ShouldSuccesReturnTrue_SizeEven(int size)
        {
            bool actual = _game.SetSizeBoard(size);
            Assert.IsTrue(actual);
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public void SetSizeBoard_ShouldFailedReturnFalse_SizeOdd(int size)
        {
            bool actual = _game.SetSizeBoard(size);

            Assert.IsFalse(actual);
        }

        [Test]
        public void AddPlayer_ShouldSuccesReturnTrue()
        {
            Mock<IPlayer> _player1 = new Mock<IPlayer>();
            _player1.Setup(x => x.Id).Returns(1);

            Mock<IPlayer> _player2= new Mock<IPlayer>();
            _player2.Setup(x => x.Id).Returns(2);

            bool actual1 = _game.AddPlayer(_player1.Object, Disc.Black);
            bool actual2 = _game.AddPlayer(_player2.Object, Disc.White);

            Assert.IsTrue(actual1);
            Assert.IsTrue(actual2);
        }

        [Test]
        public void AddPlayer_ShouldFailedReturnFalse_DiscNone()
        {
            Mock<IPlayer> _player1 = new Mock<IPlayer>();
            _player1.Setup(x => x.Id).Returns(1);

            bool actual = _game.AddPlayer(_player1.Object, Disc.None);
            Assert.IsFalse(actual);
        }

        [Test]
        public void AddPlayer_ShouldFailedReturnFalse_PlayerIdAlreadyExits()
        {
            Mock<IPlayer> _player1 = new Mock<IPlayer>();
            _player1.Setup(x => x.Id).Returns(1);

            Mock<IPlayer> _player2 = new Mock<IPlayer>();
            _player2.Setup(x => x.Id).Returns(1);

            bool actual1 = _game.AddPlayer(_player1.Object, Disc.Black);
            bool actual2 = _game.AddPlayer(_player2.Object, Disc.White);

            Assert.IsTrue(actual1);
            Assert.IsFalse(actual2);
        }
    }
}