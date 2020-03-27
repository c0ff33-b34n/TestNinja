using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestNinja.Mocking;
using static TestNinja.Mocking.HousekeeperService;

namespace TestNinja.UnitTests.Mocking
{
    [TestFixture]
    class HousekeeperServiceTests
    {
        private HousekeeperService _service;
        private Mock<IStatementGenerator> _statementGenerator;
        private Mock<IEmailSender> _emailSender;
        private Mock<IXtraMessageBox> _messageBox;
        private DateTime _statementDate = new DateTime(2020, 03, 27);
        private Housekeeper _houseKeeper;
        private string _statementFilename;

        [SetUp]
        public void SetUp()
        {
            _houseKeeper = new Housekeeper { Email = "a", FullName = "b", Oid = 1, StatementEmailBody = "c" };
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(uow => uow.Query<Housekeeper>()).Returns(new List<Housekeeper>
            {
                _houseKeeper 
            }.AsQueryable());

            _statementFilename = "filename"; // value for happy path.
            _statementGenerator = new Mock<IStatementGenerator>();
            _statementGenerator
                .Setup(sg => sg.SaveStatement(_houseKeeper.Oid, _houseKeeper.FullName, _statementDate))
                .Returns(() => _statementFilename); // lambda expression for lazy evaluation. i.e. when test method
                                                    // changes the value of _statementFilename it uses the new value
                                                    // when this code is run, not the value assigned before.

            _emailSender = new Mock<IEmailSender>();
            _messageBox = new Mock<IXtraMessageBox>();

            _service = new HousekeeperService(
                    unitOfWork.Object,
                    _statementGenerator.Object,
                    _emailSender.Object,
                    _messageBox.Object);
        }

        [Test]
        public void SendStatementEmails_WhenCalled_GenerateStatement()
        {
            _service.SendStatementEmails(_statementDate);

            _statementGenerator.Verify(sg => sg.SaveStatement(_houseKeeper.Oid, _houseKeeper.FullName, _statementDate));
        }

        [Test]
        public void SendStatementEmails_HousekeepersEmailIsWhitespace_ShouldNotGenerateStatement()
        {
            _houseKeeper.Email = " ";

            _service.SendStatementEmails(_statementDate);

            _statementGenerator.Verify(sg => sg.SaveStatement(_houseKeeper.Oid,
                _houseKeeper.FullName,
                _statementDate),
                Times.Never);
        }

        [Test]
        public void SendStatementEmails_HousekeepersEmailIsEmpty_ShouldNotGenerateStatement()
        {
            _houseKeeper.Email = "";

            _service.SendStatementEmails(_statementDate);

            _statementGenerator.Verify(sg => sg.SaveStatement(_houseKeeper.Oid,
                _houseKeeper.FullName,
                _statementDate),
                Times.Never);
        }

        [Test]
        public void SendStatementEmails_WhenCalled_EmailStatement()
        {           
            _service.SendStatementEmails(_statementDate);

            VerifyEmailSent();
        }

        [Test]
        public void SendStatementEmails_StatementFilenameIsNull_ShouldNotEmailStatement()
        {
            _statementFilename = null;

            _service.SendStatementEmails(_statementDate);

            VerifyEmailNotSent();
        }

        [Test]
        public void SendStatementEmails_StatementFilenameIsWhitespace_ShouldNotEmailStatement()
        {
            _statementFilename = " ";

            _service.SendStatementEmails(_statementDate);

            VerifyEmailNotSent();
        }

        [Test]
        public void SendStatementEmails_StatementFilenameIsEmptyString_ShouldNotEmailStatement()
        {
            _statementFilename = "";

            _service.SendStatementEmails(_statementDate);

            VerifyEmailNotSent();
        }

        private void VerifyEmailNotSent()
        {
            _emailSender.Verify(es => es.EmailFile(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        private void VerifyEmailSent()
        {
            _emailSender.Verify(es => es.EmailFile(
                _houseKeeper.Email,
                _houseKeeper.StatementEmailBody,
                _statementFilename,
                It.IsAny<string>()));
        }
    }
}
