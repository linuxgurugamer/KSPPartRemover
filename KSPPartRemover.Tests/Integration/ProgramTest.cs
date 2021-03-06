﻿using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using KSPPartRemover.KspFormat;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Tests.Integration
{
    public class ProgramTest
    {
        private static readonly String TestFilePath = "test.sfs";
        private static readonly StringBuilder StdOutput = new StringBuilder ();
        private static readonly StringWriter StdOutputWriter = new StringWriter (StdOutput);

        [TestFixtureSetUp]
        public static void TestFixtureSetUp ()
        {
            Console.SetOut (StdOutputWriter);
        }

        [TestFixtureTearDown]
        public static void TestFixtureTearDown ()
        {
            StdOutputWriter.Dispose ();
        }

        [TearDown]
        public void TearDown ()
        {
            StdOutput.Clear ();
        }

        [Test]
        public void PrintsInfoHeaderIfSilentSwitchIsNotOn ()
        {
            // given
            var inputCrafts = new KspObject ("GAME");
            var inputText = KspObjToString (inputCrafts);

            // when
            File.WriteAllText (TestFilePath, inputText);
            var returnCode = Program.Main ("list-crafts", "-i", TestFilePath);

            // then
            Assert.That (StdOutput.ToString (), Is.StringStarting ("KSPPartRemover v"));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void PrintsInfoHeaderOnError ()
        {
            // when
            Program.Main ();

            // then
            Assert.That (StdOutput.ToString (), Is.StringStarting ("KSPPartRemover v"));
        }

        [Test]
        public void PrintsUsageOnError ()
        {
            // when
            Program.Main ();

            // then
            Assert.That (StdOutput.ToString (), Is.StringContaining ("usage: "));
        }

        [Test]
        public void PrintsErrorMessageOnError ()
        {
            // when
            var returnCode = Program.Main ();

            // then
            Assert.That (StdOutput.ToString (), Is.StringEnding (
                "ERROR: Required argument 0 missing" + Environment.NewLine +
                "ERROR: Required switch argument 'i' missing" + Environment.NewLine));
            Assert.That (returnCode, Is.LessThan (0));
        }

        [Test]
        public void HasReturnValueLessThanZeroIfArgumentsAreInvalid ()
        {
            // when
            var returnCode = Program.Main ();

            // then
            Assert.That (returnCode, Is.LessThan (0));
        }

        [Test]
        public void CanRemovePartByIdAndOutputResult ()
        {
            // given
            var inputCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")));

            var expectedCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")));

            var inputText = KspObjToString (inputCraft);
            var expectedResult = KspObjToString (expectedCraft);

            // when
            File.WriteAllText (TestFilePath, inputText);
            var returnCode = Program.Main ("remove-parts", "-p", "0", "-i", TestFilePath, "-s");

            // then
            Assert.That (File.ReadAllText (TestFilePath), Is.EqualTo (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void CanRemovePartByNameAndOutputResult ()
        {
            // given
            var inputCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")));

            var expectedCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")));

            var inputText = KspObjToString (inputCraft);
            var expectedResult = KspObjToString (expectedCraft);

            // when
            File.WriteAllText (TestFilePath, inputText);
            var returnCode = Program.Main ("remove-parts", "-p", "fuelTank", "-i", TestFilePath, "-s");

            // then
            Assert.That (File.ReadAllText (TestFilePath), Is.EqualTo (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void CanRemovePartsOfMultipleCraftsAndOutputResult ()
        {
            // given
            var inputCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft1"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft2"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft3"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft4")));

            var expectedCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft1"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft2"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft3"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft4")));
            
            var inputText = KspObjToString (inputCrafts);
            var expectedResult = KspObjToString (expectedCrafts);

            // when
            File.WriteAllText (TestFilePath, inputText);
            var returnCode = Program.Main ("remove-parts", "-p", "fuelTank", "-c", "!craft2", "-i", TestFilePath, "-o", TestFilePath, "-s");

            // then
            Assert.That (File.ReadAllText (TestFilePath), Is.EqualTo (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void CanPrintCraftList ()
        {
            // given
            var inputCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "someCraft")))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "anotherCraft")))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "ignored")));
            
            var inputText = KspObjToString (inputCrafts);

            var expectedResult = "someCraft" + Environment.NewLine + "anotherCraft" + Environment.NewLine;

            // when
            File.WriteAllText (TestFilePath, inputText);
            var returnCode = Program.Main ("list-crafts", "-c", ".*Craft", "-i", TestFilePath);

            // then
            Assert.That (StdOutput.ToString (), Is.StringEnding (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void CanPrintPartList ()
        {
            // given
            var inputCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "someCraft"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "anotherCraft"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "ignored"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "ignored"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "somePart"))));
            
            var inputText = KspObjToString (inputCrafts);

            var expectedResult =
                "someCraft:" + Environment.NewLine +
                "\t[0]fuelTank" + Environment.NewLine +
                "\t[1]strut" + Environment.NewLine +
                "anotherCraft:" + Environment.NewLine +
                "\t[0]strut" + Environment.NewLine +
                "\t[1]fuelTank" + Environment.NewLine;

            // when
            File.WriteAllText (TestFilePath, inputText);
            var returnCode = Program.Main ("list-parts", "-p", "[s,f].*", "-c", ".*Craft", "-i", TestFilePath);

            // then
            Assert.That (StdOutput.ToString (), Is.StringEnding (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void CanPrintPartDependencies ()
        {
            // given
            var inputCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "someCraft"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank1")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "anotherCraft"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank2"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "ignored"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "somePart"))));

            var craft1 = inputCrafts.Children [0];
            var craft2 = inputCrafts.Children [1];

            craft1.Children [0].AddProperty (new KspPartLinkProperty (KspPartLinkProperty.Types.Link, null, (KspPartObject)craft1.Children [1], true));
            craft1.Children [1].AddProperty (new KspPartLinkProperty (KspPartLinkProperty.Types.Parent, null, (KspPartObject)craft1.Children [0], false));
            craft1.Children [1].AddProperty (new KspPartLinkProperty (KspPartLinkProperty.Types.Sym, "top", (KspPartObject)craft1.Children [0], false));
            craft2.Children [0].AddProperty (new KspPartLinkProperty (KspPartLinkProperty.Types.AttN, "bottom", (KspPartObject)craft2.Children [1], false));

            var inputText = KspObjToString (inputCrafts);

            var expectedResult =
                "someCraft:" + Environment.NewLine +
                "\t[1]strut:" + Environment.NewLine +
                "\t\t[0]fuelTank1[parent]" + Environment.NewLine +
                "\t\t[0]fuelTank1[sym(top)]" + Environment.NewLine +
                "anotherCraft:" + Environment.NewLine +
                "\t[0]strut:" + Environment.NewLine +
                "\t\t[1]fuelTank2[attN(bottom)]" + Environment.NewLine;
            
            // when
            File.WriteAllText (TestFilePath, inputText);
            var returnCode = Program.Main ("list-partdeps", "-p", ".*uelTank.*", "-c", ".*Craft", "-i", TestFilePath);

            // then
            Assert.That (StdOutput.ToString (), Is.StringEnding (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void PrintsAndReturnsErrorIfPartIdToRemoveIsNotFound ()
        {
            // given
            var inputCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "notAPart")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "somePart")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "anotherPart")));
            
            var inputText = KspObjToString (inputCraft);

            // when
            File.WriteAllText (TestFilePath, inputText);
            var returnCode = Program.Main ("remove-parts", "-p", "2", "-i", TestFilePath, "-s");

            // then
            Assert.That (StdOutput.ToString (), Is.EqualTo ("ERROR: No parts removed\n"));
            Assert.That (returnCode, Is.LessThan (0));
        }

        [Test]
        public void PrintsAndReturnsErrorIfPartNameToRemoveIsNotFound ()
        {
            // given
            var inputCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "notAPart")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "somePart")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "anotherPart")));

            var inputText = KspObjToString (inputCraft);

            // when
            File.WriteAllText (TestFilePath, inputText);
            var returnCode = Program.Main ("remove-parts", "-p", "nonExistingPart", "-i", TestFilePath, "-s");

            // then
            Assert.That (StdOutput.ToString (), Is.EqualTo ("ERROR: No parts removed\n"));
            Assert.That (returnCode, Is.LessThan (0));
        }

        [Test]
        public void PrintsAndReturnsErrorIfNoCraftWithMatchingCraftNameIsFound ()
        {
            // given
            var inputCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "someCraft")))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "anotherCraft")));
            
            var inputText = KspObjToString (inputCrafts);

            // when
            File.WriteAllText (TestFilePath, inputText);
            var returnCode = Program.Main ("remove-parts", "-p", "somePart", "--craft", "nonExistingCraft", "-i", TestFilePath, "-s");

            // then
            Assert.That (StdOutput.ToString (), Is.EqualTo ("ERROR: No craft matching 'nonExistingCraft' found, aborting" + Environment.NewLine));
            Assert.That (returnCode, Is.LessThan (0));
        }

        private String KspObjToString (KspObject obj)
        {
            var token = KspObjectWriter.WriteObject (obj);
            return KspTokenWriter.WriteToken (token, new StringBuilder ()).ToString ();
        }
    }
}
