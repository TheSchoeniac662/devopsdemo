using System;
using Loans.Domain.Applications;
using NUnit.Framework;
using Moq;
using Moq.Protected;

namespace Loans.Tests
{
    class LoanApplicationProcessorShould
    {
        [Test]
        public void DeclineLowSalary()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                product,
                                                amount,
                                                "Sarah",
                                                25,
                                                "133 Pluralsight Drive, Draper, Utah",
                                                64_999);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();

            var mockCreditScorer = new Mock<ICreditScorer>();

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                    mockCreditScorer.Object);

            sut.Process(application);

            Assert.That(application.GetIsAccepted(), Is.False);
        }

        delegate void ValidateCallback(string applicantName,
                                    int applicantAge,
                                    string applicantAddress,
                                    ref IdentityVerificationStatus status);

        [Test]
        public void Accept()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42, product, amount, "Sarah",25,
                                    "133 Pluralsight Drive, Draper, Utah", 65_000);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>(MockBehavior.Strict);

            mockIdentityVerifier.Setup(x => x.Initialize());

            //mockIdentityVerifier.Setup(x => x.Validate(It.IsAny<string>(),
            //                                            It.IsAny<int>(),
            //                                            It.IsAny<string>())).Returns(true);

            mockIdentityVerifier.Setup(x => x.Validate("Sarah", 25,
                            "133 Pluralsight Drive, Draper, Utah")).Returns(true);

            //bool isValidOutValue = true;
            //mockIdentityVerifier.Setup(x => x.Validate("Sarah", 25,
            //                "133 Pluralsight Drive, Draper, Utah",
            //                out isValidOutValue));

            //mockIdentityVerifier
            //    .Setup(x => x.Validate("Sarah",
            //                                            25,
            //                                            "133 Pluralsight Drive, Draper, Utah",
            //                                            ref It.Ref<IdentityVerificationStatus>.IsAny))
            //    .Callback(new ValidateCallback(
            //                (string applicantName,
            //                int applicantAge,
            //                string applicantAddress,
            //                ref IdentityVerificationStatus status) =>
            //                                status = new IdentityVerificationStatus(true)));

            var mockCreditScorer
                = new Mock<ICreditScorer>(); // { DefaultValue = DefaultValue.Empty };

            mockCreditScorer.SetupAllProperties();

            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);
            //mockCreditScorer.SetupProperty(x => x.Count);

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                    mockCreditScorer.Object);

            sut.Process(application);

            //verify the Score property's getter was called
            mockCreditScorer.VerifyGet(x => x.ScoreResult.ScoreValue.Score, Times.Once);
            //mockCreditScorer.VerifySet(x => x.Count = 1, Times.Once);

            Assert.That(application.GetIsAccepted(), Is.True);
            Assert.That(mockCreditScorer.Object.Count, Is.EqualTo(1));
        }

        [Test]
        public void NullReturnExample()
        {
            var mock = new Mock<INullExample>();

            mock.Setup(x => x.SomeMethod()); //ref type returns null by default
                      //.Returns<string>(null); 

            string mockReturnValue = mock.Object.SomeMethod();

            Assert.That(mockReturnValue, Is.Null);
        }

        [Test]
        public void InitializeIdentityVerifier()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42, product, amount, "Sarah", 25,
                                    "133 Pluralsight Drive, Draper, Utah", 65_000);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();
            
            mockIdentityVerifier.Setup(x => x.Validate("Sarah", 25,
                            "133 Pluralsight Drive, Draper, Utah")).Returns(true);
            
            var mockCreditScorer
                = new Mock<ICreditScorer>(); 

            mockCreditScorer.SetupAllProperties();

            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                    mockCreditScorer.Object);

            sut.Process(application);

            //verify that the initialize method was called by our mockIdentityVerifier
            mockIdentityVerifier.Verify(x => x.Initialize());

            mockIdentityVerifier.Verify(x => x.Validate(It.IsAny<string>(),
                                                        It.IsAny<int>(),
                                                        It.IsAny<string>()));

            mockIdentityVerifier.VerifyNoOtherCalls();
        }

        [Test]
        public void CalculateScore()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42, product, amount, "Sarah", 25,
                                    "133 Pluralsight Drive, Draper, Utah", 65_000);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();

            mockIdentityVerifier.Setup(x => x.Validate("Sarah", 25,
                            "133 Pluralsight Drive, Draper, Utah")).Returns(true);

            var mockCreditScorer
                = new Mock<ICreditScorer>();

            mockCreditScorer.SetupAllProperties();

            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                    mockCreditScorer.Object);

            sut.Process(application);

            //expected to see the CalculateScore() invoked with these parameters
            mockCreditScorer.Verify(x => x.CalculateScore("Sarah",
                            "133 Pluralsight Drive, Draper, Utah"),
                            Times.Once);

            //mockCreditScorer.Verify(x => x.CalculateScore(It.IsAny<string>(), 
            //                                            It.IsAny<string>())); 
        }

        [Test]
        public void DeclineWhenCreditScoreError()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42, product, amount, "Sarah", 25,
                                    "133 Pluralsight Drive, Draper, Utah", 65_000);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();

            mockIdentityVerifier.Setup(x => x.Validate("Sarah", 25,
                            "133 Pluralsight Drive, Draper, Utah")).Returns(true);
            
            var mockCreditScorer
                = new Mock<ICreditScorer>();

            mockCreditScorer.SetupAllProperties();

            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            mockCreditScorer.Setup(x => x.CalculateScore(It.IsAny<string>(), It.IsAny<string>()))
                                    .Throws<InvalidOperationException>();

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                    mockCreditScorer.Object);

            sut.Process(application);

            Assert.That(application.GetIsAccepted(), Is.False);
        }

        [Test]
        public void AcceptUsingPartialMock()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42, product, amount, "Sarah", 25,
                                    "133 Pluralsight Drive, Draper, Utah", 65_000);

            var mockIdentityVerifier = new Mock<IdentityVerifierServiceGateway>();

            mockIdentityVerifier.Protected().Setup<bool>("CallService", "Sarah", 25,
                "133 Pluralsight Drive, Draper, Utah").Returns(true);

            var mockCreditScorer = new Mock<ICreditScorer>();
            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                    mockCreditScorer.Object);

            sut.Process(application);

            Assert.That(application.GetIsAccepted(), Is.True);
        }
    }

    public interface INullExample
    {
        string SomeMethod();
    }
}
