﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using KSPPartRemover.KspObjects;
using KSPPartRemover.KspObjects.Format;
using KSPPartRemover.Extension;

namespace KSPPartRemover.Tests.Integration
{
	public class ProgramTest
	{
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
			Program.Main ();

			// then
			Assert.That (StdOutput.ToString (), Is.StringContaining ("ERROR: "));
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
			
			var inputText = KspObjectWriter.ToString (inputCraft);
			var expectedResult = KspObjectWriter.ToString (expectedCraft);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "0", "-i", "input.txt", "-s");

			// then
			Assert.That (StdOutput.ToString (), Is.EqualTo (expectedResult));
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
			
			var inputText = KspObjectWriter.ToString (inputCraft);
			var expectedResult = KspObjectWriter.ToString (expectedCraft);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "fuelTank", "-i", "input.txt", "-s");

			// then
			Assert.That (StdOutput.ToString (), Is.EqualTo (expectedResult));
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
					.AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank"))));

			var expectedCrafts = new KspObject ("GAME")
				.AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft1"))
					.AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut"))))
				.AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft2"))
					.AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
					.AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
					.AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank"))))
				.AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft3"))
					.AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut"))));
					
			var inputText = KspObjectWriter.ToString (inputCrafts);
			var expectedResult = KspObjectWriter.ToString (expectedCrafts);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "fuelTank", "-c", ".*raft[1,3]", "-i", "input.txt", "-s");

			// then
			Assert.That (StdOutput.ToString (), Is.EqualTo (expectedResult));
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
			
			var inputText = KspObjectWriter.ToString (inputCrafts);
			var expectedResult = "someCraft" + Environment.NewLine + "anotherCraft" + Environment.NewLine;

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("list-crafts", "-c", ".*Craft", "-i", "input.txt");

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
					.AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank"))))
				.AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "ignored"))
					.AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "somePart"))));
			
			var inputText = KspObjectWriter.ToString (inputCrafts);
			var expectedResult =
				"someCraft:" + Environment.NewLine +
				"\t[0] fuelTank" + Environment.NewLine +
				"\t[1] strut" + Environment.NewLine +
				"anotherCraft:" + Environment.NewLine +
				"\t[0] strut" + Environment.NewLine +
				"\t[1] fuelTank" + Environment.NewLine;

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("list-parts", "-c", ".*Craft", "-i", "input.txt");

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
			
			var inputText = KspObjectWriter.ToString (inputCraft);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "2", "-i", "input.txt");

			// then
			Assert.That (StdOutput.ToString (), Is.StringContaining ("No part with id=2 found"));
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

			var inputText = KspObjectWriter.ToString (inputCraft);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "nonExistingPart", "-i", "input.txt");

			// then
			Assert.That (StdOutput.ToString (), Is.StringContaining ("No parts with a name of 'nonExistingPart' found"));
			Assert.That (returnCode, Is.LessThan (0));
		}

		[Test]
		public void PrintsAndReturnsErrorIfNoCraftWithMatchingCraftNameIsFound ()
		{
			// given
			var inputCrafts = new KspObject ("GAME")
				.AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "someCraft")))
				.AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "anotherCraft")));
			
			var inputText = KspObjectWriter.ToString (inputCrafts);

			// when
			File.WriteAllText ("input.txt", inputText);
			var returnCode = Program.Main ("remove-part", "somePart", "--craft", "nonExistingCraft", "-i", "input.txt");

			// then
			Assert.That (StdOutput.ToString (), Is.StringContaining ("No craft matching 'nonExistingCraft' found, aborting"));
			Assert.That (returnCode, Is.LessThan (0));
		}
	}
}
