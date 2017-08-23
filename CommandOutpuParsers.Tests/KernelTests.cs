using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WinDbgX.CommandOutput;
using System.Linq;

namespace CommandOutpuParsers.Tests {
	[TestClass]
	public class KernelTests {
		[TestMethod]
		public void TestProcessCommand() {
			var output = File.ReadAllText(@"..\..\processOutput.txt");
			var parser = new KernelProcessOutputParser();
			var list = parser.ParseCommandOutput("!process 0 0 ", output).ToArray();
			Assert.IsTrue(list.Length == 24);
		}
	}
}
