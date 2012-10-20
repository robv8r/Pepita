﻿
#if (DEBUG)
using System;
using System.IO;
using System.Threading;
using Moq;
using NUnit.Framework;

[TestFixture]
public class TaskFileReplacerTests
{

    [Test]
    public void AddFileNotExists()
    {
        string tempFileName = null;
        try
        {
            tempFileName = Path.GetTempFileName();
            var errorDisplayer = new Mock<MessageDisplayer>().Object;
            var taskFileReplacer = new TaskFileReplacer(errorDisplayer, null)
                                       {
                                           TaskFilePath = tempFileName
                                       };
            taskFileReplacer.AddFile(@"C:\Foo");
            Assert.AreEqual("C:\\Foo\n", FileReader.Read(taskFileReplacer.TaskFilePath));
        }
        finally
        {
            if (tempFileName != null && File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
        }
    }

    [Test]
    public void AddFileNotExists2()
    {
        string tempFileName = null;
        try
        {
            var errorDisplayer = new Mock<MessageDisplayer>().Object;
            tempFileName = Path.GetTempFileName();
            var taskFileReplacer = new TaskFileReplacer(errorDisplayer, null)
                                       {
                                           TaskFilePath = tempFileName
                                       };
            taskFileReplacer.AddFile(@"C:\Foo");
            taskFileReplacer.AddFile(@"C:\Foo2");
            Assert.AreEqual("C:\\Foo\nC:\\Foo2\n", FileReader.Read(taskFileReplacer.TaskFilePath));
        }
        finally
        {
            if (tempFileName != null && File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
        }
    }

    [Test]
    public void CheckForFilesToUpdateNotExists()
    {
        string tempFileName = null;
        try
        {
            tempFileName = Path.GetTempFileName();


            var errorDisplayer = new Mock<MessageDisplayer>().Object; var taskFileReplacer = new TaskFileReplacer(errorDisplayer, null)
                                       {
                                           TaskFilePath = tempFileName
                                       };
            taskFileReplacer.AddFile(@"C:\Foo");
            taskFileReplacer.AddFile(@"C:\Foo2");
            Thread.Sleep(300);
            taskFileReplacer.CheckForFilesToUpdate();
            Thread.Sleep(300);
            Assert.AreEqual("", FileReader.Read(taskFileReplacer.TaskFilePath));
        }
        finally
        {
            if (tempFileName != null && File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
        }
    }

    [Test]
    public void CheckForFilesToUpdateExportFails()
    {
        string tempFileName = null;
        try
        {
            tempFileName = Path.GetTempFileName();

            var errorDisplayer = new Mock<MessageDisplayer>().Object;
            var taskFileReplacer = new TaskFileReplacer(errorDisplayer, new ContentsFinder())
                                       {
                                           TaskFilePath = tempFileName
                                       };

            var dir1 = Environment.CurrentDirectory + @"..\..\..\Dir1";
            taskFileReplacer.AddFile(dir1);
            var dir2 = Environment.CurrentDirectory + @"..\..\..\Dir2";
            taskFileReplacer.AddFile(dir2);

            taskFileReplacer.CheckForFilesToUpdate();
            Assert.AreEqual(dir1 + "\n" + dir2 + "\n", FileReader.Read(taskFileReplacer.TaskFilePath));
        }
        finally
        {
            if (tempFileName != null && File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
        }
    }
}
#endif
