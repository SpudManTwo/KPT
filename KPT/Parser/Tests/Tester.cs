using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KPT.Parser;
using KPT.Parser.Instructions;
using KPT.Parser.Headers;
using KPT.Parser.Elements;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace KPT.Parser.Tests
{
    static class Tester
    {

        public static string GetTestDir()
        {
            string testDirectory = @"..\..\KPT\Parser\Tests";

            Debug.Assert(Directory.Exists(testDirectory), "Could not find test directory");

            return testDirectory;
        }

        public static void DataBoxTest(Type element, string testFileName)
        {

            FileInfo boxTestFile = new FileInfo(Path.Combine(GetTestDir(), testFileName));
            int dataSize = (int)boxTestFile.Length;

            DataBox boxTest = new DataBox(dataSize);

            FileStream fs = boxTestFile.Open(FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            byte[] inData = br.ReadBytes(dataSize);
            br.BaseStream.Seek(0, SeekOrigin.Begin);

            boxTest.Read(br);

            br.Close();
            fs.Close();

            byte[] outData = new byte[dataSize];

            MemoryStream ms = new MemoryStream(outData);
            BinaryWriter bw = new BinaryWriter(ms);

            boxTest.Write(bw);

            bw.Close();
            ms.Close();

            Debug.Assert(inData.SequenceEqual(outData), "Input/output replication test failed for " + element.ToString());

        }

        public static void InstructionBoxTest(Type element, string testFileName)
        {

            FileInfo boxTestFile = new FileInfo(Path.Combine(GetTestDir(), testFileName));
            int dataSize = (int)boxTestFile.Length;

            InstructionBox boxTest = new InstructionBox(dataSize);

            FileStream fs = boxTestFile.Open(FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            byte[] inData = br.ReadBytes(dataSize);
            br.BaseStream.Seek(0, SeekOrigin.Begin);

            boxTest.Read(br);

            br.Close();
            fs.Close();

            byte[] outData = new byte[dataSize];

            MemoryStream ms = new MemoryStream(outData);
            BinaryWriter bw = new BinaryWriter(ms);

            boxTest.Write(bw);

            bw.Close();
            ms.Close();

            Debug.Assert(inData.SequenceEqual(outData), "Input/output replication test failed for " + element.ToString());

        }

        public static void TestElement(Type element, string testFileName)
        {
            
            // Box has a different construction from the other elements and thus needs its own tests
            if (element == typeof(DataBox))
            {
                DataBoxTest(element, testFileName);
                return;
            }
            else if (element == typeof(InstructionBox))
            {
                InstructionBoxTest(element, testFileName);
                return;
            }

            FileInfo testFile = new FileInfo(Path.Combine(GetTestDir(), testFileName));
            int dataSize = (int)testFile.Length;

            IElement elementTest = Activator.CreateInstance(element) as IElement;

            FileStream fs = testFile.Open(FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            byte[] inData = br.ReadBytes(dataSize);
            br.BaseStream.Seek(0, SeekOrigin.Begin);

            elementTest.Read(br);

            br.Close();
            fs.Close();

            byte[] outData = new byte[dataSize];

            MemoryStream ms = new MemoryStream(outData);
            BinaryWriter bw = new BinaryWriter(ms);

            elementTest.Write(bw);

            bw.Close();
            ms.Close();

            Debug.Assert(inData.SequenceEqual(outData), "Input/output replication test failed for " + element.ToString());

        }

        public static void TestStripTrailingNulls()
        {
            byte[] stringWithNulls = { (byte)'t', (byte)'e', (byte)'s', (byte)'t', 0x00, 0x00, 0x00, 0x00 };
            string stringToStrip = ActiveEncodings.currentEncoding.GetString(stringWithNulls);
            string strippedString = FileIOHelper.StripTrailingNulls(stringToStrip);

            Debug.Assert(strippedString.Equals("test"), "Input/output replication test failed for StripTrailingNulls (string with nulls)");

            string stringWithoutNulls = "test";
            strippedString = FileIOHelper.StripTrailingNulls(stringWithoutNulls);

            Debug.Assert(strippedString.Equals("test"), "Input/output replication test failed for StripTrailingNulls (string without nulls)" +
                "");

        }

        public static void TestReadName()
        {
            string testName = "testname";
            byte[] testNameAsBytes = ActiveEncodings.currentEncoding.GetBytes(testName);
            byte[] fixedLengthArray = new byte[Constants.NAME_LENGTH];
            Array.Copy(testNameAsBytes, 0, fixedLengthArray, 0, testNameAsBytes.Length);

            MemoryStream ms = new MemoryStream(fixedLengthArray);
            BinaryReader br = new BinaryReader(ms);

            string returnedName = FileIOHelper.ReadName(br);

            br.Close();
            ms.Close();

            Debug.Assert(returnedName.Equals(testName), "Input/output replication test failed for ReadName");

        }

        public static void TestStringCollection()
        {
            var testCollection = new StringCollection();

            string testID = "test_id";
            string testString = "test_string";
            string testStringTwo = "test_string_two";

            testCollection.AddString(testID, testString);
            string testNewString = testCollection.GetString(testID);

            Debug.Assert(testString.Equals(testNewString), "Input/output replication test failed for StringCollection");

            bool exceptionCaught;

            try
            {
                // Adding same string with same ID
                testCollection.AddString(testID, testString);
                exceptionCaught = false;
            }
            catch (Exception e)
            {
                exceptionCaught = true;
            }

            Debug.Assert(exceptionCaught == false, "Same ID same string test failed for StringCollection");

            try
            {
                // Adding different string with same ID
                testCollection.AddString(testID, testStringTwo);
                exceptionCaught = false;
            }
            catch (Exception e)
            {
                exceptionCaught = true;
            }

            Debug.Assert(exceptionCaught == true, "Same ID different string test failed for StringCollection");

            try
            {
                // Trying to access non-existent string
                string nonExistentID = "afesrgrt3qr\aefgsgggaftr3qr4q4";
                string nonExistentString = testCollection.GetString(nonExistentID);
                exceptionCaught = false;
            }
            catch (Exception e)
            {
                exceptionCaught = true;
            }

            Debug.Assert(exceptionCaught == true, "Non-existent ID test failed for StringCollection");

            var testCollectionWithID = new StringCollection("TEST");

            Debug.Assert(testCollectionWithID.GenerateNewID() == "$TEST_0", "ID generation test failed for StringCollection"); // this test does not account for variable idPrefix etc. which are technicallty not supported at the moment

        }

        public static void RunTests()
        {

            TestStripTrailingNulls();
            TestReadName();

            TestElement(typeof(ChoiceBar), "ChoiceBarTest.bin");

            TestElement(typeof(StCp_Header), "StCp_HeaderTest.bin");

            TestElement(typeof(SpriteInfo), "SpriteInfoTest.bin");

            TestElement(typeof(DataBox), "DataBoxTest.bin");
            TestElement(typeof(InstructionBox), "InstructionBoxTest.bin");
            TestElement(typeof(ChoiceDialog), "ChoiceDialogTest.bin");
            TestElement(typeof(LocationCard), "LocationCardTest.bin");
            TestElement(typeof(ShowImage), "ShowImageTest.bin");
            TestElement(typeof(U_501), "U_501Test.bin");
            TestElement(typeof(BasicTextBox), "U_500Test.bin");
            TestElement(typeof(U_502), "U_502Test.bin");
            TestElement(typeof(HideTextBox), "HideTextBoxTest.bin");

            TestStringCollection();

        }

    }
}
