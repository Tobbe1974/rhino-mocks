﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;

namespace RhinoMocksIntroduction
{
    /// <summary>
    /// Ordered is useful to verify if expectation in the code calls are done in the good order.
    /// You can control the correct callin
    /// </summary>
    /// <see cref="http://www.ayende.com/wiki/Rhino+Mocks+Ordered+and+Unordered.ashx"/>
    public class RhinoMocksOrderedUnorderedTest
    {
        [Test]
        public void SaveProjectAs_NewNameWithoutConflicts()
        {
            MockRepository mocks = new MockRepository();
            IProjectView projectView = mocks.StrictMock<IProjectView>();
            IProjectRepository repository = mocks.StrictMock<IProjectRepository>();
            IProject prj = mocks.StrictMock<IProject>();

            //Component to test
            IProjectPresenter presenter = new ProjectPresenter(prj, repository, projectView);

            string question = "Mock ?";
            string answer = "Yes";
            string newProjectName = "RhinoMocks";

            //Not expected but its necessary for the implementation
            //We give the property behavior to prj.Name
            Expect.Call(prj.Name).PropertyBehavior();

            using (mocks.Ordered())
            {
                Expect.Call(projectView.Title).
                    Return(prj.Name);
                Expect.Call(projectView.Ask(question, answer)).
                    Return(newProjectName);
                Expect.Call(repository.GetProjectByName(newProjectName)).
                    Return(null);

                projectView.Title = newProjectName;
                projectView.HasChanges = false;

                repository.SaveProject(prj);
            }

            mocks.ReplayAll();

            Assert.IsTrue(presenter.SaveProjectAs());
            Assert.AreEqual(newProjectName, prj.Name);

            mocks.VerifyAll();
        }

        [Test]
        public void MovingFundsUsingTransactions()
        {
            MockRepository mocks = new MockRepository();
            IDatabaseManager databaseManager = mocks.StrictMock<IDatabaseManager>();
            IBankAccount accountOne = mocks.StrictMock<IBankAccount>(),
                         accountTwo = mocks.StrictMock<IBankAccount>();
            using (mocks.Ordered())
            {
                Expect.Call(databaseManager.BeginTransaction()).Return(databaseManager);
                using (mocks.Unordered())
                {
                    Expect.Call(accountOne.Withdraw(1000));
                    Expect.Call(accountTwo.Deposit(1000));
                }
                databaseManager.Dispose();
            }

            mocks.ReplayAll();

            Bank bank = new Bank(databaseManager);
            bank.TransferFunds(accountOne, accountTwo, 1000);

            mocks.VerifyAll();
        }
    }
}
